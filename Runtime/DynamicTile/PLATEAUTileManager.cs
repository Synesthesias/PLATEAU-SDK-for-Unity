using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public class PLATEAUTileManager : MonoBehaviour
    {
        /// <summary>
        /// マネージャーの状態を示す列挙型。
        /// </summary>
        public enum ManagerState
        {
            None,
            Initializing,
            Operating,
            CleaningUp,
        }

        /// <summary>
        /// アセットのロード結果を示す列挙型。
        /// </summary>
        public enum LoadResult
        {
            None, // 初期状態
            Success,
            Failure,
            AlreadyLoaded,
            Cancelled,
            Timeout
        }

        private const int MaxLodLevel = 4; // 最大LODレベル (最小LODは0とする)

        /// <summary>
        /// 各Zoomレベルごとのカメラからのロード距離を定義します。
        /// </summary>
        public TileLoadDistanceCollection loadDistances = new TileLoadDistanceCollection(); 

        [SerializeField]
        private string catalogPath;

        [SerializeField]
        private bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool showDebugLog = false; // ログを表示するか

        // タイルコレクション
        internal DynamicTileCollection TileCollection { get; }

        // マネージャーの状態
        public ManagerState State { get; private set; } = ManagerState.None;

        // 最後にカメラが更新された位置
        internal Vector3 LastCameraPosition { get; private set; } = Vector3.zero;
        

        private AddressableLoader addressableLoader = new ();

        private PLATEAUDynamicTileLoadTask loadTask; // タイルのロードタスクを管理するクラス
        
        /// <summary>
        /// 現在タスクが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool HasCurrentTask => loadTask != null && loadTask.HasCurrentTask;

        /// <summary>
        /// Instance化のコルーチンが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool IsCoroutineRunning => loadTask != null && loadTask.IsCoroutineRunning;

        private readonly ConditionalLogger logger;

        public PLATEAUTileManager()
        {
            logger = new ConditionalLogger(() => this.showDebugLog);
            TileCollection = new DynamicTileCollection(logger);
        }

        /// <summary>
        /// カタログに変わるScriptableObjectを使用して、タイルの初期化を行います。
        /// </summary>
        /// <returns></returns>
        public async Task InitializeTiles()
        {
            if (State == ManagerState.Initializing)
                return;

            State = ManagerState.Initializing;

            // PLATEAUDynamicTileMetaStoreをAddressablesからロード
            var metaStore = await addressableLoader.InitializeAsync(catalogPath);
            if (metaStore == null || metaStore.TileMetaInfos.Count == 0)
            {
                logger.LogWarn("No tiles found in the meta store. Please check the catalog path or ensure tiles are registered.");
                State = ManagerState.None;
                return;
            }

            logger.Log($"InitializeTiles: {metaStore.TileMetaInfos.Count} tiles found in meta store.");
            
            TileCollection.RefreshByTileMetas(metaStore.TileMetaInfos);
            

            loadTask = new(this, logger);
            LastCameraPosition = Vector3.zero; 
            State = ManagerState.Operating;
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
        /// Tileを指定してAddressablesからロードする
        /// </summary>
        public async Task<LoadResult> Load(PLATEAUDynamicTile tile, float timeoutSeconds = 2f)
        {
            if (loadTask == null)
            {
                Debug.LogError("LoadTask is not initialized.");
                return LoadResult.Failure;
            }
            return await loadTask.Load(tile, timeoutSeconds);
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        public async Task<LoadResult> Load(string address)
        {
            var tile = TileCollection.GetTileByAddress(address);
            if (tile == null)
            {
                logger.LogWarn($"指定したアドレスに対応するタイルが見つかりません: {address}");
                return await Task.FromResult<LoadResult>(LoadResult.Failure);
            }
            if (loadTask == null)
            {
                Debug.LogError("LoadTask is not initialized.");
                return LoadResult.Failure;
            }
            return await loadTask.Load(tile);
        }
        

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private void OnDestroy()
        {
            if (loadTask != null)
            {
                loadTask.DestroyTask().ContinueWithErrorCatch();
            }
            TileCollection?.ClearTiles();
        }

        /// <summary>
        /// Job Systemを使用している場合、OnDisableでDisposeする
        /// </summary>
        private void OnDisable()
        {
            if (loadTask != null)
            {
                loadTask.DestroyTask().ContinueWithErrorCatch();
            }
        }

        /// <summary>
        /// 保持しているカメラの位置を更新します。
        /// </summary>
        /// <param name="position"></param>
        public void UpdateCameraPosition(Vector3 position)
        {
            LastCameraPosition = position;
        }

        /// <summary>
        /// Tileデータを追加
        /// </summary>
        /// <param name="tile"></param>
        public void AddTile(PLATEAUDynamicTile tile)
        {
            TileCollection.AddTile(tile);
        }
        
        /// <summary>
        /// 登録したタイルをクリア
        /// </summary>
        public void ClearTiles()
        {
            TileCollection.ClearTiles();
            State = ManagerState.None; // マネージャーの状態をリセット
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearTileAssets()
        {
            var originalState = State == ManagerState.CleaningUp ? ManagerState.Operating : State;
            State = ManagerState.CleaningUp;

            TileCollection.ClearTileAssets();
            State = originalState;
        }

        public bool CheckIfCameraPositionHasChanged(Vector3 position, float threshold = 0.01f)
        {
            return Vector3.Distance(LastCameraPosition, position) > threshold;
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timeoutSeconds">完了まで待機する際のタイムアウト秒数</param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position, float timeoutSeconds = 10f)
        {
            if ( State != ManagerState.Operating)
                return;

            if (TileCollection.IsEmpty)
                return;

            if (loadTask == null) return;
            if (await loadTask.UpdateAssetsByCameraPosition(position, useJobSystem, timeoutSeconds))
            {
                UpdateCameraPosition(position); // 最後のカメラ位置を更新
            };
        }

        /// <summary>
        /// 実行中のロードタスクをキャンセルし、CancellationTokenSourceをリセットします。
        /// </summary>
        public async Task CancelLoadTask()
        {
            if (loadTask != null)
            {
                await loadTask.CancelLoadTask().ContinueWithErrorCatch(); // タスクのキャンセルをタスクに委譲
            }
        }

        /// <summary>
        /// TaskからCallされるTileのロード開始処理
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        internal async Task<LoadResult> PrepareLoadTile(PLATEAUDynamicTile tile)
        {
            if (loadTask != null)
            {
                return await loadTask.PrepareLoadTile(tile);
            }

            return LoadResult.Failure;
        }

        /// <summary>
        /// 指定されたアセット読込済タイルからGameObjectをインスタンス化します。
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        internal bool InstantiateFromTile(PLATEAUDynamicTile tile)
        {
            if (tile.LoadHandle.IsValid() && tile.LoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                //DebugLog($"アセットのロードに成功しました: {address}", false);
                var instance = Instantiate(tile.LoadHandle.Result);
                if (instance != null)
                {
                    var parentTransform = FindParent(tile.Lod);
                    if (parentTransform != null)
                    {
                        instance.transform.SetParent(parentTransform, false); // LODごとの親Transformに設定
                        instance.name = tile.Address;
                        instance.hideFlags = HideFlags.DontSave; // シーン保存時にオブジェクトを保存しない
                        DynamicTileCollection.DeleteGameObjectInstance(tile.LoadedObject); // 既存のオブジェクトが存在する場合削除
                        tile.LoadedObject = instance; // ロードしたオブジェクトを保持

                        //Debug Material色変え
                        //ReplaceWithDebugMaterial(tile);

                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// GameObjectインスタンスを削除します。
        /// </summary>
        /// <param name="obj"></param>
        internal static void DeleteGameObjectInstance(GameObject obj)
        {
            DynamicTileCollection.DeleteGameObjectInstance(obj);
        }

        /// <summary>
        /// 指定されたLODから親Transformを取得または作成します。
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        private Transform FindParent(int lod)
        {
            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                instance = this.gameObject;

            return TileCollection.FindParent(lod, instance.transform);
        }

        /// <summary>
        /// デバッグ用 (元アセットのマテリアルが変わってしまうので、注意)
        /// タイルのマテリアルをZoomLevelごとに着色して置き換えます。
        /// </summary>
        /// <param name="tile"></param>
        private void ReplaceWithDebugMaterial(PLATEAUDynamicTile tile)
        {
            var color = Color.white;
            switch (tile.ZoomLevel)
            {
                case 9:
                    color = Color.red;
                    break;
                case 10:
                    color = Color.yellow;
                    break;
                case 11:
                    color = Color.green;
                    break;
                default:
                    break;
            }
            //color = Color.white;
            var renderers = tile.LoadedObject.GetComponentsInChildren<Renderer>(false)?.ToList();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    var materials = renderer.sharedMaterials;
                    if (materials != null)
                    {
                        for (int i = 0; i < materials.Length; i++)
                        {
                            var material = materials[i];
                            if (material != null)
                            {
                                material.SetColor("_BaseColor", color);
                                material.color = color;
                            }
                        }
                    }
                }
            }
        }
    }
}