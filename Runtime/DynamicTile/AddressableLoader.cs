using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PLATEAU.DynamicTile
{
    public class AddressableLoader
    {
        /// <summary>
        /// Addressからアセットをインスタンス化します
        /// </summary>
        public async Task<GameObject> InstantiateAssetAsync(string address, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(address, parent);
            while (!handle.IsDone)
            {
                await Task.Yield();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                handle.Result.name = address;
                return handle.Result;
            }
            else
            {
                Debug.LogError($"アセットのロードに失敗しました: {address}");
                return null;
            }
        }

        /// <summary>
        /// カタログファイルをロードし、Addressを読み込みます
        /// </summary>
        /// <param name="catalogPath">カタログファイルのパス</param>
        /// <param name="label">ラベル</param>
        /// <returns>ロードされたGameObjectのリスト</returns>
        public async Task<List<string>> LoadCatalogAsync(string catalogPath, string label)
        {
            var addresses = new List<string>();
            try
            {
                // パスを正規化（バックスラッシュをスラッシュに変換）
                catalogPath = catalogPath.Replace('\\', '/');

                // file://プロトコルを追加
                if (!catalogPath.StartsWith("file://"))
                {
                    catalogPath = "file://" + catalogPath;
                }

                // カタログファイルをロード
                var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath);
                while (!catalogHandle.IsDone)
                {
                    await Task.Yield();
                }

                if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
                    return addresses;
                }
                
                // カタログからアセットのアドレスを取得
                IList<IResourceLocation> locations;
                if (catalogHandle.Result.Locate(label, typeof(GameObject), out locations) && locations.Count > 0)
                {
                    // 各アセットのアドレスを取得
                    foreach (var location in locations)
                    {
                        addresses.Add(location.PrimaryKey);
                    }
                }
                else
                {
                    Debug.LogError($"アセットのアドレスを取得できませんでした");
                }
                // カタログ用のhandleの解放
                Addressables.Release(catalogHandle);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"カタログのロード中にエラーが発生しました: {ex.Message}");
            }
            return addresses;
        }
        
        public async Task<List<string>> LoadLocalAddresses(string label)
        {
            var addresses = new List<string>();
            try
            {
                // ラベルでリソースロケーションを非同期取得
                var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
                await locationsHandle.Task;
                if (locationsHandle.Status == AsyncOperationStatus.Succeeded && locationsHandle.Result.Count > 0)
                {
                    addresses = locationsHandle.Result.Select(x => x.PrimaryKey).ToList();
                }
                else
                {
                    Debug.LogError($"アセットのアドレスを取得できませんでした: {label}");
                }
                
                // 取得したリソースの解放
                Addressables.Release(locationsHandle);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"アセットのロード中にエラーが発生しました: {ex.Message}");
            }
            return addresses;
        }
    }
} 