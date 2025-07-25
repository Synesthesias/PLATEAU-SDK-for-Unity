using PLATEAU.Util;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Addressableを使用してカタログとMeta情報をロードするクラス。
    /// </summary>
    public class AddressableLoader
    {
        // DynamicTileのラベル名
        private const string DynamicTileLabelName = "DynamicTile";

        /// <summary>
        /// 非同期処理を制御します
        /// </summary>
        private async Task<T> WaitForCompletionAsync<T>(DisposableAsyncOperationHandle<T> handle)
        {
            // エディタプレイ以外は同期で処理
            if (Application.isEditor && !Application.isPlaying)
            {
                handle.Handle.WaitForCompletion();
            }
            else
            {
                await handle.Handle.Task;
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
        /// <param name="catalogPath"></param>
        /// <returns></returns>
        public async Task<PLATEAUDynamicTileMetaStore> InitializeAsync(string catalogPath)
        {
            using var init = Addressables.InitializeAsync(false).ToDisposable();
            await WaitForCompletionAsync(init);
            if (init.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Addressablesの初期化に失敗しました。");
                return null;
            }

            if (!string.IsNullOrEmpty(catalogPath))
            {
                // カタログから取得
                var metaStorePath = await LoadCatalog(catalogPath, DynamicTileLabelName);
                if (metaStorePath.IsNullOrEmpty())
                {
                    Debug.LogError("カタログのロードに失敗しました。アドレスが見つかりません。");
                    return null;
                }

                // meta情報をロード
                var metaStore = LoadMetaStore(metaStorePath);
                if (metaStore == null)
                {
                    return null;
                }

                return metaStore;
            }
            else
            {
                // catalogPathが空なのでUnityプロジェクト内のAddressable Groupから取得
                using var handle =
                    Addressables.LoadResourceLocationsAsync(DynamicTileLabelName, typeof(PLATEAUDynamicTileMetaStore)).ToDisposable();
                await WaitForCompletionAsync(handle);

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result.Count == 0)
                {
                    Debug.LogError($"ラベル '{DynamicTileLabelName}' の PLATEAUDynamicTileMetaStore が見つかりません");
                    return null;
                }

                // 最初に見つかったメタストアを直接ロード
                using var metaStoreHandle = Addressables.LoadAssetAsync<PLATEAUDynamicTileMetaStore>(handle.Result[0]).ToDisposable();
                await WaitForCompletionAsync(metaStoreHandle);

                if (metaStoreHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError("PLATEAUDynamicTileMetaStore のロードに失敗しました");
                    return null;
                }

                var metaStore = metaStoreHandle.Result;
                return metaStore;
            }
        }

        /// <summary>
        /// file://プロトコルを除去します
        /// </summary>
        private string RemoveFileProtocol(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return path.StartsWith("file://") ? path.Substring("file://".Length) : path;
        }

        /// <summary>
        /// カタログファイルをロードします
        /// </summary>
        /// <param name="catalogPath">カタログファイルのパス</param>
        /// <param name="label">ラベル</param>
        /// <returns>ロードされたGameObjectのリスト</returns>
        private async Task<string> LoadCatalog(string catalogPath, string label)
        {
            var bundlePath = string.Empty;

            // パスを正規化（バックスラッシュをスラッシュに変換）
            catalogPath = catalogPath.Replace('\\', '/');

            // bundlePathを保持
            bundlePath = Path.GetDirectoryName(catalogPath);

            // file://プロトコルを追加
            if (!catalogPath.StartsWith("file://"))
            {
                catalogPath = "file://" + catalogPath;
            }

            var removedProtocolPath = RemoveFileProtocol(catalogPath);
            if (string.IsNullOrEmpty(removedProtocolPath) || !File.Exists(removedProtocolPath))
            {
                Debug.LogError($"カタログファイルが見つかりません: {removedProtocolPath}");
                return null;
            }

            // カタログファイルをロード
            using var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath).ToDisposable();
            await WaitForCompletionAsync(catalogHandle);

            if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
                return null;
            }

            // DynamicTileメタ情報のアドレスを取得
            if (catalogHandle.Result.Locate(label, typeof(PLATEAUDynamicTileMetaStore),
                    out var scriptableObjectLocations))
            {
                foreach (var location in scriptableObjectLocations)
                {
                    if (location.Dependencies.Count <= 0)
                    {
                        continue;
                    }

                    if (bundlePath != null)
                    {
                        // メタ情報のパスを返す
                        foreach (var iResourceLocation in location.Dependencies)
                        {
                            var fullPath = iResourceLocation.InternalId;
                            var dirName = Path.GetDirectoryName(fullPath);

                            // ディレクトリ名が一致するか確認
                            if (dirName != null && dirName.Contains(bundlePath))
                            {
                                return iResourceLocation.InternalId;
                            }
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// パスをもとに、meta情報をロードします。
        /// </summary>
        /// <returns></returns>
        private PLATEAUDynamicTileMetaStore LoadMetaStore(string metaStorePath)
        {
            PLATEAUDynamicTileMetaStore metaStore = null;
            try
            {
                // ファイルの存在確認
                if (!File.Exists(metaStorePath))
                {
                    Debug.LogError($"ファイルが存在しません: {metaStorePath}");
                    return null;
                }

                // Addressable経由だとロードできないので、直接ファイルシステムから読み込み
                var bundle = AssetBundle.LoadFromFile(metaStorePath);
                if (bundle != null)
                {
                    // バンドル内のアセット名をログ出力
                    string[] assetNames = bundle.GetAllAssetNames();
                    if (assetNames.Length == 0)
                    {
                        Debug.LogWarning($"バンドル内にアセットが見つかりません: {metaStorePath}");
                        return null;
                    }

                    metaStore = bundle.LoadAsset<PLATEAUDynamicTileMetaStore>(assetNames[0]);
                    bundle.Unload(false);
                }
                else
                {
                    Debug.LogError($"MetaStoreのロードに失敗しました: {metaStorePath}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"MetaStoreのロード中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
            }

            return metaStore;
        }
    }
}