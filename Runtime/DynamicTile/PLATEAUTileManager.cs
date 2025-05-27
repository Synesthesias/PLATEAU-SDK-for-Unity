using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        [SerializeField]
        private List<PLATEAUDynamicTile> dynamicTiles = new();
        
        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        private const string DynamicTileLabelName = "DynamicTile";
        private Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();
        private AddressableLoader addressableLoader = new ();

        async void Start()
        {
            // TODO: カメラからの距離に応じてLoad
            
            var cityModel = FindObjectOfType<PLATEAUInstancedCityModel>();
            if (cityModel == null)
            {
                Debug.LogWarning("都市モデルが見つかりません。");
                return;
            }
            
            List<string> addresses;
            if (!string.IsNullOrEmpty(catalogPath))
            {
                // カタログパスが指定されている場合は、カタログをロード
                addresses = await addressableLoader.LoadCatalogAsync(catalogPath, DynamicTileLabelName);
            }
            else
            {
                // ローカルのアドレスをロード
                addresses = await addressableLoader.LoadLocalAddresses(DynamicTileLabelName);
            }
            
            foreach (var address in addresses)
            {
                Load(address, cityModel.transform);
            }
            
            Debug.Log("全てのAddressablesのロードが完了しました");
        }
        
        /// <summary>
        /// カタログパスを保存します。
        /// </summary>
        /// <param name="path"></param>
        public void SaveCatalogPath(string path)
        {
            // パスを正規化（バックスラッシュをスラッシュに変換）
            catalogPath = path.Replace('\\', '/');
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        public async void Load(string address, Transform parent = null)
        {
            try
            {
                var instance = await addressableLoader.InstantiateAssetAsync(address, parent);
                if (instance != null)
                {
                    loadedObjects[address] = instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"アセットのロード中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// Addressを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public void Unload(string address)
        {
            if (loadedObjects.TryGetValue(address, out var instance))
            {
                try
                {
                    Addressables.ReleaseInstance(instance);
                    loadedObjects.Remove(address);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"アセットのアンロード中にエラーが発生しました: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"アセット {address} はロードされていません");
            }
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private void OnDestroy()
        {
            foreach (var loadedObject  in loadedObjects)
            {
                Addressables.ReleaseInstance(loadedObject.Value);
            }
            loadedObjects.Clear();
            Debug.Log("全てのアセットをアンロードしました");
        }
        
        /// <summary>
        /// Tileデータを追加
        /// </summary>
        /// <param name="tile"></param>
        public void AddTile(PLATEAUDynamicTile tile)
        {
            dynamicTiles.Add(tile);
        }
    }
}