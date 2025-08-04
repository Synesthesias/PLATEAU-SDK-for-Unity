using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Linq;


using System.IO;
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
        public async Task<PLATEAUDynamicTileMetaStore> InitializeAsync(string catalogPath, string metaStoreAddress)
        
        {
            var init = Addressables.InitializeAsync(false);
            await WaitForCompletionAsync(init);
            if (init.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(init);
                Debug.LogError("Addressablesの初期化に失敗しました。");
                return null;
            }


            Addressables.Release(init);

            // カタログから取得
            string catalogPathToUse;
            if (!catalogPath.StartsWith("Assets/"))
            {
                // プロジェクトの外からカタログを取得する場合
                catalogPathToUse = catalogPath;
            }
            else
            {
                // 設定されたcatalogPathが空のときはプロジェクトの中からカタログを取得します。
                // プロジェクト内であれば一見カタログパスなど求めなくてもロード可能に思えますが、
                // 実際はAddressables.LoadContentCatalogAsync(catalogPath)からカタログを読み直さないと処理を2回したときに1回目のデータが残る不具合が起きます。
                // Localビルド(StreamingAssets) からカタログを探索
                var folderStreaming = Path.Combine(Application.streamingAssetsPath, AddressableLocalBuildFolderName);

                if (!Directory.Exists(folderStreaming))
                {
                    Debug.LogError("folder not found: " + folderStreaming);
                }

                var catalogFiles = Directory.GetFiles(folderStreaming, "catalog_*.json", SearchOption.AllDirectories);

                if (catalogFiles.Length == 0)
                {
                    Debug.LogError("catalog file is not found.");
                }

                catalogPathToUse = catalogFiles.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
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