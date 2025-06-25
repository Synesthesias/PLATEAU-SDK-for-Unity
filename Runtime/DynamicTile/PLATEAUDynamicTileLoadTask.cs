using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static PLATEAU.DynamicTile.PLATEAUTileManager;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileのロードとアンロードのTaskを管理するクラス。
    /// </summary>
    internal class PLATEAUDynamicTileLoadTask : IDisposable
    {
        private readonly PLATEAUTileManager tileManager;

        // 全タイルのロードタスク実行時のCancellationTokenSource
        internal CancellationTokenSource LoadTaskCancellationTokenSource { get; private set; } = new();

        private PLATEAUDynamicTileLoader tileLoader; // Addressablesからタイルをロード/アンロードするクラス
        private PLATEAUDynamicTileInstantiation tileInstantiation; // ロード時インスタンス化とアンロードをキューで管理してコルーチンでインスタンス化を実行するクラス
        private PLATEAUDynamicTileJobSystem jobSystem; // JobSystemで距離判定を行う場合に使用

        // 実行中のUpdateAssetsByCameraPosition内のTask
        private Task CurrentTask;

        public PLATEAUTileManager TileManager => tileManager;

        /// <summary>
        /// 現在タスクが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool HasCurrentTask => CurrentTask != null && !CurrentTask.IsCompleted;

        /// <summary>
        /// Instance化のコルーチンが実行中かどうかを示すプロパティ。
        /// </summary>
        public bool IsCoroutineRunning => tileInstantiation != null && tileInstantiation.IsRunning;

        public PLATEAUDynamicTileLoadTask(PLATEAUTileManager tileManager)
        {
            this.tileManager = tileManager;
            tileLoader = new(this);
            tileInstantiation = new(this);
        }

        public async void Dispose()
        {
            await OnDestroyTask();
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
        /// Tileを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public bool Unload(PLATEAUDynamicTile tile)
        {
            return tileLoader?.Unload(tile) ?? false;
        }

        /// <summary>
        /// 破棄時にすべてのロード済みオブジェクトをアンロード
        /// </summary>
        public async Task OnDestroyTask()
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
        public async Task OnDisableTask()
        {
            await CancelLoadTask();
            jobSystem?.Dispose();
            jobSystem = null;
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timeoutSeconds">完了まで待機する際のタイムアウト秒数</param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position, bool useJobSystem, float timeoutSeconds = 10f)
        {
            // 前回のタスクがまだ完了していない場合処理しない
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                return;

            await CancelLoadTask();
            LoadTaskCancellationTokenSource?.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds)); // タイムアウト設定

            if (useJobSystem)
            {
                if (jobSystem?.TileCount != tileManager.DynamicTiles.Count)
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
                    jobSystem.Initialize(this, tileManager?.DynamicTiles);
                }

                CurrentTask = jobSystem.UpdateAssetsByCameraPosition(position);
            }
            else
            {
                // Job Systemを使用しない場合
                CurrentTask = UpdateAssetsByCameraPositionInternal(position);
            }

            tileManager.UpdateCameraPosition(position); // 最後のカメラ位置を更新

            await CurrentTask;
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// JobSystemを使用しない場合の実装。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPositionInternal(Vector3 position)
        {
            var sortByDistance = new List<PLATEAUDynamicTile>(tileManager.DynamicTiles);
            sortByDistance.Sort((a, b) => a.DistanceFromCamera.CompareTo(b.DistanceFromCamera)); // Distance順にソート

            foreach (var tile in sortByDistance)
            {
                var distance = tile.GetDistance(position, true);
                if (tileManager.WithinTheRange(distance, tile))
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
                tileManager.DebugLog("タイルのロードTaskがキャンセルされました。");
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
            foreach (var tile in tileManager.DynamicTiles)
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
            tileInstantiation?.DeleteFromeQueue(); // キューからGameObjectインスタンス削除
        }

        public void DebugLog(string message, bool warn = true)
        {
            TileManager.DebugLog(message, warn);
        }
    }
}
