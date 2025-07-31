using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        /// <returns></returns>
        public async Task<PLATEAUDynamicTileMetaStore> InitializeAsync(string catalogPath, string metaStoreAddress)
        
        {
            using var init = Addressables.InitializeAsync(false).ToDisposable();
            await WaitForCompletionAsync(init);
            if (init.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Addressablesの初期化に失敗しました。");
                return null;
            }

            // カタログから取得
            string catalogPathToUse;
            if (!catalogPath.IsNullOrEmpty())
            {
                // プロジェクトの外からカタログを取得する場合
                catalogPathToUse = catalogPath;
            }
            else
            {
                // 設定されたcatalogPathが空のときはプロジェクトの中からカタログを取得します。
                // プロジェクト内であれば一見カタログパスなど求めなくてもロード可能に思えますが、
                // 実際はAddressables.LoadContentCatalogAsync(catalogPath)からカタログを読み直さないと処理を2回したときに1回目のデータが残る不具合が起きます。
#if UNITY_EDITOR
                var folderProject = Path.GetDirectoryName(Application.dataPath);
                var folderBuild = Path.GetFullPath(Path.Combine(folderProject, Addressables.BuildPath));

                if (!Directory.Exists(folderBuild))
                {
                    Debug.LogError("folder not found: " + folderBuild);
                    return null;
                }

                var catalogFiles = Directory.GetFiles(folderBuild, "catalog_*.json", SearchOption.AllDirectories);

                if (catalogFiles.Length == 0)
                {
                    Debug.LogError("catalog file is not found.");
                    return null;
                }

                catalogPathToUse = catalogFiles.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
#else
                    var folderRuntime = Addressables.RuntimePath;
                    if (!Directory.Exists(folderRuntime))
                    {
                        Debug.LogError("folder not found: " + folderRuntime);
                        return null;
                    }
                    var files = Directory.GetFiles(folderRuntime, "catalog_*.json", SearchOption.AllDirectories);
                    if (files.Length == 0)
                    {
                        Debug.LogError("catalog file is not found.");
                        return null;
                    }
                    catalogPathToUse = files.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
#endif
            }

            await LoadCatalog(catalogPathToUse);
            if (metaStoreAddress.IsNullOrEmpty())
            {
                Debug.LogError("MetaStoreのアドレスがありません。");
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
        private async Task LoadCatalog(string catalogPath)
        {

            // パスを正規化（バックスラッシュをスラッシュに変換）
            catalogPath = catalogPath.Replace('\\', '/');

            // file://プロトコルを追加
            if (!catalogPath.StartsWith("file://"))
            {
                catalogPath = "file://" + catalogPath;
            }

            var removedProtocolPath = RemoveFileProtocol(catalogPath);
            if (string.IsNullOrEmpty(removedProtocolPath) || !File.Exists(removedProtocolPath))
            {
                Debug.LogError($"カタログファイルが見つかりません: {removedProtocolPath}");
                return;
            }

            // カタログファイルをロード
            using var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath).ToDisposable();
            await WaitForCompletionAsync(catalogHandle);

            if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
                return;
            }
        }


        /// <summary>
        /// meta情報をロードします。
        /// </summary>
        /// <returns></returns>
        private PLATEAUDynamicTileMetaStore LoadMetaStore(string metaStoreAddress)
        {
            using var handle = Addressables.LoadAssetAsync<PLATEAUDynamicTileMetaStore>(metaStoreAddress).ToDisposable();
            WaitForCompletionAsync(handle).ContinueWithErrorCatch();
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load PLATEAUDynamicTileMetaStore: " + metaStoreAddress);
                return null;
            }

            return handle.Result;
            // PLATEAUDynamicTileMetaStore metaStore = null;
            // try
            // {
            //     // ファイルの存在確認
            //     if (!File.Exists(metaStorePath))
            //     {
            //         Debug.LogError($"ファイルが存在しません: {metaStorePath}");
            //         return null;
            //     }
            //
            //     // Addressable経由だとロードできないので、直接ファイルシステムから読み込み
            //     var bundle = AssetBundle.LoadFromFile(metaStorePath);
            //     if (bundle != null)
            //     {
            //         // バンドル内のアセット名をログ出力
            //         string[] assetNames = bundle.GetAllAssetNames();
            //         if (assetNames.Length == 0)
            //         {
            //             Debug.LogWarning($"バンドル内にアセットが見つかりません: {metaStorePath}");
            //             return null;
            //         }
            //
            //         metaStore = bundle.LoadAsset<PLATEAUDynamicTileMetaStore>(assetNames[0]);
            //         bundle.Unload(false);
            //     }
            //     else
            //     {
            //         Debug.LogError($"MetaStoreのロードに失敗しました: {metaStorePath}");
            //         return null;
            //     }
            // }
            // catch (System.Exception ex)
            // {
            //     Debug.LogError($"MetaStoreのロード中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
            // }
            //
            // return metaStore;
        }
    }
}