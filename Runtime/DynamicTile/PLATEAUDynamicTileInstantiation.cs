using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// AddressablesでのLoad後のTileインスタンス化、GameObject削除を管理するクラス。
    /// インスタンス化はコルーチンで毎フレーム１オブジェクトずつ行う。
    /// </summary>
    internal class PLATEAUDynamicTileInstantiation : IDisposable
    {
        private readonly PLATEAUDynamicTileLoadTask loadTask;
        private readonly MonoBehaviour mono;

        private Queue<PLATEAUDynamicTile> loadQueue = new Queue<PLATEAUDynamicTile>();
        private Queue<PLATEAUDynamicTile> unloadQueue = new Queue<PLATEAUDynamicTile>();

        // Runtimeコルーチン停止用に保持
        private Coroutine instantiationFromQueueCoroutine;

        private bool isInstantiationFromQueueRunning = false;

        public bool IsRunning => isInstantiationFromQueueRunning;

        internal PLATEAUDynamicTileInstantiation(PLATEAUDynamicTileLoadTask loadTask)
        {
            this.loadTask = loadTask;
            this.mono = loadTask.TileManager as MonoBehaviour;
        }

        /// <summary>
        /// キューにタイルを追加してコルーチンを実行します。
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="load">true:load, false:unload</param>
        internal void AddToQueue(PLATEAUDynamicTile tile, bool load)
        {
            if (tile == null)
                return;
            if (load)
                loadQueue.Enqueue(tile);
            else
                unloadQueue.Enqueue(tile);

            StartInstantiationFromQueue();
        }

        /// <summary>
        /// ロード完了後のInstantiateをキューから実行します。
        /// </summary>
        internal void StartInstantiationFromQueue()
        {
            if (isInstantiationFromQueueRunning)
                return; // 既に実行中の場合は実行しない

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                instantiationFromQueueCoroutine = mono.StartCoroutine(InstantiationFromQueueRoutine());
            }
            else
            {
                EditorCoroutineRunner.StartEditorCoroutine(InstantiationFromQueueRoutine());
            }

#else
                instantiationFromQueueCoroutine = mono.StartCoroutine(InstantiationFromQueueRoutine());
#endif
        }

        /// <summary>
        /// Instance化処理のコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiationFromQueueRoutine()
        {
            isInstantiationFromQueueRunning = true;

            while(loadQueue.Count > 0)
            {
                var tile = loadQueue.Dequeue();
                if (tile == null)
                    continue;

                if (!loadTask.TileManager.InstantiateFromTile(tile))
                {
                    loadTask.DebugLog($"タイルのインスタンス化に失敗しました: {tile.Address}");
                }

                yield return null; // フレームごとに処理を実行
            }

            isInstantiationFromQueueRunning = false;
        }

        /// <summary>
        /// Unloadにキューされたタイルを全てUnloadします。
        /// </summary>
        internal void DeleteFromQueue()
        {
            while (unloadQueue.Count > 0)
            {
                var tile = unloadQueue.Dequeue();
                if (tile == null)
                    continue;

                loadTask.Unload(tile);
            }

            unloadQueue.Clear();
        }

        public void Dispose()
        {
            if (instantiationFromQueueCoroutine != null)
                mono.StopCoroutine(instantiationFromQueueCoroutine);

#if UNITY_EDITOR
            EditorCoroutineRunner.StopAllEditorCoroutines();
#endif
        }

    }

#if UNITY_EDITOR

    /// <summary>
    /// Unityエディタ上でコルーチンを実行するためのクラス。
    /// </summary>
    internal class EditorCoroutineRunner
    {
        private static List<IEnumerator> coroutines = new List<IEnumerator>();

        internal static void StartEditorCoroutine(IEnumerator coroutine)
        {
            if (coroutines.Count == 0)
            {
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            }
            coroutines.Add(coroutine);
        }

        public static bool IsRunning()
        {
            return coroutines.Count > 0;
        }

        internal static void StopEditorCoroutine(IEnumerator coroutine)
        {
            if (coroutines.Remove(coroutine) && coroutines.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }

        internal static void StopAllEditorCoroutines()
        {
            coroutines.Clear();
            EditorApplication.update -= Update;
        }

        private static void Update()
        {
            if (coroutines.Count == 0)
            {
                EditorApplication.update -= Update;
                return;
            }

            for (int i = coroutines.Count - 1; i >= 0; i--)
            {
                if (!coroutines[i].MoveNext())
                {
                    coroutines.RemoveAt(i);
                }
            }
        }
    }
#endif
}
