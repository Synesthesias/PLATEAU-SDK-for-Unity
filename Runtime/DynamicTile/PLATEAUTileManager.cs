using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Jobs;


namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        public static readonly float DefaultLoadDistance = 1500f; // デフォルトのロード距離

        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        [SerializeField]
        private List<PLATEAUDynamicTile> dynamicTiles = new List<PLATEAUDynamicTile>(); // タイルリスト

        //[SerializeField]
        //private List<PLATEAUDynamicTile> excludeTiles = new List<PLATEAUDynamicTile>(); // 除外タイルリスト

        private PLATEAUDynamicTileJobSystem jobSystem;

        /// <summary>
        /// 指定したDynamicTileをもとにAddressablesからロードする
        /// </summary>
        public bool Load(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            //string address = tile.GetTileAddress(downSampleLevel, excludeTiles.Contains(tile)); 
            string address = tile.GetTileAddress(downSampleLevel);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {address}");
                return false;
            }
            // 既にロードされている場合はスキップ
            if( tile.LoadedObject != null || tile.IsLoading )
            {
                //Debug.Log($"Already loaded: {address}");
                return true;
            }

            Transform parent = tile.GetParent();

            tile.IsLoading = true;
            Addressables.InstantiateAsync(address, parent).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    tile.LoadedObject = handle.Result;
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
                tile.IsLoading = false;
            };

            return true;
        }

        // Task　Load
        public async Task<bool> LoadAsync(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            var tcs = new TaskCompletionSource<bool>();

            //string address = tile.GetTileAddress(downSampleLevel, excludeTiles.Contains(tile)); 
            string address = tile.GetTileAddress(downSampleLevel);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {address}");
                return await Task.FromResult<bool>(false);
            }
            // 既にロードされている場合はスキップ
            if (tile.LoadedObject != null || tile.IsLoading)
            {
                //Debug.Log($"Already loaded: {address}");
                return await Task.FromResult<bool>(true);
            }

            Transform parent = tile.GetParent();

            tile.IsLoading = true;
            Addressables.InstantiateAsync(address, parent).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    tile.LoadedObject = handle.Result;
                    tcs.SetResult(true);
                }
                else
                {
                    Debug.LogError($"Failed to load: {address}");
                    // 失敗の詳細情報を記録
                    if (handle.OperationException != null)
                    {
                        //Debug.LogException(handle.OperationException);
                        tcs.SetException(handle.OperationException);
                    }
                }
                tile.IsLoading = false;
            };

            return await tcs.Task;
        }

        /// <summary>
        /// 指定したDynamicTileに対応するAddressablesインスタンスをアンロードする
        /// </summary>
        public bool Unload(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            //string address = tile.GetTileAddress(downSampleLevel, excludeTiles.Contains(tile));
            string address = tile.GetTileAddress(downSampleLevel);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"指定したアドレスが見つかりません: {address}");
                return false;
            }

            if (tile.LoadedObject != null)
            {
                if (Addressables.ReleaseInstance(tile.LoadedObject))
                {
                    tile.LoadedObject = null;
                    tile.IsLoading = false;
                }
                else
                {
                    // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                    DestroyImmediate(tile.LoadedObject);
                    tile.LoadedObject = null;
                    tile.IsLoading = false;

                    Debug.LogWarning($"Failed to ReleaseInstance : {address}");
                }
            }
            else
            {
                //Debug.LogWarning($"Object not loaded: {address}");
            }

            return true;
        }


        public void AddTile(PLATEAUDynamicTile tile)
        {
            dynamicTiles.Add(tile);
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private void OnDestroy()
        {
            ClearAll();
        }

        private void OnDisable()
        {
            //NativeTileStates.Dispose();

            jobSystem?.Dispose();
            jobSystem = null;
        }

        public void ClearAll()
        {
            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in dynamicTiles)
            {
                if (tile.LoadedObject != null)
                {
                    if (!Addressables.ReleaseInstance(tile.LoadedObject))
                    {
                        DestroyImmediate(tile.LoadedObject);
                    }
                }
            }
        }

        public void UpdateAssetByCameraPosition(Vector3 position)
        {
            if(useJobSystem)
            {
                // Job Systemを使用する場合
                if(jobSystem == null)
                {
                    jobSystem = new PLATEAUDynamicTileJobSystem();
                    jobSystem.Initialize(this, dynamicTiles);
                }
                jobSystem.UpdateAssetByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                UpdateAssetByCameraPositionInternal(position);
            }
        }

        public void UpdateAssetByCameraPositionInternal(Vector3 position)
        {
            //Debug.Log($"UpdateAssetByCameraPosition: {position}");

            foreach (var tile in dynamicTiles)
            {
                var distance = tile.GetDistance(position, true);

                //Debug.Log($"UpdateAssetByCameraPosition: {distance}");
                if (distance < DefaultLoadDistance)
                {
                    //Load(tile, 0);
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.None;
                    else
                        tile.NextLoadState = LoadState.Load;
                }
                else
                {
                    //Unload(tile, 0);
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.Unload;
                    else
                        tile.NextLoadState = LoadState.None;
                }
            }

            ExecuteLoadTask();
        }

        private async void ExecuteLoadTask()
        {
            foreach (var tile in dynamicTiles)
            {
                if(tile.NextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (tile.NextLoadState == LoadState.Load)
                {
                    await LoadAsync(tile, 0);
                }
                else if (tile.NextLoadState == LoadState.Unload)
                {
                    Unload(tile, 0);
                }
            }
        }

        public void ShowBounds()
        {
            foreach (var tile in dynamicTiles)
            {
                PLATEAUDynamicTile.DrawBounds(tile.GetExtent(), Color.red, 30f);
            }
        }

        //private Vector3 lastCameraPosition;

        //void Awake()
        //{
        //    Debug.Log("PLATEAUTileManager Start");

        //    lastCameraPosition = Vector3.zero;
        //    ClearAll();
        //}

        ////Runtime時のCamera Update
        //private void Update()
        //{

        //    if (Camera.main != null)
        //    {
        //        Vector3 currentPosition = Camera.main.transform.position;
        //        if (currentPosition != lastCameraPosition)
        //        {
        //            //Debug.Log($"MainCameraが移動しました！ 新しい位置: {currentPosition}");
        //            lastCameraPosition = currentPosition;

        //            UpdateAssetByCameraPosition(currentPosition);
        //        }
        //    }
        //}
    }
}