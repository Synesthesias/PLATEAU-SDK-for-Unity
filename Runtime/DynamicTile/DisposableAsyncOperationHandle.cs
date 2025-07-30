using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// AsyncOperationHandleのラッパークラスです。
    /// using文で自動的にReleaseされるので、すぐReleaseしたいケースでのRelease忘れを防止できます。
    /// </summary>
    public class DisposableAsyncOperationHandle<T> : IDisposable
    {
        private AsyncOperationHandle<T> handle;
        private bool disposed;

        /// <summary>
        /// 内部のAsyncOperationHandle
        /// </summary>
        public AsyncOperationHandle<T> Handle => handle;

        /// <summary>
        /// 操作の結果
        /// </summary>
        public T Result => handle.IsValid() ? handle.Result : default;

        /// <summary>
        /// 操作の状態
        /// </summary>
        public AsyncOperationStatus Status => handle.IsValid() ? handle.Status : AsyncOperationStatus.None;
        

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handle">ラップするAsyncOperationHandle</param>
        public DisposableAsyncOperationHandle(AsyncOperationHandle<T> handle)
        {
            this.handle = handle;
        }
        

        /// <summary>
        /// リソースを解放します
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドリソースを解放するかどうか</param>
        private void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                disposed = true;
            }
        }
        
    }

    public static class DisposableAsyncOperationHandleEx
    {
        /// <summary>
        /// AsyncOperationHandleを変換し、usingで自動的にReleaseされるDisposableAsyncOperationHandleにします。
        /// </summary>
        public static DisposableAsyncOperationHandle<T> ToDisposable<T>(this AsyncOperationHandle<T> handle)
        {
            return new DisposableAsyncOperationHandle<T>(handle);
        }
    }
    
} 