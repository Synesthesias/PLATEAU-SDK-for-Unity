using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    public enum LoadState
    {
        None,
        Load,
        Unload
    }

    /// <summary>
    /// タイルの範囲を表す構造体。
    /// </summary>
    public readonly struct TileBounds
    {
        public readonly Vector3 BoundsMin;
        public readonly Vector3 BoundsMax;
        public readonly int ZoomLevel;

        public TileBounds(Vector3 boundsMin, Vector3 boundsMax, int zoomLevel)//, LoadedState currentState)
        {
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
            ZoomLevel = zoomLevel;
        }

        // カメラからの距離を計算するメソッド。
        public float CalcDistance(Vector3 cameraPosition, bool ignoreY)
        {
            if (ignoreY)
            {
                Vector3 position2d = new(cameraPosition.x, 0, cameraPosition.z);
                Vector3 closestPoint3d = ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                Vector3 closestPoint2d = new(closestPoint3d.x, 0, closestPoint3d.z);
                return Vector3.Distance(position2d, closestPoint2d);
            }
            else
            {
                Vector3 closestPoint = ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                return Vector3.Distance(cameraPosition, closestPoint);
            }
        }

        // タイルの範囲とカメラの位置から最も近い点を計算するメソッド。
        Vector3 ClosestPointOnBounds(Vector3 position, Vector3 boundsMin, Vector3 boundsMax)
        {
            Bounds bounds = new();
            bounds.SetMinMax(boundsMin, boundsMax);
            return bounds.ClosestPoint(position);
        }

        // ↑の軽量処理。未使用 パフォーマンステスト用に残しておく。
        Vector3 ClosestPointOnBoundsSimple(Vector3 position, Vector3 boundsMin, Vector3 boundsMax)
        {
            float x = Mathf.Clamp(position.x, boundsMin.x, boundsMax.x);
            float y = Mathf.Clamp(position.y, boundsMin.y, boundsMax.y);
            float z = Mathf.Clamp(position.z, boundsMin.z, boundsMax.z);
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// タイルの距離とインデックスを保持する構造体。
    /// </summary>
    public readonly struct DistanceWithIndex
    {
        public readonly float Distance;
        public readonly int Index;
        public DistanceWithIndex(float distance, int index)
        {
            Distance = distance;
            Index = index;
        }
    }

    /// <summary>
    /// タイルの距離を計算するJobSystemのJob
    /// </summary>
    public struct TileDistanceCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<TileBounds> TileStates;
        [WriteOnly] public NativeArray<DistanceWithIndex> Distances;

        public Vector3 CameraPosition;
        public bool IgnoreY;

        void IJobParallelFor.Execute(int index)
        {
            var tile = TileStates[index];
            Distances[index] = new DistanceWithIndex(tile.CalcDistance(CameraPosition, IgnoreY), index);
        }
    }

    /// <summary>
    /// タイルの距離をソートするJobSystemのJob
    /// </summary>
    public struct SortDistancesJob : IJob
    {
        public NativeArray<DistanceWithIndex> Distances;

        public void Execute()
        {
            //Distances.Sort(new DistanceComparer());
            //NativeSortExtensionsが使えないので自前でソート
            int length = Distances.Length;
            for (int i = 0; i < length - 1; i++)
            {
                for (int j = 0; j < length - i - 1; j++)
                {
                    if (Distances[j].Distance > Distances[j + 1].Distance)
                    {
                        var temp = Distances[j];
                        Distances[j] = Distances[j + 1];
                        Distances[j + 1] = temp;
                    }
                }
            }
        }
    }

    //public struct DistanceComparer : IComparer<DistanceWithIndex>
    //{
    //    public int Compare(DistanceWithIndex x, DistanceWithIndex y)
    //    {
    //        return x.Distance.CompareTo(y.Distance);
    //    }
    //}

    /// <summary>
    /// Jobsystemを使用したタイルロード処理
    /// 今のところカメラ距離計算のみJob Systemを使用する。
    /// </summary>
    public class PLATEAUDynamicTileJobSystem : IDisposable
    {
        public NativeArray<TileBounds> NativeTileBounds;
        public NativeArray<DistanceWithIndex> NativeDistances;

        private List<PLATEAUDynamicTile> dynamicTiles; // タイルリスト
        private PLATEAUTileManager tileManager;

        public int TileCount => dynamicTiles?.Count ?? 0; // タイルの数を取得するプロパティ

        /// <summary>
        /// NativeArrayを初期化し、タイルマネージャーとタイルリストを設定する。
        /// 
        /// [Leak Detected : Persistent allocates 2 individual allocations.]
        /// Unity Editor上でNativeArrayをAllocator.Persistentで確保すると、Leak Detectedという警告が表示されるのは仕様です。
        /// これは、Unityのメモリ管理システムがEditor環境でのメモリリークを検出するための仕組みとして動作しているためです。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="tiles"></param>
        public void Initialize(PLATEAUTileManager manager, List<PLATEAUDynamicTile> tiles)
        {
            dynamicTiles = tiles;
            tileManager = manager;

            if (!NativeTileBounds.IsCreated || NativeTileBounds.Length != dynamicTiles.Count)
            {
                // 既存の配列を破棄
                Dispose();
            }

            if (!NativeTileBounds.IsCreated)
                NativeTileBounds = new NativeArray<TileBounds>(dynamicTiles.Count, Allocator.Persistent);
            if (!NativeDistances.IsCreated)
                NativeDistances = new NativeArray<DistanceWithIndex>(dynamicTiles.Count, Allocator.Persistent);

            for (int i = 0; i < dynamicTiles.Count; i++)
            {
                var tile = dynamicTiles[i];
                NativeTileBounds[i] = tile.GetTileBoundsStruct();
            }
        }

        // NativeArray PersistentなのでDispose必須　（メモリリーク防止のため）
        public void Dispose()
        {
            if (NativeTileBounds.IsCreated)
                NativeTileBounds.Dispose();
            if (NativeDistances.IsCreated)
                NativeDistances.Dispose();
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position)
        {
            TileDistanceCheckJob job = new TileDistanceCheckJob { TileStates = NativeTileBounds, Distances = NativeDistances, CameraPosition = position, IgnoreY = false };
            JobHandle handle = job.Schedule(NativeTileBounds.Length, 64);
            handle.Complete();

            //距離が近い順にソート
            JobHandle sortHandle = new SortDistancesJob { Distances = NativeDistances }.Schedule();
            sortHandle.Complete();

            try
            {
                await ExecuteLoadTask(tileManager.LoadTaskCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                tileManager.DebugLog("タイルのロードTaskがキャンセルされました。");
            }
            finally
            {
                tileManager.PostLoadTask();
            }
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// </summary>
        private async Task ExecuteLoadTask(CancellationToken token)
        {
            int loadFailCount = 0;
            int unLoadFailCount = 0;

            for (int i = 0; i < NativeDistances.Length; i++)
            {
                var distanceWithIndex = NativeDistances[i];
                var distance = distanceWithIndex.Distance;
                var index = distanceWithIndex.Index;
                var tile = dynamicTiles[index];

                var nextLoadState = LoadState.None;
                if (tileManager.WithinTheRange(distance, tile))
                {
                    nextLoadState = LoadState.Load;
                }
                else
                {
                    nextLoadState = LoadState.Unload;
                }

                tile.DistanceFromCamera = distance;
                tile.NextLoadState = nextLoadState;

                if (nextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (nextLoadState == LoadState.Load && !tile.LoadHandle.IsValid())
                {
                    var success = await tileManager.LoadWithRetry(tile);

                    if (!success)
                        loadFailCount++;
                    
                }
                else if (nextLoadState == LoadState.Unload && tile.LoadHandle.IsValid())
                {
                    var success = tileManager.Unload(tile);

                    if (!success)
                        unLoadFailCount++;
                }
                token.ThrowIfCancellationRequested();
            }

            tileManager.DebugLog($"タイルのロード・アンロード処理が完了しました。ロード失敗数: {loadFailCount}, アンロード失敗数: {unLoadFailCount}");
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。(並列処理版)
        ///　（タイルが消えなくなる不具合があるため、現在は使用していない）
        /// </summary>
        private async Task ExecuteLoadTaskParallel(CancellationToken token, int maxConcurrency = 3)
        {
            using var sem = new SemaphoreSlim(maxConcurrency);
            var tasks = dynamicTiles.Select(async (tile, i) =>
            {
                var distanceWithIndex = NativeDistances[i];
                var distance = distanceWithIndex.Distance;
                var index = distanceWithIndex.Index;

                // ロード・アンロード判定
                tile.DistanceFromCamera = distance;
                //tile.NextLoadState = distance < tileManager.loadDistance
                tile.NextLoadState = tileManager.WithinTheRange(distance, tile)
                    ? LoadState.Load
                    : LoadState.Unload;

                await sem.WaitAsync(token);
                try
                {
                    if (tile.NextLoadState == LoadState.Load && !tile.LoadHandle.IsValid())
                        await tileManager.LoadWithRetry(tile);
                    else if (tile.NextLoadState == LoadState.Unload && tile.LoadHandle.IsValid())
                        tileManager.Unload(tile);
                }
                finally
                {
                    sem.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);
        }
    }

}
