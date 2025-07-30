using PLATEAU.Util;
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

        private readonly Queue<PLATEAUDynamicTile> loadQueue = new Queue<PLATEAUDynamicTile>();
        private readonly Queue<PLATEAUDynamicTile> unloadQueue = new Queue<PLATEAUDynamicTile>();
        private readonly ConditionalLogger logger;

        // Runtimeコルーチン停止用に保持
        private Coroutine instantiationFromQueueCoroutine;

        private bool isInstantiationFromQueueRunning = false;

        public bool IsRunning => isInstantiationFromQueueRunning;
        private readonly PLATEAUTileManager tileManager;

        internal PLATEAUDynamicTileInstantiation(ConditionalLogger logger, PLATEAUTileManager tileManager)
        {
            this.logger = logger;
            this.tileManager = tileManager;
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
                instantiationFromQueueCoroutine = tileManager.StartCoroutine(InstantiationFromQueueRoutine());
            }
            else
            {
                EditorCoroutineRunner.StartEditorCoroutine(InstantiationFromQueueRoutine());
            }

#else
                instantiationFromQueueCoroutine = tileManager.StartCoroutine(InstantiationFromQueueRoutine());
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

                if (!tileManager.InstantiateFromTile(tile))
                {
                    logger.LogWarn($"タイルのインスタンス化に失敗しました: {tile.Address}");
                    tile.LastLoadResult = PLATEAUTileManager.LoadResult.Failure;
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

                tile.Unload();
            }

            unloadQueue.Clear();
        }

        public void Dispose()
        {
            if (instantiationFromQueueCoroutine != null)
                tileManager.StopCoroutine(instantiationFromQueueCoroutine);

#if UNITY_EDITOR
            EditorCoroutineRunner.StopAllEditorCoroutines();
#endif
        }

    }

#if UNITY_EDITOR

    /// <summary>
    /// Unityエディタ上でコルーチンを実行するためのクラス。
    /// </summary>
    internal static class EditorCoroutineRunner
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
