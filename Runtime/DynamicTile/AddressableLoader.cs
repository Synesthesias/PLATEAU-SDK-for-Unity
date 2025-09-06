using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Addressableを使用してカタログとMeta情報をロードするクラス。
    /// </summary>
    public class AddressableLoader
    {
        // DynamicTileのラベル名
        private const string DynamicTileLabelName = "DynamicTile";
        internal const string AddressableLocalBuildFolderName = "PLATEAUBundles";

        /// <summary>
        /// 非同期処理を制御します
        /// </summary>
        private async Task<T> WaitForCompletionAsync<T>(AsyncOperationHandle<T> handle)
        {
            if (!handle.IsValid())
            {
                throw new Exception("AsyncOperationHandle is invalid.");
            }
            
            // エディタプレイ以外は同期で処理
            if (Application.isEditor && !Application.isPlaying)
            {
                handle.WaitForCompletion();
            }
            else
            {
                await handle.Task;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new System.Exception($"Operation failed with status: {handle.Status}");
            }

            return handle.Result;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public async Task<PLATEAUDynamicTileMetaStore> InitializeAsync(string catalogPath)
        {
            // ランタイム（プレイヤー）では、Assets/StreamingAssets を実ディレクトリにマッピング
            if (!Application.isEditor && !string.IsNullOrEmpty(catalogPath))
            {
                var normalizedPath = catalogPath.Replace('\\', '/');
                const string assetsStreamingPrefix = "Assets/StreamingAssets/";
                if (normalizedPath.StartsWith(assetsStreamingPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var subPath = normalizedPath.Substring(assetsStreamingPrefix.Length);
                    var mapped = Path.Combine(Application.streamingAssetsPath, subPath).Replace('\\', '/');
                    catalogPath = mapped;
                }
            }
            var init = Addressables.InitializeAsync(false);
            await WaitForCompletionAsync(init);
            if (init.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(init);
                Debug.LogError("Addressablesの初期化に失敗しました。");
                return null;
            }


            Addressables.Release(init);

            // カタログをロード
            string catalogPathToUse;
            if (!catalogPath.StartsWith("Assets/"))
            {
                // プロジェクトの外からカタログを取得する場合
                catalogPathToUse = catalogPath;
            }
            else
            {
                // プロジェクトの中からカタログを取得します。
                // プロジェクト内であれば一見カタログなど求めなくてもロード可能に思えますが、
                // 実際はAddressables.LoadContentCatalogAsync(catalogPath)からカタログを読み直さないと処理を2回したときに1回目のデータが残る不具合が起きます。
                // Localビルド(StreamingAssets) からカタログを探索
                var directoryFromAssets = Path.GetDirectoryName(catalogPath);
                if (directoryFromAssets == null)
                {
                    Debug.LogError("failed to find catalog.");
                    return null;
                }
                var folderStreaming = Path.GetFullPath(directoryFromAssets);

                if (!Directory.Exists(folderStreaming))
                {
                    Debug.LogError("folder not found: " + folderStreaming);
                }

                var catalogFiles = TileCatalogSearcher.FindCatalogFiles(folderStreaming, true);

                if (catalogFiles.Length == 0)
                {
                    Debug.LogError("catalog file is not found.");
                }

                catalogPathToUse = catalogFiles.FirstOrDefault();
            }

            await LoadCatalog(catalogPathToUse);
            // MetaStoreアドレスを自動解決（同一カタログディレクトリ配下のものを優先）
            var metaStoreAddress = await ResolveMetaStoreAddressAsync(catalogPathToUse);
            if (string.IsNullOrEmpty(metaStoreAddress))
            {
                return null;
            }

            // meta情報をロード
            var metaStore = LoadMetaStore(metaStoreAddress);
            if (metaStore == null)
            {
                return null;
            }

            return metaStore;
        }

        /// <summary>
        /// Addressables から PLATEAUDynamicTileMetaStore の Address を自動解決します。
        /// 同一カタログディレクトリ配下のロケーションを優先して選択します。
        /// </summary>
        private async Task<string> ResolveMetaStoreAddressAsync(string catalogPath)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(
                DynamicTileLabelName, typeof(PLATEAUDynamicTileMetaStore));
            try
            {
                await WaitForCompletionAsync(locationsHandle);
                var allLocations = locationsHandle.Result;

                // カタログのディレクトリを正規化
                var catalogDir = Path.GetDirectoryName(catalogPath);
                if (string.IsNullOrEmpty(catalogDir))
                {
                    Debug.LogError("カタログディレクトリの取得に失敗しました。");
                    return null;
                }
                var catalogDirFull = Path.GetFullPath(catalogDir).Replace('\\', '/');
                var catalogDirName = new DirectoryInfo(catalogDirFull).Name;

                bool isInAssets = PathUtil.IsSubDirectoryOfAssets(catalogPath);
                if (isInAssets)
                {
                    var sanitizedDir = System.Text.RegularExpressions.Regex.Replace(catalogDirName, @"[^\w\-_]", "_");
                    sanitizedDir = sanitizedDir.Replace("PLATEAUCityObjectGroup_", "");
                    var expectedAddress = $"PLATEAUTileMeta_{sanitizedDir}";
                    return expectedAddress;
                }
                
                
                
                // ディレクトリ名でマッチ（アプリビルド後にフルパスは変わるが、親ディレクトリ名は変わらない）
                var candidates = allLocations
                    .Where(loc =>
                        loc != null && !string.IsNullOrEmpty(loc.PrimaryKey) &&
                        InternalIdContainsDirectoryName(loc, catalogDirName)
                    )
                    .ToList();

                if (candidates.Count == 0)
                {
                    Debug.LogError($"PLATEAUDynamicTileMetaStore がカタログから見つかりませんでした。Addressables のビルド/ラベル設定/カタログ指定を確認してください。");
                    return null;
                }

                if (candidates.Count > 1)
                {
                    Debug.LogWarning($"複数の PLATEAUDynamicTileMetaStore が見つかりました（同一カタログ配下を優先）。先頭の1つを使用します。 count={candidates.Count}");
                }

                return candidates[0].PrimaryKey;
            }
            catch (Exception ex)
            {
                Debug.LogError($"MetaStore アドレスの自動解決に失敗しました: {ex}");
                return null;
            }
            finally
            {
                if (locationsHandle.IsValid())
                {
                    Addressables.Release(locationsHandle);
                }
            }
        }

        /// <summary>
        /// IResourceLocation が内部ID文字列上で指定ディレクトリ名を含むか（依存を辿りながら）を判定します。
        /// アプリビルドでフルパスは変わるが、親ディレクトリ名は変わらないため。
        /// </summary>
        private bool InternalIdContainsDirectoryName(IResourceLocation rootLocation, string dirName)
        {
            if (string.IsNullOrEmpty(dirName)) return false;
            var queue = new Queue<IResourceLocation>();
            var visited = new HashSet<IResourceLocation>();
            queue.Enqueue(rootLocation);
            while (queue.Count > 0)
            {
                var loc = queue.Dequeue();
                if (loc == null || visited.Contains(loc)) continue;
                visited.Add(loc);

                var internalId = loc.InternalId;
                if (!string.IsNullOrEmpty(internalId))
                {
                    var normalized = Path.GetFileName(internalId.Replace('\\', '/'));
                    normalized = normalized.Replace("PLATEAUTileMeta_", "");
                    if (normalized.Contains(dirName))
                    {
                        return true;
                    }
                }

                var deps = loc.Dependencies;
                if (deps != null)
                {
                    foreach (var d in deps)
                    {
                        if (d != null && !visited.Contains(d)) queue.Enqueue(d);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// カタログファイルをロードします
        /// </summary>
        /// <param name="catalogPath">カタログファイルのパス</param>
        private async Task LoadCatalog(string catalogPath)
        {

            // パスを正規化（バックスラッシュをスラッシュに変換）
            catalogPath = catalogPath.Replace('\\', '/');

            if (string.IsNullOrEmpty(catalogPath) || !File.Exists(catalogPath))
            {
                Debug.LogError($"カタログファイルが見つかりません: {catalogPath}");
                return;
            }
            
            // カタログファイルのロード前に古いロケーターを削除しておかないと、
            // 同じフォルダを対象に2回タイル化したケースで1回目のデータが読み込まれてしまう不具合が起きます。
            Addressables.ClearResourceLocators();

            // カタログファイルをロード
            var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath);
            await WaitForCompletionAsync(catalogHandle);

            if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
            }

            Addressables.Release(catalogHandle);
        }


        /// <summary>
        /// meta情報をロードします。
        /// </summary>
        /// <returns></returns>
        private PLATEAUDynamicTileMetaStore LoadMetaStore(string metaStoreAddress)
        {
            var handle = Addressables.LoadAssetAsync<PLATEAUDynamicTileMetaStore>(metaStoreAddress);
            handle.WaitForCompletion(); // 必ず同期的にする
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load PLATEAUDynamicTileMetaStore: " + metaStoreAddress);
                Addressables.Release(handle);
                return null;
            }

            var ret = handle.Result;
            Addressables.Release(handle);
            return ret;
        }
    }
}