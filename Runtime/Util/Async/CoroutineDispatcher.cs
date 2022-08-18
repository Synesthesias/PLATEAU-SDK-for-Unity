using System.Collections;
using UnityEngine;

namespace PLATEAU.Util.Async
{
    /// <summary>
    /// <see cref="UnityWebRequestExtension"/> から利用されます。
    /// async/await を使ってコルーチンを実行する拡張機能のうち、実際にコルーチンを実行するゲームオブジェクトの機能です。
    /// <see href="https://zenn.dev/meson/articles/implement-awaiter-for-unity"/>
    /// </summary>
    internal class CoroutineDispatcher : MonoBehaviour
    {
        private static CoroutineDispatcher instance;
        public static CoroutineDispatcher Instance
        {
            get
            {
                if (instance == null) instance = FindOrCreateInstance();
                return instance;
            }
        }

        private static CoroutineDispatcher FindOrCreateInstance()
        {
            var dispatcher = FindObjectOfType<CoroutineDispatcher>();
            if (dispatcher == null)
            {
                var obj = new GameObject(nameof(CoroutineDispatcher));
                dispatcher = obj.AddComponent<CoroutineDispatcher>();
            }

            return dispatcher;
        }

        public void Dispatch(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}