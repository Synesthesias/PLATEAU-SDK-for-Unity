using PLATEAU.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        public static readonly float DefaultLoadDistance = 1500f; // デフォルトのロード距離

        public const bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか
      
        [SerializeField]
        private List<PLATEAUDynamicTile> dynamicTiles = new List<PLATEAUDynamicTile>(); // タイルリスト

        //[SerializeField]
        //private List<PLATEAUDynamicTile> excludeTiles = new List<PLATEAUDynamicTile>(); // 除外タイルリスト

        public Vector3 LastCameraPosition { get; private set; } = Vector3.zero; // 最後にカメラが更新された位置

        private PLATEAUDynamicTileJobSystem jobSystem;

        /// <summary>
        /// 指定したDynamicTileをもとにAddressablesからロードする
        /// </summary>
        public async Task<bool> LoadAsync(PLATEAUDynamicTile tile, int downSampleLevel)
        {
            var tcs = new TaskCompletionSource<bool>();
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

            Transform parent = tile.Parent;

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
            return true;
        }

        /// <summary>
        /// PLATEAUDynamicTileを追加する
        /// </summary>
        /// <param name="tile"></param>
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

        /// <summary>
        /// Job Systemを使用している場合、OnDisableでDisposeする
        /// </summary>
        private void OnDisable()
        {
            jobSystem?.Dispose();
            jobSystem = null;
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearAll()
        {
            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in dynamicTiles)
            {
                if (tile == null) continue; // タイルがnullの場合はスキップ
                if (tile.LoadedObject != null)
                {
                    if (!Addressables.ReleaseInstance(tile.LoadedObject))
                    {
                        DestroyImmediate(tile.LoadedObject);
                    }
                }
            }
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
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

            LastCameraPosition = position; // 最後のカメラ位置を更新
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateAssetByCameraPositionInternal(Vector3 position)
        {
            foreach (var tile in dynamicTiles)
            {
                var distance = tile.GetDistance(position, true);
                if (distance < DefaultLoadDistance)
                {
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.None;
                    else
                        tile.NextLoadState = LoadState.Load;
                }
                else
                {
                    if (tile.IsLoadedOrLoading)
                        tile.NextLoadState = LoadState.Unload;
                    else
                        tile.NextLoadState = LoadState.None;
                }
            }
            ExecuteLoadTask();
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// </summary>
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

        // Debug Bounds表示
        public void ShowBounds()
        {
            foreach (var tile in dynamicTiles)
            {
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
            }
        }
    }
}