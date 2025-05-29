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
        public List<PLATEAUDynamicTile> DynamicTiles => dynamicTiles;

        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        private const string DynamicTileLabelName = "DynamicTile";
        private Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();
        private AddressableLoader addressableLoader = new ();

        async void Start()
        {
            // 初期化
            var addresses = await addressableLoader.Initialize(catalogPath);

            // TODO: カメラからの距離に応じてLoad
            var cityModel = FindObjectOfType<PLATEAUInstancedCityModel>();
            if (cityModel == null)
            {
                Debug.LogWarning("都市モデルが見つかりません。");
                return;
            }
            
            foreach (var address in addresses)
            {
                Load(address, cityModel.transform);
            }
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
            if (loadedObjects.ContainsKey(address))
            {
                Debug.LogWarning($"アセット {address} は既にロードされています");
                return;
            }

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
            if (!loadedObjects.ContainsKey(address))
            {
                Debug.LogWarning($"アセット {address} はロードされていません");
                return;
            }

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
            if (tile == null)
            {
                Debug.LogWarning("nullのタイルは追加できません");
                return;
            }
            
            if (dynamicTiles.Contains(tile))
            {
                Debug.LogWarning("既に追加済みのタイルです");
                return;
            }
            dynamicTiles.Add(tile);
        }
        
        /// <summary>
        /// 登録したタイルをクリア
        /// </summary>
        public void ClearTiles()
        {
            dynamicTiles.Clear();
        }
    }
}