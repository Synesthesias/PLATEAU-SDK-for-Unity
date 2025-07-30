using PLATEAU.Util;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static PLATEAU.DynamicTile.PLATEAUTileManager;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileのロードとアンロードのTaskを管理するクラス。
    /// </summary>
    internal class PLATEAUDynamicTileLoadTask : IAsyncDisposable
    {

        // 全タイルのロードタスク実行時のCancellationTokenSource
        internal CancellationTokenSource LoadTaskCancellationTokenSource { get; private set; } = new();

        private readonly PLATEAUDynamicTileLoader tileLoader; // Addressablesからタイルをロード/アンロードするクラス
        private readonly PLATEAUDynamicTileInstantiation tileInstantiation; // ロード時インスタンス化とアンロードをキューで管理してコルーチンでインスタンス化を実行するクラス
        private PLATEAUDynamicTileJobSystem jobSystem; // JobSystemで距離判定を行う場合に使用

        // 実行中のUpdateAssetsByCameraPosition内のTask
        private Task CurrentTask;
        
        private readonly DynamicTileCollection dynamicTiles;
        private readonly TileLoadDistanceCollection tileLoadDistances;
        private readonly ConditionalLogger logger;

        /// <summary>
        /// 現在タスクが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool HasCurrentTask => CurrentTask != null && !CurrentTask.IsCompleted;

        /// <summary>
        /// Instance化のコルーチンが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool IsCoroutineRunning => tileInstantiation != null && tileInstantiation.IsRunning;

        public PLATEAUDynamicTileLoadTask(PLATEAUTileManager tileManager, ConditionalLogger logger)
        {
            this.tileLoadDistances = tileManager.loadDistances;
            this.logger = logger;
            dynamicTiles = tileManager.TileCollection;
            tileLoader = new(this, logger);
            tileInstantiation = new(logger, tileManager);
        }

        public async ValueTask DisposeAsync()
        {
            await DestroyTask();
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        public async Task DestroyTask()
        {
            await CancelLoadTask();
            jobSystem?.Dispose();
            jobSystem = null;

            tileLoader?.Dispose();
            tileInstantiation?.Dispose();
        }

        /// <summary>
        /// Job Systemを使用している場合、OnDisableでDisposeする
        /// </summary>
        public async Task DisableTask()
        {
            await CancelLoadTask();
            jobSystem?.Dispose();
            jobSystem = null;
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする
        /// </summary>
        public async Task<LoadResult> Load(PLATEAUDynamicTile tile, float timeoutSeconds = 2f)
        {
            if (tileLoader != null)
            {
                return await tileLoader.Load(tile, timeoutSeconds);
            }

            return LoadResult.Failure;
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
            if (tileLoader != null)
            {
                return await tileLoader.LoadWithRetry(tile, maxRetryCount, delaySeconds);
            }

            return LoadResult.Failure;
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timeoutSeconds">完了まで待機する際のタイムアウト秒数</param>
        /// <returns>処理を行ったかどうか</returns>
        public async Task<bool> UpdateAssetsByCameraPosition(Vector3 position, bool useJobSystem, float timeoutSeconds = 10f)
        {
            // 前回のタスクがまだ完了していない場合処理しない
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                return false;

            await CancelLoadTask();
            LoadTaskCancellationTokenSource?.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds)); // タイムアウト設定

            if (useJobSystem)
            {
                // Job Systemを使用する場合
                if (jobSystem != null && jobSystem?.TileCount != dynamicTiles.Count)
                {
                    // タイル数が変更された場合、Job SystemのNativeArrayを再初期化
                    await CancelLoadTask();
                    jobSystem?.Dispose();
                    jobSystem = null;
                }

                if (jobSystem == null)
                {
                    jobSystem = new PLATEAUDynamicTileJobSystem();
                    jobSystem.Initialize(this, tileLoadDistances, dynamicTiles, logger);
                }

                CurrentTask = jobSystem.UpdateAssetsByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                CurrentTask = UpdateAssetsByCameraPositionInternal(position);
            }

            await CurrentTask;
            return true;
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// JobSystemを使用しない場合の実装。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPositionInternal(Vector3 position)
        {
            var sortByDistance = dynamicTiles.ToList();
            sortByDistance.Sort((a, b) => a.DistanceFromCamera.CompareTo(b.DistanceFromCamera)); // Distance順にソート

            foreach (var tile in sortByDistance)
            {
                var distance = tile.GetDistance(position, true);
                if (tileLoadDistances.IsWithinRange(distance, tile.ZoomLevel))
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
                logger.LogWarn("タイルのロードTaskがキャンセルされました。");
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
            foreach (var tile in dynamicTiles)
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
            var result = await LoadWithRetry(tile);
            if (result == LoadResult.Success)
            {
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
        /// タイルのロードタスク後に実行する処理。
        /// Unloadキューに追加されたGameObjectインスタンスをアンロードします。
        /// </summary>
        internal void PostLoadTask()
        {
            tileInstantiation?.DeleteFromQueue(); // キューからGameObjectインスタンス削除
        }

    }
}
