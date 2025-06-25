using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        public Dictionary<int, (float, float)> loadDistances = new Dictionary<int, (float, float)>
        {
            { 11, (-10000f, 500f) },
            { 10, (500f, 1500f) },
            { 9, (1500f, 10000f) },
        }; 

        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        [SerializeField]
        private bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool showDebugLog = false; // ログを表示するか

        // 使用中のタイルリスト
        public List<PLATEAUDynamicTile> DynamicTiles { get; private set; } = new();

        // 全タイルのロードタスク実行時のCancellationTokenSource
        internal CancellationTokenSource LoadTaskCancellationTokenSource { get; private set; } = new();

        // TileとAddressのマッピング
        private Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        // Parent TransformをLODごとに管理する辞書
        private Dictionary<int, Transform> lodParentDict = new();

        // マネージャーの状態
        public ManagerState State { get; private set; } = ManagerState.None;

        // 最後にカメラが更新された位置
        internal Vector3 LastCameraPosition { get; private set; } = Vector3.zero; 

        private AddressableLoader addressableLoader = new ();
        private PLATEAUDynamicTileLoader tileLoader; // Addressablesからタイルをロード/アンロードするクラス
        private PLATEAUDynamicTileInstantiation tileInstantiation; // ロード時インスタンス化とアンロードをキューで管理してコルーチンでインスタンス化を実行するクラス
        private PLATEAUDynamicTileJobSystem jobSystem; // JobSystemで距離判定を行う場合に使用

        // 実行中のUpdateAssetsByCameraPosition内のTask
        private Task CurrentTask;

        /// <summary>
        /// 現在タスクが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool HasCurrentTask => CurrentTask != null && !CurrentTask.IsCompleted;

        /// <summary>
        /// Instance化のコルーチンが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool IsCoroutineRunning => tileInstantiation != null && tileInstantiation.IsRunning;

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
                Debug.LogWarning("No tiles found in the meta store. Please check the catalog path or ensure tiles are registered.");
                State = ManagerState.None;
                return;
            }

            Debug.Log($"InitializeTiles: {metaStore.TileMetaInfos.Count} tiles found in meta store.");
            ClearTiles(); // 既存のタイルリストをクリア
            foreach (var tileMeta in metaStore.TileMetaInfos)
            {
                var tile = new PLATEAUDynamicTile(tileMeta);
                AddTile(tile);
            }

            tileLoader = new(this); 
            tileInstantiation = new(this);

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
            return await tileLoader?.Load(tile, timeoutSeconds);
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする (リトライ機能付き)
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="maxRetryCount">リトライ数</param>
        /// <param name="delaySeconds">リトライ時のディレイ</param>
        /// <returns></returns>
        public async Task<LoadResult> LoadWithRetry(PLATEAUDynamicTile tile, int maxRetryCount = 2, float delaySeconds = 0.3f) 
        { 
            return await tileLoader?.LoadWithRetry(tile, maxRetryCount, delaySeconds);
        }

        /// <summary>
        /// Addressを指定してAddressablesからロードする
        /// </summary>
        public async Task<LoadResult> Load(string address)
        {
            var tile = tileAddressesDict.GetValueOrDefault(address);
            if (tile == null)
            {
                DebugLog($"指定したアドレスに対応するタイルが見つかりません: {address}");
                return await Task.FromResult<LoadResult>(LoadResult.Failure);
            }
            return await tileLoader?.Load(tile);
        }

        /// <summary>
        /// Tileを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public bool Unload(PLATEAUDynamicTile tile)
        {
            return tileLoader?.Unload(tile) ?? false;
        }

        /// <summary>
        /// Addressを指定してAddressablesからアンロードする
        /// </summary>
        public bool Unload(string address)
        {
            var tile = tileAddressesDict.GetValueOrDefault(address);
            if (tile == null)
            {
                DebugLog($"指定したアドレスに対応するタイルが見つかりません: {address}");
                return false;
            }
            return Unload(tile);
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        private async void OnDestroy()
        {
            await CancelLoadTask();
            ClearTiles();
            jobSystem?.Dispose();
            jobSystem = null;

            tileLoader?.Dispose();
            tileInstantiation?.Dispose();
        }

        /// <summary>
        /// Job Systemを使用している場合、OnDisableでDisposeする
        /// </summary>
        private async void OnDisable()
        {
            await CancelLoadTask();
            jobSystem?.Dispose();
            jobSystem = null;
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
            if (DynamicTiles.Contains(tile))
            {
                Debug.LogWarning("既に追加済みのタイルです");
                return;
            }
            if (tileAddressesDict.ContainsKey(tile.Address))
            {
                Debug.LogWarning("既に追加済みのタイルAddressです");
                return;
            }
                
            DynamicTiles.Add(tile);
            tileAddressesDict[tile.Address] = tile;
        }
        
        /// <summary>
        /// 登録したタイルをクリア
        /// </summary>
        public void ClearTiles()
        {
            ClearTileAssets();
            DynamicTiles.Clear();
            tileAddressesDict.Clear();

            // LODの親Transformもクリア
            lodParentDict.Clear();

            State = ManagerState.None; // マネージャーの状態をリセット
        }

        /// <summary>
        /// すべてのロード済みオブジェクトをアンロードする
        /// </summary>
        public void ClearTileAssets()
        {
            var originalState = State == ManagerState.CleaningUp ? ManagerState.Operating : State;
            State = ManagerState.CleaningUp;

            // すべてのロード済みオブジェクトをアンロード
            foreach (var tile in DynamicTiles)
            {
                try
                {
                    if (tile.LoadHandle.IsValid())
                    {
                        Addressables.Release(tile.LoadHandle);
                    }
                }
                catch (Exception ex)
                {
                    DebugLog($"タイルのアンロード中にエラーが発生しました: {tile.Address} {ex.Message}");
                }
                finally
                {
                    DeleteGameObjectInstance(tile.LoadedObject);
                    tile.Reset(); // タイルの状態をリセット
                }
            }

            ClearLodChildren();
            State = originalState;
        }

        /// <summary>
        /// LOD直下の子オブジェクトをすべてアンロードします。
        /// (TileからのUnloadでは消去されない場合があるため)
        /// </summary>
        private void ClearLodChildren()
        { 
            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                instance = this.gameObject;

            // Lod直下の子オブジェクトをすべてアンロード
            for (int lod = 0; lod <= MaxLodLevel; lod++)
            {
                var lodName = $"LOD{lod}";
                var lodParent = instance.transform.Find(lodName)?.gameObject;
                if (lodParent != null)
                {
                    for (int i = lodParent.transform.childCount - 1; i >= 0; i--)
                    {
                        var child = lodParent.transform.GetChild(i);
                        if (child != null)
                        {
                            // 子オブジェクトを削除
                            DeleteGameObjectInstance(child.gameObject);
                        }
                    }
                    // LODの親Transformも削除
                    DeleteGameObjectInstance(lodParent);
                }
            }
        }

        public bool CheckIfCameraPositionHasChenged(Vector3 position, float threshold = 0.01f)
        {
            return Vector3.Distance(LastCameraPosition, position) > threshold;
            //return (position != LastCameraPosition);
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

            if (DynamicTiles.Count <= 0)
                return;

            // 前回のタスクがまだ完了していない場合処理しない
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                return;

            await CancelLoadTask();
            LoadTaskCancellationTokenSource?.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds)); // タイムアウト設定

            if (useJobSystem)
            {
                if(jobSystem?.TileCount != DynamicTiles.Count)
                {
                    // タイル数が変更された場合、Job SystemのNativeArrayを再初期化
                    await CancelLoadTask();
                    jobSystem?.Dispose();
                    jobSystem = null;
                }

                // Job Systemを使用する場合
                if (jobSystem == null)
                {
                    jobSystem = new PLATEAUDynamicTileJobSystem();
                    jobSystem.Initialize(this, DynamicTiles);
                }

                CurrentTask = jobSystem.UpdateAssetsByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                CurrentTask = UpdateAssetsByCameraPositionInternal(position);
            }

            LastCameraPosition = position; // 最後のカメラ位置を更新

            await CurrentTask;
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// JobSystemを使用しない場合の実装。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPositionInternal(Vector3 position)
        {
            var sortByDistance = new List<PLATEAUDynamicTile>(DynamicTiles);
            sortByDistance.Sort((a, b) => a.DistanceFromCamera.CompareTo(b.DistanceFromCamera)); // Distance順にソート

            foreach (var tile in sortByDistance)
            {
                var distance = tile.GetDistance(position, true);
                if (WithinTheRange(distance,tile))
                {
                    if (tile.LoadHandle.IsValid())
                        tile.NextLoadState = LoadState.None;
                    else
                        tile.NextLoadState = LoadState.Load;
                }
                else
                {
                    if (tile.LoadHandle.IsValid())
                        tile.NextLoadState = LoadState.Unload;
                    else
                        tile.NextLoadState = LoadState.None;
                }
            }

            try
            {
                await ExecuteLoadTask(LoadTaskCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                DebugLog("タイルのロードTaskがキャンセルされました。");
            }
            finally
            {
                PostLoadTask();
            }
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// Job Systemを使用しない場合の実装。
        /// </summary>
        private async Task ExecuteLoadTask(CancellationToken token)
        {
            foreach (var tile in DynamicTiles)
            {
                if (tile.NextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (tile.NextLoadState == LoadState.Load)
                {
                    await PrepareLoadTile(tile);
                }
                else if (tile.NextLoadState == LoadState.Unload)
                {
                    PrepareUnloadTile(tile);
                }
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// 実行中のロードタスクをキャンセルし、CancellationTokenSourceをリセットします。
        /// </summary>
        public async Task CancelLoadTask()
        {
            try
            {
                LoadTaskCancellationTokenSource?.Cancel();
            }
            catch (ObjectDisposedException)
            { }
            if (HasCurrentTask)
                await CurrentTask;
            LoadTaskCancellationTokenSource?.Dispose();
            LoadTaskCancellationTokenSource = new();
        }

        /// <summary>
        /// TaskからCallされるTileのロード処理
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        internal async Task<LoadResult> PrepareLoadTile(PLATEAUDynamicTile tile)
        {
            var result =  await LoadWithRetry(tile);
            if (result == LoadResult.Success) {
                // ロード成功時は、タイルのインスタンス化キューに追加
                tileInstantiation?.AddToQueue(tile, true);
            }
            return result;
        }

        /// <summary>
        /// TaskからCallされるTileのアンロード処理
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        internal bool PrepareUnloadTile(PLATEAUDynamicTile tile)
        {
            tileInstantiation?.AddToQueue(tile, false); // タイルを削除キューに追加
            return true;
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
                    instance.transform.SetParent(FindParent(tile.Lod), false); // LODごとの親Transformに設定
                    instance.name = tile.Address;
                    instance.hideFlags = HideFlags.DontSave; // シーン保存時にオブジェクトを保存しない
                    DeleteGameObjectInstance(tile.LoadedObject); // 既存のオブジェクトが存在する場合削除
                    tile.LoadedObject = instance; // ロードしたオブジェクトを保持

                    //Debug Material色変え
                    ReplaceWithDebugMaterial(tile);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// GameObjectインスタンスを削除します。
        /// </summary>
        /// <param name="obj"></param>
        internal void DeleteGameObjectInstance(GameObject obj)
        {
            if (obj == null)
                return;
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        /// <summary>
        /// タイルのロードタスク後に実行する処理。
        /// Unloadキューに追加されたGameObjectインスタンスをアンロードします。
        /// </summary>
        internal void PostLoadTask()
        {
            tileInstantiation?.DeleteFromeQueue(); // キューからGameObjectインスタンス削除
        }

        /// <summary>
        /// 指定された距離がタイルのロード範囲内にあるかどうかを判定
        /// </summary>
        /// <param name="distance">カメラからの距離</param>
        /// <param name="tile">Tile</param>
        /// <returns></returns>
        internal bool WithinTheRange(float distance, PLATEAUDynamicTile tile)
        {
            if(loadDistances.TryGetValue(tile.ZoomLevel, out var minmax)){
                var (min, max) = minmax;
                return (distance >= min && distance <= max);
            }
            return false;
        }

        /// <summary>
        /// 指定されたLODから親Transformを取得または作成します。
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        private Transform FindParent(int lod)
        {
            if (lodParentDict.TryGetValue(lod, out var parentTransform))
            {
                if (parentTransform != null)
                    return parentTransform;
            }

            var lodName = $"LOD{lod}";
            GameObject lodObject = null;
            var instance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>()?.gameObject;
            if (instance == null)
                instance = this.gameObject;

            lodObject = instance.transform.Find(lodName)?.gameObject;
            if (lodObject == null)
            {
                lodObject = new GameObject(lodName);
                lodObject.transform.SetParent(instance?.transform, false);
            }

            lodParentDict[lod] = lodObject.transform;
            return lodObject.transform;
        }

        /// <summary>
        /// showDebugTileInfo:ON時のみDebugログに警告メッセージを出力します。
        /// </summary>
        /// <param name="message"></param>
        internal void DebugLog(string message, bool warn = true)
        {
            if (showDebugLog)
            {
                if (warn)
                    Debug.LogWarning(message);
                else
                    Debug.Log(message);
            }
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