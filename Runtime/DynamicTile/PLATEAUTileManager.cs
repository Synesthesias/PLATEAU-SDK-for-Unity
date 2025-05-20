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
                    DestroyImmediate(tile.transform.GetChild(i).gameObject);
                }
                
                Load(tile, downSampleLevel);
            }
        }

        /// <summary>
        /// 指定したDynamicTileをもとにAddressablesからロードする
        /// </summary>
        public void Load(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            string address;
            if (excludeTiles.Contains(tile))
            {
                address = tile.GetOriginalAddress();
            }
            else
            {
                address = tile.GetAddress(downSampleLevel);
            }
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {tile.name}");
                return;
            }
            Transform parent = tile.transform;
            LoadByAddress(address, parent);
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
                    Debug.Log($"Loaded: {address}");
                }
                else
                {
                    Debug.LogError($"Failed to load: {address}");
                }
            };
        }

        /// <summary>
        /// 指定したDynamicTileに対応するAddressablesインスタンスをアンロードする
        /// </summary>
        public void Unload(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            string address;
            if (excludeTiles.Contains(tile))
            {
                address = tile.GetOriginalAddress();
            }
            else
            {
                address = tile.GetAddress(downSampleLevel);
            }
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {tile.name}");
                return;
            }
            UnloadByAddress(address);
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
                Debug.Log($"Unloaded: {address}");
            }
            else
            {
                Debug.LogWarning($"Object not loaded: {address}");
            }
        }
    }
}