using System;
using System.Collections.Generic;
using System.Linq;
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

        private int forceHighResTileAddressesHash = 0;

        public PLATEAUDynamicTileLoadTask(PLATEAUTileManager tileManager)
        {
            this.tileManager = tileManager;
            tileLoader = new(this);
            tileInstantiation = new(this);
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
        /// Tileを指定してAddressablesからアンロードする
        /// </summary>
        /// <param name="address"></param>
        public bool Unload(PLATEAUDynamicTile tile)
        {
            return tileLoader?.Unload(tile) ?? false;
        }

        /// <summary>
        /// カメラの位置に応じてタイルのロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timeoutSeconds">完了まで待機する際のタイムアウト秒数</param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position, bool ignoreY, float timeoutSeconds = 10f , IEnumerable<string> forceHighResTileAddresses = null)
        {
            // 前回のタスクがまだ完了していない場合処理しない
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                return;

            await CancelLoadTask();
            LoadTaskCancellationTokenSource?.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds)); // タイムアウト設定


            var hash = forceHighResTileAddresses?.GetHashCode() ?? 0;
            if (jobSystem != null && (jobSystem?.TileCount != tileManager.DynamicTiles.Count || forceHighResTileAddressesHash != hash))
            {
                // タイル数が変更された or 強制高解像度タイル情報が更新された場合、Job SystemのNativeArrayを再初期化
                await CancelLoadTask();
                jobSystem?.Dispose();
                jobSystem = null;
                forceHighResTileAddressesHash = hash;

            }

            if (jobSystem == null)
            {
                jobSystem = new PLATEAUDynamicTileJobSystem();
                jobSystem.Initialize(this, tileManager?.DynamicTiles, forceHighResTileAddresses);
            }

            CurrentTask = jobSystem.UpdateAssetsByCameraPosition(position, ignoreY);
            
           
            tileManager.UpdateCameraPosition(position); // 最後のカメラ位置を更新

            await CurrentTask;
        }

      
        /// <summary>
        /// タイルの穴埋め処理を非同期で実行する。
        /// Unload状態のタイルはその階層の子タイルを個別に判定してロード状態を補正する。
        /// zoomLevel 9 ～ zoomLevel 11まで対応　(zoomLevel追加時は改修が必要）
        /// </summary>
        /// <returns></returns>
        internal Task FillTileHoles(CancellationToken token)
        {
            return Task.Run(() => {
                List<PLATEAUDynamicTile> zoom9Tiles = tileManager.DynamicTiles.Where(t => t.ZoomLevel == 9 && t.NextLoadState == LoadState.Unload && tileManager.WithinMaxRange(t.DistanceFromCamera, t)).ToList();
                foreach (var zoom9Tile in zoom9Tiles)
                {
                    List<PLATEAUDynamicTile> zoom10Tiles = zoom9Tile.ChildrenTiles.Where(t => t.NextLoadState == LoadState.Unload).ToList();
                    foreach (var zoom10Tile in zoom10Tiles)
                    {
                        List<PLATEAUDynamicTile> zoom11Tiles = zoom10Tile.ChildrenTiles.Where(t => t.NextLoadState == LoadState.Unload).ToList();
                        if (zoom11Tiles.Count == zoom10Tile.ChildrenTiles.Count) // 子が全てUnloadの場合
                        {
                            zoom10Tile.NextLoadState = LoadState.Load; // 上位タイルをロード状態にする
                        }
                        else
                        {
                            // 子のうち一部がロード状態の場合は、子の全てをロード状態にする
                            foreach (var zoom11Tile in zoom11Tiles)
                            {
                                zoom11Tile.NextLoadState = LoadState.Load;
                                token.ThrowIfCancellationRequested();
                            }
                        }
                        token.ThrowIfCancellationRequested();
                    }
                    token.ThrowIfCancellationRequested();
                }
            }, token);
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

        public void DebugLog(string message, bool warn = true)
        {
            TileManager.DebugLog(message, warn);
        }
    }
}
