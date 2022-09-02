using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace PLATEAU.Util.Async
{
    /// <summary>
    /// UnityWebRequest を await できるようにするための拡張です。
    /// <seealso href="https://zenn.dev/meson/articles/implement-awaiter-for-unity"/>
    /// </summary>
    internal static class UnityWebRequestExtension
    {
        public static UnityWebRequestAwaitable GetAwaiter(this UnityWebRequestAsyncOperation operation)
        {
            return new UnityWebRequestAwaitable(operation);
        }

        public class UnityWebRequestAwaitable : INotifyCompletion
        {
            private UnityWebRequestAsyncOperation operation;
            private Action continuation;

            public UnityWebRequestAwaitable(UnityWebRequestAsyncOperation operation)
            {
                this.operation = operation;
                CoroutineDispatcher.Instance.Dispatch(CheckLoop());
            }

            public bool IsCompleted => this.operation.isDone;

            public void OnCompleted(Action continuationArg) => this.continuation = continuationArg;

            public void GetResult() { }

            private IEnumerator CheckLoop()
            {
                yield return this.operation;
                this.continuation?.Invoke();
            }
        }
    }
}