using PLATEAU.Util;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static PLATEAU.DynamicTile.PLATEAUTileManager;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileのロードとアンロードを管理するクラス。
    /// </summary>
    internal class PLATEAUDynamicTileLoader : IDisposable
    {

        private readonly PLATEAUDynamicTileLoadTask loadTask;
        private readonly ConditionalLogger logger;

        internal PLATEAUDynamicTileLoader(PLATEAUDynamicTileLoadTask loadTask, ConditionalLogger logger)
        {
            this.loadTask = loadTask;
            this.logger = logger;
        }

        public void Dispose()
        {
            // 現在は解放すべきリソースなし
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする
        /// TileのLoadHandleにAddressables.LoadAssetAsync設定
        /// ロード成功時にTileのLoadHandleからResultを取得して利用する
        /// </summary>
        internal async Task<LoadResult> Load(PLATEAUDynamicTile tile, float timeoutSeconds = 2f)
        {
            string address = tile.Address;
            if (string.IsNullOrEmpty(address))
            {
                logger.LogWarn($"指定したアドレスが見つかりません: {address}");
                return await Task.FromResult<LoadResult>(LoadResult.Failure);
            }
            // 既にロードされている場合はスキップ
            if (tile.LoadHandle.IsValid() || tile.LoadedObject != null)
            {
                logger.Log($"Already loaded: {address}");
                return await Task.FromResult<LoadResult>(LoadResult.AlreadyLoaded);
            }

            try
            {
                if (tile.LoadHandle.IsValid() && !tile.LoadHandle.IsDone)
                {
                    tile.LoadHandleCancellationTokenSource.Cancel();
                    try
                    {
                        await tile.LoadHandle.Task;
                    }
                    catch (OperationCanceledException){ /*キャンセル済みタスクの完了待機時の例外は無視*/ }

                    tile.LoadHandleCancellationTokenSource.Dispose();
                    tile.LoadHandleCancellationTokenSource = null;
                }

                if (tile.LoadHandleCancellationTokenSource == null)
                {
                    // Addressablesでは、Cancel処理がサポートされていないため、CancellationTokenSourceを使用してキャンセル可能なロードを実装
                    tile.LoadHandleCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(loadTask.LoadTaskCancellationTokenSource.Token);
                }

                var timeoutTask = Task.Delay((int)(timeoutSeconds * 1000), tile.LoadHandleCancellationTokenSource.Token);

                //Cancel処理
                tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                // Addressablesからアセットを非同期でロード
                tile.LoadHandle = Addressables.LoadAssetAsync<GameObject>(address);

                //Cancel処理
                tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                // タイムアウト付きロード
                var completedTask = await Task.WhenAny(tile.LoadHandle.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"アセットのロードがタイムアウトしました: {address}");
                }

                // ロード成功時
                if (tile.LoadHandle.IsValid() && tile.LoadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // ロードに成功した場合は、LoadHandleCancellationTokenSourceをDisposeしてnullに設定
                    tile.LoadHandleCancellationTokenSource?.Dispose();
                    tile.LoadHandleCancellationTokenSource = null; // Dispose後はnullにする
                    return LoadResult.Success;
                }
                else
                {
                    if (tile.LoadHandle.IsValid())
                        Addressables.Release(tile.LoadHandle);

                    tile.Reset();
                    logger.LogWarn($"アセットのロードに失敗しました: {address}");
                    return LoadResult.Failure;
                }
            }
            catch (Exception ex)
            {
                var loadResult = LoadResult.Failure;
                if (ex is OperationCanceledException)
                {
                    logger.LogWarn($"アセットのロードがキャンセルされました: {address}");
                    loadResult = LoadResult.Cancelled;
                }
                else if (ex is TimeoutException)
                {
                    logger.LogWarn($"アセットのロードがタイムアウトしました: {address}");
                    loadResult = LoadResult.Timeout;
                }
                else
                {
                    logger.LogWarn($"アセットのロード中にエラーが発生しました: {address} {ex.Message}");
                }

                if (tile.LoadHandle.IsValid())
                    Addressables.Release(tile.LoadHandle);

                tile.Reset();
                return loadResult;
            }
        }

        /// <summary>
        /// Tileを指定してAddressablesからロードする (リトライ機能付き)
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="maxRetryCount">リトライ数</param>
        /// <returns></returns>
        internal async Task<LoadResult> LoadWithRetry(PLATEAUDynamicTile tile, int maxRetryCount = 2, float delaySeconds = 0.3f)
        {
            var result = await Load(tile);
            if (result != LoadResult.Success && result != LoadResult.Cancelled)
            {
                // ロードに失敗した場合は、リトライ
                int retryCount = 0;
                while (retryCount < maxRetryCount)
                {
                    logger.LogWarn($"タイルのロードに失敗しました。リトライします: {tile.Address} Count: {retryCount}");

                    if (tile.LoadHandleCancellationTokenSource == null)
                        throw new OperationCanceledException("LoadHandleCancellationTokenSource is null.");
                    tile.LoadHandleCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var retryResult = await Load(tile);
                    if (retryResult == LoadResult.Success)
                        return retryResult;

                    retryCount++;
                    await Task.Delay((int)(delaySeconds * 1000), tile.LoadHandleCancellationTokenSource.Token);
                }
            }
            else if (result == LoadResult.Success)
            {
                tile.LastLoadResult = result;
                return result;
            }

            logger.Log($"タイルのロードのリトライに失敗しました: {tile.Address}");

            tile.LastLoadResult = result; // 最後のロード結果を保存
            return result;
        }

    }
}
