using PLATEAU.Util.Async;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace PLATEAU.DynamicTile
{
    public class AddressableLoader
    {
        private const string DynamicTileLabelName = "DynamicTile";
        private Dictionary<string, string> bundlePathMap = new ();
        private string bundlePath;

        public async Task<PLATEAUDynamicTileMetaStore> Initialize(string catalogPath)
        {
            // 明示的に初期化しておくと安心
            // Addressables.InitializeAsync().Completed += handle =>
            // {
            //     Debug.Log("Addressables Initialized!");
            // };
            
            var init = Addressables.InitializeAsync();
            init.WaitForCompletion();
            // await init.Task.ContinueWithErrorCatch();
            
            Debug.Log("AddressableLoader Initialize called");

            // カタログをロード
            if (!string.IsNullOrEmpty(catalogPath))
            {
                Debug.Log($"AddressableLoader Initialize called with catalogPath: {catalogPath}");
                var addresses = LoadCatalogAsync(catalogPath, DynamicTileLabelName).ContinueWithErrorCatch();
            }

            // meta情報をロード
            var metaStore = await LoadMetaStoreAsync().ContinueWithErrorCatch();
            if (metaStore == null)
            {
                return null;
            }

            return metaStore;
        }
        
        /// <summary>
        /// Clear処理
        /// </summary>
        public void Clear()
        {
            bundlePathMap.Clear();
            bundlePath = string.Empty;
        }
        
        /// <summary>
        /// Addressからアセットをインスタンス化します
        /// 注意: 返されたGameObjectは呼び出し側でAddressables.ReleaseInstanceを使用して解放する必要があります
        /// </summary>
        public async Task<GameObject> InstantiateAssetAsync(string address, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(address, parent);
            handle.WaitForCompletion();
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                //Debug.Log($"アセットのロードに成功しました: {address}");
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
                
                // bundlePathを保持
                bundlePath = Path.GetDirectoryName(catalogPath);

                // file://プロトコルを追加
                if (!catalogPath.StartsWith("file://"))
                {
                    catalogPath = "file://" + catalogPath;
                }

                // カタログファイルをロード
                // var locationHadh = Addressables.CreateCatalogLocationWithHashDependencies<TextDataProvider>(catalogPath);
                // Debug.Log($"AddressableLoader LoadCatalog called {locationHadh.PrimaryKey}");
                var catalogHandle = Addressables.LoadContentCatalogAsync(catalogPath);
                catalogHandle.WaitForCompletion();
                // Debug.Log($"AddressableLoader LoadCatalog called 2 {catalogHandle.Result}");
                
                if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"カタログファイルのロードに失敗しました: {catalogPath}");
                    Addressables.Release(catalogHandle);
                    return addresses;
                }

                // カタログからアセットのアドレスを取得
                bool hasGameObjects = catalogHandle.Result.Locate(label, typeof(GameObject), out var gameObjectLocations);
                bool hasScriptableObjects = catalogHandle.Result.Locate(label, typeof(ScriptableObject), out var scriptableObjectLocations);

                // GameObjectのアドレスを取得
                if (hasGameObjects && gameObjectLocations.Count > 0)
                {
                    foreach (var location in gameObjectLocations)
                    {
                        addresses.Add(location.PrimaryKey);
                        if (location.Dependencies.Count <= 0)
                        {
                            continue;
                        }
                        Debug.Log($"GameObjectのアドレス: {location.PrimaryKey}, バンドルパス: {location.Dependencies[0].PrimaryKey}");
                        
                        // バンドルパスをマップに追加
                        if (!bundlePathMap.ContainsKey(location.PrimaryKey))
                        {
                            bundlePathMap.Add(location.PrimaryKey, location.Dependencies[0].PrimaryKey);
                        }
                    }
                }

                // ScriptableObjectのアドレスを取得
                if (hasScriptableObjects && scriptableObjectLocations.Count > 0)
                {
                    foreach (var location in scriptableObjectLocations)
                    {
                        addresses.Add(location.PrimaryKey);
                        if (location.Dependencies.Count <= 0)
                        {
                            continue;
                        }
                        Debug.Log($"ScriptableObjectのアドレス: {location.PrimaryKey}, バンドルパス: {location.Dependencies[0].PrimaryKey}");
                        // バンドルパスをマップに追加
                        if (!bundlePathMap.ContainsKey(location.PrimaryKey))
                        {
                            bundlePathMap.Add(location.PrimaryKey, location.Dependencies[0].PrimaryKey);
                        }
                        else
                        {
                            bundlePathMap[location.PrimaryKey] = location.Dependencies[0].PrimaryKey;
                        }
                    }
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
                locationsHandle.WaitForCompletion();
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

        /// <summary>
        /// meta情報をロードします。
        /// </summary>
        /// <returns></returns>
        private async Task<PLATEAUDynamicTileMetaStore> LoadMetaStoreAsync()
        {
            PLATEAUDynamicTileMetaStore metaStore = null;
            try
            {
                if (bundlePathMap.TryGetValue(nameof(PLATEAUDynamicTileMetaStore), out var metaStorePath))
                {
                    metaStorePath = Path.Combine(bundlePath, metaStorePath);

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
                else
                {
                    var data = Addressables.LoadAssetAsync<PLATEAUDynamicTileMetaStore>(nameof(PLATEAUDynamicTileMetaStore));
                    data.WaitForCompletion();
                    if (data.Status == AsyncOperationStatus.Succeeded)
                    {
                        metaStore = data.Result;
                    }
                    else
                    {
                        Debug.LogError($"MetaStoreのロードに失敗しました。ステータス: {data.Status}");
                    }

                    Addressables.Release(data);
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