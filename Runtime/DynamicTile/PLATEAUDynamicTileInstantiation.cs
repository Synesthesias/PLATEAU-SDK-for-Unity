using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// インスタンス化、GameObject削除を管理するクラス。
    /// インスタンス化はコルーチンで毎フレーム１オブジェクトずつ行う。
    /// </summary>
    public class PLATEAUDynamicTileInstantiation : IDisposable
    {
        private PLATEAUTileManager manager;

        private Queue<PLATEAUDynamicTile> loadQueue = new Queue<PLATEAUDynamicTile>();
        private Queue<PLATEAUDynamicTile> unloadQueue = new Queue<PLATEAUDynamicTile>();

        // Editr/Runtime共にコルーチン停止用に保持
        private Coroutine instantiationFromQueueCoroutine;
        private Coroutine instantiationFromTileCoroutine;
        private IEnumerator instantiationFromQueueEnumerator;
        private IEnumerator instantiationFromTileEnumerator;

        private bool isInstantiationFromQueueRunning = false;
        private bool isInstantiationFromTileRunning = false;

        public bool IsRunning => (isInstantiationFromQueueRunning || isInstantiationFromTileRunning );

        public PLATEAUDynamicTileInstantiation(PLATEAUTileManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// キューにタイルを追加します。
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
                instantiationFromQueueCoroutine = manager.StartCoroutine(InstantiationFromQueueRoutine());
            }
            else
            {
                instantiationFromQueueEnumerator = EditorCoroutineRunner.StartEditorCoroutine(InstantiationFromQueueRoutine());
            }

#else
                instantiationFromQueueCoroutine = manager.StartCoroutine(InstantiationFromQueueRoutine());
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

                if (!manager.InstantiateFromTile(tile))
                {
                    manager.DebugLog($"タイルのインスタンス化に失敗しました: {tile.Address}");
                }

                yield return null; // フレームごとに処理を実行
            }

            isInstantiationFromQueueRunning = false;
        }

        /// <summary>
        /// キューを使用せず、タイルのロード状態に基づいてインスタンス化を行います。
        ///　未使用
        /// </summary>
        internal void StartInstantiationFromTile()
        {


#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (isInstantiationFromTileRunning)
                    manager.StopCoroutine(instantiationFromTileCoroutine); // 既に実行中の場合は停止して再実行
                instantiationFromTileCoroutine = manager.StartCoroutine(InstantiationFromTileRoutine());
            }
            else
            {
                if (isInstantiationFromTileRunning)
                    EditorCoroutineRunner.StopEditorCoroutine(instantiationFromTileEnumerator); // 既に実行中の場合は停止して再実行
                instantiationFromTileEnumerator = EditorCoroutineRunner.StartEditorCoroutine(InstantiationFromTileRoutine());
            }

#else
            if (isInstantiationFromTileRunning)
                manager.StopCoroutine(instantiationFromTileCoroutine); // 既に実行中の場合は停止して再実行
            instantiationFromTileCoroutine = manager.StartCoroutine(InstantiationFromTileRoutine());
#endif
        }

        /// <summary>
        /// キューを使用せず、タイルのロード状態に基づいてインスタンス化する処理のコルーチン。
        /// </summary>
        /// <returns></returns>
        private IEnumerator InstantiationFromTileRoutine()
        {
            isInstantiationFromTileRunning = true;

            var loadList = manager.DynamicTiles.FindAll(tile => tile.NextLoadState == LoadState.Load && tile.LoadedObject == null);
            var unloadList = manager.DynamicTiles.FindAll(tile => tile.NextLoadState == LoadState.Unload && tile.LoadedObject != null);
            loadList.Sort((b, a) => a.DistanceFromCamera.CompareTo(b.DistanceFromCamera)); // DistanceFromCameraでソート

            // Instantiate処理
            for (int i = 0; i < loadList.Count; i++)
            {
                var tile = loadList[i];
                if (tile == null)
                    continue;

                if (!manager.InstantiateFromTile(tile))
                {
                    manager.DebugLog($"タイルのインスタンス化に失敗しました: {tile.Address} {i}");
                }

                yield return null; // フレームごとに処理を実行
            }

            // Delete処理
            for (int i = 0; i < unloadList.Count; i++)
            {
                var tile = unloadList[i];
                if (tile == null)
                    continue;

                manager.DeleteGameObjectInstance(tile.LoadedObject);
            }

            loadList.Clear();
            unloadList.Clear();

            isInstantiationFromTileRunning = false;
        }

        /// <summary>
        /// Unloadにキューされたタイルを全て削除します。
        /// </summary>
        public void DeleteFromeQueue()
        {
            while (unloadQueue.Count > 0)
            {
                var tile = unloadQueue.Dequeue();
                if (tile == null)
                    continue;

                manager.DeleteGameObjectInstance(tile.LoadedObject);
            }

            unloadQueue.Clear();
        }

        public void Dispose()
        {
            manager.StopCoroutine(instantiationFromQueueCoroutine);
            manager.StopCoroutine(instantiationFromTileCoroutine);

#if UNITY_EDITOR
            EditorCoroutineRunner.StopAllEditorCoroutines();
#endif
        }

    }

#if UNITY_EDITOR

    /// <summary>
    /// Unityエディタ上でコルーチンを実行するためのクラス。
    /// </summary>
    public class EditorCoroutineRunner
    {
        private static List<IEnumerator> coroutines = new List<IEnumerator>();

        public static IEnumerator StartEditorCoroutine(IEnumerator coroutine)
        {
            if (coroutines.Count == 0)
            {
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            }
            coroutines.Add(coroutine);
            return coroutine;
        }

        public static void StopEditorCoroutine(IEnumerator coroutine)
        {
            if (coroutines.Remove(coroutine) && coroutines.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }

        public static bool IsRunning()
        {
            return coroutines.Count > 0;
        }

        public static void StopAllEditorCoroutines()
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
