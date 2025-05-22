using PLATEAU.CityInfo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        private Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();
        private List<string> addressCatalog = new List<string>();

        [SerializeField]
        private List<string> excludeTiles = new List<string>(); // 除外Addressリスト

        private const string DynamicTileLabelName = "DynamicTile";

        void Start()
        {
            var cityModel = FindObjectOfType<PLATEAUInstancedCityModel>();
            if (cityModel == null)
            {
                Debug.LogWarning("都市モデルが見つかりません。");
                return;
            }
            
            // Addressablesの初期化を非同期で行い、完了後にカタログ（アドレス一覧）を取得
            Addressables.InitializeAsync().Completed += handle =>
            {
                addressCatalog.Clear();

                // Addressablesのラベルを使用して、アドレスを取得
                Addressables.LoadResourceLocationsAsync(DynamicTileLabelName).Completed += locHandle =>
                {
                    foreach (var loc in locHandle.Result)
                    {
                        var address = loc.PrimaryKey;
                        addressCatalog.Add(address);

                        // TODO：カメラに応じてLoadする
                        LoadByAddress(address, cityModel.gameObject.transform);
                    }
                };
            };
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        public void LoadByAddress(string address, Transform parent = null)
        {
            Addressables.InstantiateAsync(address, parent).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedObjects[address] = handle.Result;
                }
                else
                {
                    Debug.LogError($"Failed to load: {address}");
                    // 失敗の詳細情報を記録
                    if (handle.OperationException != null)
                    {
                        Debug.LogException(handle.OperationException);
                    }
                }
            };
        }

        /// <summary>
        /// Addressで指定したインスタンスをAddressablesからアンロードする
        /// </summary>
        public void UnloadByAddress(string address)
        {
            if (loadedObjects.ContainsKey(address))
            {
                Addressables.ReleaseInstance(loadedObjects[address]);
                loadedObjects.Remove(address);
            }
            else
            {
                Debug.LogWarning($"Object not loaded: {address}");
            }
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private void OnDestroy()
        {
            // すべてのロード済みオブジェクトをアンロード
            foreach (var address in new List<string>(loadedObjects.Keys))
            {
                UnloadByAddress(address);
            }
            // ディクショナリをクリア
            loadedObjects.Clear();
        }
    }
}