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
            Success,
            Failure,
            AlreadyLoaded,
            NeedRetry,
            Cancelled,
            Timeout
        }

        private const int MaxLodLevel = 4; // 最大LODレベル (最小LODは0とする)

        //[SerializeField]
        //public float loadDistance = 1500f; // ロードを行うカメラからの距離

        /// <summary>
        /// 各Zoomレベルごとのカメラからのロード距離を定義します。
        /// </summary>
        [SerializeField]
        public Dictionary<int, (float, float)> loadDistances = new Dictionary<int, (float, float)>
        {
            { 11, (-1000f, 1001f) },
            { 10, (1000f, 1501f) },
            { 9, (1500f, 2000f) },
        }; 

        [SerializeField]
        private string catalogPath;
        public string CatalogPath => catalogPath;

        [SerializeField]
        private bool showDebugTileInfo = true; // Debug情報を表示するかどうか

        [ConditionalShow("showDebugTileInfo")]
        [SerializeField]
        private bool useJobSystem = true; // Job Systemを使用するかどうか

        // 使用中のタイルリスト
        public List<PLATEAUDynamicTile> DynamicTiles { get; private set; } = new();

        // 全タイルのロードタスク実行時のCancellationTokenSource
        public CancellationTokenSource LoadTaskCancellationTokenSource { get; private set; } = new();

        // TileとAddressのマッピング
        private Dictionary<string, PLATEAUDynamicTile> tileAddressesDict = new();

        // Parent TransformをLODごとに管理する辞書
        private Dictionary<int, Transform> lodParentDict = new();

        // マネージャーの状態
        public ManagerState State { get; private set; } = ManagerState.None;

        // 最後にカメラが更新された位置
        public Vector3 LastCameraPosition { get; private set; } = Vector3.zero; 

        private AddressableLoader addressableLoader = new ();
        private PLATEAUDynamicTileJobSystem jobSystem;

        // 実行中のUpdateAssetsByCameraPosition内のTask
        private Task CurrentTask;
        // Taskの実行中にキャンセルされたタスクの数をカウントし指定回数以上になった場合は、強制的にキャンセルする
        private int NumPendingTask = 0;
        private const int MAX_PENDING_TASK_COUNT = 100; // 最大待機タスク数

        /// <summary>
        /// 現在タスクが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool HasCurrentTask => CurrentTask != null && !CurrentTask.IsCompleted;

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
        public async Task<LoadResult> Load(PLATEAUDynamicTile tile, float timeoutSeconds = 1f)
        {
            string address = tile.Address;
            if (string.IsNullOrEmpty(address))
            {
                DebugLog($"指定したアドレスが見つかりません: {address}");
                return await Task.FromResult<LoadResult>(LoadResult.Failure);
            }
            // 既にロードされている場合はスキップ
            if (tile.LoadHandle.IsValid() || tile.LoadedObject != null)
            {
                DebugLog($"Already loaded: {address}", false);
                return await Task.FromResult<LoadResult>(LoadResult.AlreadyLoaded);
            }

            try
            {
                if (tile.LoadHandleCancellationTokenSource == null)
                {
                    // Addressablesでは、Cancel処理がサポートされていないため、CancellationTokenSourceを使用してキャンセル可能なロードを実装
                    tile.LoadHandleCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(LoadTaskCancellationTokenSource.Token);
                }

                if (!tile.LoadHandle.IsDone)
                {
                    tile.LoadHandleCancellationTokenSource.Cancel();
                    await tile.LoadHandle.Task;
                }

                var timeoutTask = Task.Delay((int)(timeoutSeconds * 1000));

                //Cancel処理
                tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                var handle = Addressables.InstantiateAsync(address, FindParent(tile.Lod));

                //Cancel処理
                tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                tile.LoadHandle = handle;
                //await handle.Task;
                var completedTask = await Task.WhenAny(handle.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"アセットのロードがタイムアウトしました: {address}");
                }

                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    //DebugLog($"アセットのロードに成功しました: {address}", false);
                    var instance = handle.Result;
                    if (instance != null)
                    {
                        instance.name = address;
                        instance.hideFlags = HideFlags.DontSave; // シーン保存時にオブジェクトを保存しない
                        tile.LoadHandleCancellationTokenSource?.Dispose();
                        tile.LoadHandleCancellationTokenSource = null; // Dispose後はnullにする
                        return LoadResult.Success;
                    }
                }
                else
                {
                    if (tile.LoadHandle.IsValid())
                        Addressables.ReleaseInstance(tile.LoadHandle);
                    tile.Reset();
                    DebugLog($"アセットのロードに失敗しました: {address}");
                    return LoadResult.NeedRetry;
                }
            }
            catch (Exception ex)
            {
                var loadResult = LoadResult.Failure;
                if(ex is OperationCanceledException)
                {
                    DebugLog($"アセットのロードがキャンセルされました: {address}");
                    loadResult = LoadResult.Cancelled;
                }
                else if (ex is TimeoutException)
                {
                    DebugLog($"アセットのロードがタイムアウトしました: {address}");
                    loadResult = LoadResult.Timeout;
                }
                else
                {
                    DebugLog($"アセットのロード中にエラーが発生しました: {address} {ex.Message}");
                }

                if (tile.LoadHandle.IsValid())
                    Addressables.ReleaseInstance(tile.LoadHandle);

                tile.Reset();
                return loadResult;
            }

            tile.Reset();
            return LoadResult.Failure;
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする (リトライ機能付き)
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="maxRetryCount">リトライ数</param>
        /// <returns></returns>
        public async Task<bool> LoadWithRetry(PLATEAUDynamicTile tile, int maxRetryCount = 2, int delay = 300)
        {
            var result = await Load(tile);
            if (result == LoadResult.NeedRetry)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Retry");
                // ロードに失敗した場合は、リトライ
                DebugLog($"タイルのロードに失敗しました。リトライします: {tile.Address}");
                int retryCount = 0;
                while (retryCount < maxRetryCount)
                {
                    DebugLog($"Retrying {retryCount + 1}/{maxRetryCount} for tile: {tile.Address}");

                    if (tile.LoadHandleCancellationTokenSource == null)
                        throw new OperationCanceledException("LoadHandleCancellationTokenSource is null.");
                    tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var retryResult = await Load(tile);
                    if (retryResult == LoadResult.Success)
                        return true;
                    else if (retryResult != LoadResult.NeedRetry)
                        break;

                    retryCount++;
                    await Task.Delay(delay, tile.LoadHandleCancellationTokenSource.Token);
                }
            }
            else if (result == LoadResult.Success)
                return true;
            return false;
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
            return await Load(tile);
        }

        /// <summary>
        /// Tileを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public bool Unload(PLATEAUDynamicTile tile)
        {
            string address = tile.Address;
            if (string.IsNullOrEmpty(address))
            {
                DebugLog($"指定したアドレスが見つかりません: {address}");
                return false;
            }
            try
            {
                if (tile.LoadHandle.IsValid())
                {
                    if (!tile.LoadHandle.IsDone)
                    {
                        // ロードが完了していない場合はキャンセル
                        tile.LoadHandleCancellationTokenSource?.Cancel();
                        return false;
                    }

                    if (!Addressables.ReleaseInstance(tile.LoadHandle))
                    {
                        throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {address}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog($"アセットのRelease中にエラーが発生しました: {address} {ex.Message}");
                if(tile.LoadedObject != null)
                {
                    // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                    DestroyImmediate(tile.LoadedObject);
                }
                tile.Reset();
            }
            return true;
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
        private void OnDestroy()
        {
            CancelLoadTask().ConfigureAwait(false); // 非同期タスクをキャンセル
            ClearTiles();
            jobSystem?.Dispose();
            jobSystem = null;
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
                        if (!Addressables.ReleaseInstance(tile.LoadHandle))
                        {
                            throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {tile.Address}");
                        }
                        tile.Reset();
                    }
                }
                catch (Exception ex)
                {
                    DebugLog($"タイルのアンロード中にエラーが発生しました: {tile.Address} {ex.Message}");
                    if (tile.LoadedObject != null)
                    {
                        // AddressablesのReleaseInstanceが失敗した場合、オブジェクトを破棄
                        if (Application.isPlaying)
                            Destroy(tile.LoadedObject);
                        else
                            DestroyImmediate(tile.LoadedObject);
                    }
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
                            try
                            {
                                if (!Addressables.ReleaseInstance(child.gameObject))
                                {
                                    throw new Exception($"AddressablesのReleaseInstanceに失敗しました: {child.gameObject.name}");
                                }
                            }
                            catch (Exception ex)
                            {
                                DebugLog($"GameObjectのアンロードでエラーが発生しました。{child.gameObject.name} {ex.Message}");
                                // アドレスのリリースに失敗した場合、直接破棄
                                if (Application.isPlaying)
                                    Destroy(child.gameObject);
                                else
                                    DestroyImmediate(child.gameObject);
                            }
                        }
                    }
                    // LODの親Transformも削除
                    if (Application.isPlaying)
                        Destroy(lodParent);
                    else
                        DestroyImmediate(lodParent);
                }
            }
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position)
        {
            if ( State != ManagerState.Operating)
                return;

            if (DynamicTiles.Count <= 0)
                return;

            // 前回のタスクがまだ完了していない場合は指定回数実行まで待機
            if (CurrentTask != null && !CurrentTask.IsCompleted)
            {
                NumPendingTask++;
                if (NumPendingTask < MAX_PENDING_TASK_COUNT)
                //if (Vector3.Distance(LastCameraPosition, position) < 100)
                {
                    //DebugLog("前回のタスクがまだ完了していません。タスクをキャンセルします。");
                    return;
                }

                await CancelLoadTask();
            }
            NumPendingTask = 0;

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
            foreach (var tile in DynamicTiles)
            {
                var distance = tile.GetDistance(position, true);
                //if (distance < loadDistance)
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
                    await LoadWithRetry(tile);
                }
                else if (tile.NextLoadState == LoadState.Unload)
                {
                    Unload(tile);
                }
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。(並列処理版)
        ///　（タイルが消えなくなる不具合があるため、現在は使用していない）
        /// </summary>
        private async Task ExecuteLoadTaskParallel(CancellationToken token, int maxConcurrency = 3)
        {
            using var sem = new SemaphoreSlim(maxConcurrency);
            var tasks = DynamicTiles.Select(async tile =>
            {
                await sem.WaitAsync(token);
                try
                {
                    if (tile.NextLoadState == LoadState.Load)
                        await LoadWithRetry(tile);
                    else if (tile.NextLoadState == LoadState.Unload)
                        Unload(tile);
                }
                finally
                {
                    sem.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 実行中のロードタスクをキャンセルし、CancellationTokenSourceをリセットします。
        /// </summary>
        public async Task CancelLoadTask()
        {
            LoadTaskCancellationTokenSource?.Cancel();
            LoadTaskCancellationTokenSource?.Dispose();
            LoadTaskCancellationTokenSource = new();

            if (HasCurrentTask)
                await CurrentTask;
        }

        /// <summary>
        /// 指定されたLODから親Transformを取得または作成します。
        /// </summary>
        /// <param name="lod"></param>
        /// <returns></returns>
        private Transform FindParent(int lod)
        {
            if(lodParentDict.TryGetValue(lod, out var parentTransform))
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
        /// 指定された距離がタイルのロード範囲内にあるかどうかを判定
        /// </summary>
        /// <param name="distance">カメラからの距離</param>
        /// <param name="tile">Tile</param>
        /// <returns></returns>
        internal bool WithinTheRange(float distance, PLATEAUDynamicTile tile)
        {
            var (min, max) = loadDistances[tile.ZoomLevel];
            return (distance > min && distance < max);
        }

        /// <summary>
        /// showDebugTileInfo:ON時のみDebugログに警告メッセージを出力します。
        /// </summary>
        /// <param name="message"></param>
        internal void DebugLog(string message, bool warn = true)
        {
            if (showDebugTileInfo)
            {
                if (warn)
                    Debug.LogWarning(message);
                else
                    Debug.Log(message);
            }
        }
    }
}