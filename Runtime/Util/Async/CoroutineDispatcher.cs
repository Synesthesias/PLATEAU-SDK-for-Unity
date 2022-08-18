using System.Collections;
using UnityEngine;

namespace PLATEAU.Util.Async
{
    /// <summary>
    /// <see href="https://zenn.dev/meson/articles/implement-awaiter-for-unity"/>
    /// </summary>
    public class CoroutineDispatcher : MonoBehaviour
    {
        public static CoroutineDispatcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void Dispatch(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}