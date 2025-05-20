using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        private Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();

        [SerializeField]
        private List<PLATEAUDynamicTile> excludeTiles = new List<PLATEAUDynamicTile>(); // 除外タイルリスト

        void Start()
        {
            // TODO: カメラ位置・視野に応じてロード処理を行うようにする
            int downSampleLevel = 0; // 仮のダウンサンプルレベル
            PLATEAUDynamicTile[] dynamicTiles = GameObject.FindObjectsOfType<PLATEAUDynamicTile>();
            foreach (var tile in dynamicTiles)
            {
                // 子要素をすべて削除
                for (int i = tile.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(tile.transform.GetChild(i).gameObject);
                }
                
                Load(tile, downSampleLevel);
            }
        }

        /// <summary>
        /// 指定したDynamicTileをもとにAddressablesからロードする
        /// </summary>
        public bool Load(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            string address = GetTileAddress(tile, downSampleLevel);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {tile.name}");
                return false;
            }
            // 既にロードされている場合はスキップ
            if (loadedObjects.ContainsKey(address))
            {
                Debug.Log($"Already loaded: {address}");
                return true;
            }
            Transform parent = tile.transform;
            LoadByAddress(address, parent);
            return true;
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        private void LoadByAddress(string address, Transform parent = null)
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
        /// 指定したDynamicTileに対応するAddressablesインスタンスをアンロードする
        /// </summary>
        public bool Unload(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            string address = GetTileAddress(tile, downSampleLevel);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {tile.name}");
                return false;
            }
            UnloadByAddress(address);
            return true;
        }

        /// <summary>
        /// Addressで指定したインスタンスをAddressablesからアンロードする
        /// </summary>
        private void UnloadByAddress(string address)
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
        /// タイルのアドレスを取得する（除外リストを考慮）
        /// </summary>
        private string GetTileAddress(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            if (tile == null) return null;
            return excludeTiles.Contains(tile)
                ? tile.GetOriginalAddress()
                : tile.GetAddress(downSampleLevel);
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