using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

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

        public TileBounds(Vector3 boundsMin, Vector3 boundsMax, int zoomLevel)
        {
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
            ZoomLevel = zoomLevel;
        }

        // カメラからの距離を計算するメソッド。
        public float CalcDistance(Vector3 cameraPosition, bool ignoreY, bool useLight = false)
        {
            if (ignoreY)
            {
                Vector3 position2d = new(cameraPosition.x, 0, cameraPosition.z);
                Vector3 closestPoint3d = useLight ? 
                    ClosestPointOnBoundsLight(cameraPosition, BoundsMin, BoundsMax) : 
                    ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                Vector3 closestPoint2d = new(closestPoint3d.x, 0, closestPoint3d.z);
                return Vector3.Distance(position2d, closestPoint2d);
            }
            else
            {
                Vector3 closestPoint = useLight ? 
                    ClosestPointOnBoundsLight(cameraPosition, BoundsMin, BoundsMax) : 
                    ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
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
        Vector3 ClosestPointOnBoundsLight(Vector3 position, Vector3 boundsMin, Vector3 boundsMax)
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
    public struct DistanceWithIndex : IComparable<DistanceWithIndex>
    {
        public readonly float Distance;
        public readonly int Index;
        public readonly int ZoomLevel;
        public LoadState State;
        public bool WithinMaxRange;

        public DistanceWithIndex(float distance, int index, int zoomLevel)
        {
            Distance = distance;
            Index = index;
            ZoomLevel = zoomLevel;
            State = LoadState.None; // 初期状態はNone
            WithinMaxRange = false;
        }
        public int CompareTo(DistanceWithIndex other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    /// <summary>
    /// タイルの子タイル(下位ズームレベル)を保持する構造体。
    /// PLATEAUDynamicTile.ChildrenTilesの情報を配列indexで保持する。
    /// fixed配列だとunsafeにする必要があるため、構造体で保持する。
    /// </summary>
    public readonly struct ChildrenTiles
    {
        public readonly int tile1;
        public readonly int tile2;
        public readonly int tile3;
        public readonly int tile4;
        public readonly int Length;

        public ChildrenTiles(IEnumerable<int> tiles)
        {
            tile1 = tiles.Count() > 0 ? tiles.ElementAt(0) : 0;
            tile2 = tiles.Count() > 1 ? tiles.ElementAt(1) : 0;
            tile3 = tiles.Count() > 2 ? tiles.ElementAt(2) : 0;
            tile4 = tiles.Count() > 3 ? tiles.ElementAt(3) : 0;
            Length = tiles.Count() > 4 ? 4 : tiles.Count(); // 最大4つの子タイルを保持する
        }

        public int[] ToArray()
        {
            int[] array = new int[Length];
            for (int i = 0; i < Length; i++)
            {
                array[i] = i switch
                {
                    0 => tile1,
                    1 => tile2,
                    2 => tile3,
                    3 => tile4,
                    _ => 0 // それ以上のインデックスは存在しない
                };
            }
            return array;
        }
    }

    /// <summary>
    /// 各Zoomレベルごとのカメラからのロード距離を保持する構造体。
    /// NativeLoadDistancesで使用される。
    /// </summary>
    public struct FloatMinMax
    {
        public float min;
        public float max;

        public bool WithinTheRange(float distance)
        {
             return (distance >= min && distance <= max);
        }
        public bool WithinMaxRange(float distance)
        {
             return distance <= max;
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
            var zoomLevel = tile.ZoomLevel;
            Distances[index] = new DistanceWithIndex(tile.CalcDistance(CameraPosition, IgnoreY), index, zoomLevel);
        }
    }

    /// <summary>
    /// タイルの範囲をチェックするJobSystemのJob
    /// 範囲内であればLoadStateをLoadに設定し、範囲外であればUnloadに設定する。
    /// </summary>
    public struct TileRangeCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeHashMap<int, FloatMinMax> LoadDistances;
        public NativeArray<DistanceWithIndex> Distances;

        void IJobParallelFor.Execute(int index)
        {
            var distanceWithIndex = Distances[index];
            var zoomLevel = distanceWithIndex.ZoomLevel;

            if (LoadDistances[zoomLevel].WithinTheRange(distanceWithIndex.Distance))
            {
                // 距離が範囲内の場合はLoadStateをLoadに設定
                distanceWithIndex.State = LoadState.Load;
                distanceWithIndex.WithinMaxRange = true;
            }
            else
            {
                // 距離が範囲外の場合はLoadStateをUnloadに設定
                distanceWithIndex.State = LoadState.Unload;
                distanceWithIndex.WithinMaxRange = LoadDistances[zoomLevel].WithinMaxRange(distanceWithIndex.Distance);
            }
            Distances[index] = distanceWithIndex;
        }
    }

    /// <summary>
    /// タイルの穴埋め処理を行うJobSystemのJob
    /// FillTileHolesと同様の処理
    /// 注意：ソート処理でNativeArray<DistanceWithIndex>のindexが変更される前に実行する必要がある。
    /// </summary>
    public struct FillTileHolesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ChildrenTiles> Childrens;

        [NativeDisableParallelForRestriction]
        public NativeArray<DistanceWithIndex> Distances;

        private DistanceWithIndex[] GetChildren(int[] indices)
        {
            var distArray = Distances.ToArray();
            DistanceWithIndex[] children = new DistanceWithIndex[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < 0 || indices[i] >= Distances.Length)
                {
                    throw new IndexOutOfRangeException($"Invalid index {indices[i]} for Distances array.");
                }
                children[i] = distArray[indices[i]];
            }
            return children;
        }

        public void Execute(int index)
        {
            // タイルの穴埋め処理
            var z9UnloadedTiles = Distances.ToArray().Where(t => t.ZoomLevel == 9 && t.State == LoadState.Unload && t.WithinMaxRange == true).ToArray();
            foreach (var z9Unloaded in z9UnloadedTiles)
            {
                // indexから子タイル情報を取得
                var z10Children = GetChildren(Childrens[z9Unloaded.Index].ToArray());
                var z10ChildrenUnloaded = z10Children.Where(t => t.State == LoadState.Unload).ToArray();
                foreach (var z10Unloaded in z10ChildrenUnloaded)
                { 
                    Span<DistanceWithIndex> z11Children = GetChildren(Childrens[z10Unloaded.Index].ToArray());
                    var z11ChildrenUnloaded = z11Children.ToArray().Where(t => t.State == LoadState.Unload).ToArray();
                    if (z11ChildrenUnloaded.Length == Childrens[z10Unloaded.Index].Length) // 子が全てUnloadの場合
                    {
                        // 上位タイルをロード状態にする
                        var item = Distances[z10Unloaded.Index];
                        item.State = LoadState.Load; // 上位タイルをロード状態にする
                        Distances[z10Unloaded.Index] = item; // 更新
                    }
                    else
                    {
                        // 子のうち一部がロード状態の場合は、子の全てをロード状態にする
                        foreach (var z11Unloaded in z11ChildrenUnloaded)
                        {
                            var item = Distances[z11Unloaded.Index];
                            item.State = LoadState.Load; // 子タイルをロード状態にする
                            Distances[z11Unloaded.Index] = item; // 更新
                        }
                　   }  
                }
            }
        }
    }

    /// <summary>
    /// タイルの距離でソートするJobSystemのJob
    /// </summary>
    public struct SortDistancesJob : IJob
    {
        public NativeArray<DistanceWithIndex> Distances;

        public void Execute()
        {
            Distances.Sort(); // NativeSortExtension を利用
        }
    }

    /// <summary>
    /// Jobsystemを使用したタイルロード処理
    /// 今のところカメラ距離計算、SortのみJob Systemを使用する。
    /// </summary>
    public class PLATEAUDynamicTileJobSystem : IDisposable
    {
        private NativeArray<TileBounds> NativeTileBounds;
        private NativeArray<DistanceWithIndex> NativeDistances;

        // 各Zoomレベルごとのカメラからのロード距離 Dictionary<int, (float, float)> loadDistances
        private NativeHashMap<int, FloatMinMax> NativeLoadDistances;

        // タイルの子タイル(下位ズームレベル)を保持する配列
        private NativeArray<ChildrenTiles> NativeChildrens;

        private List<PLATEAUDynamicTile> dynamicTiles; // タイルリスト
        private PLATEAUDynamicTileLoadTask loadTask;

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
        internal void Initialize(PLATEAUDynamicTileLoadTask loadTask, List<PLATEAUDynamicTile> tiles)
        {
            dynamicTiles = tiles;
            this.loadTask = loadTask;

            if (!NativeTileBounds.IsCreated || NativeTileBounds.Length != dynamicTiles.Count)
            {
                // 既存の配列を破棄
                Dispose();
            }

            if (!NativeTileBounds.IsCreated)
                NativeTileBounds = new NativeArray<TileBounds>(dynamicTiles.Count, Allocator.Persistent);
            if (!NativeDistances.IsCreated)
                NativeDistances = new NativeArray<DistanceWithIndex>(dynamicTiles.Count, Allocator.Persistent);
            if (!NativeChildrens.IsCreated)
                NativeChildrens = new NativeArray<ChildrenTiles>(dynamicTiles.Count, Allocator.Persistent);

            if (!NativeLoadDistances.IsCreated) 
            {
                // 各Zoomレベルごとのカメラからのロード距離を設定
                int numZoomLevels = loadTask.TileManager.loadDistances.Count;
                NativeLoadDistances = new NativeHashMap<int, FloatMinMax>(numZoomLevels, Allocator.Persistent);
                foreach (var loadDist in loadTask.TileManager.loadDistances)
                {
                    var (min, max) = loadDist.Value;
                    NativeLoadDistances.TryAdd(loadDist.Key, new FloatMinMax { min = min, max = max });
                }
            }
   
            for (int i = 0; i < dynamicTiles.Count; i++)
            {
                var tile = dynamicTiles[i];
                NativeTileBounds[i] = tile.GetTileBoundsStruct();

                // 子タイルのインデックスを取得
                if (tile.ChildrenTiles == null || tile.ChildrenTiles.Count == 0)
                    NativeChildrens[i] = new ChildrenTiles(new int[0]);
                else
                    NativeChildrens[i] = new ChildrenTiles(tile.ChildrenTiles.Where(t => t != null).Select(t => dynamicTiles.IndexOf(t)));
            }
        }

        // NativeArray PersistentなのでDispose必須　（メモリリーク防止のため）
        public void Dispose()
        {
            if (NativeTileBounds.IsCreated)
                NativeTileBounds.Dispose();
            if (NativeDistances.IsCreated)
                NativeDistances.Dispose();

            if (NativeLoadDistances.IsCreated)
                NativeLoadDistances.Dispose();
            if (NativeChildrens.IsCreated)
                NativeChildrens.Dispose();
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position, bool ignoreY)
        {
            // 距離計算
            TileDistanceCheckJob distJob = new TileDistanceCheckJob 
            { 
                TileStates = NativeTileBounds, 
                Distances = NativeDistances, 
                CameraPosition = position, 
                IgnoreY = ignoreY 
            };
            JobHandle distHandle = distJob.Schedule(NativeTileBounds.Length, 64);
            distHandle.Complete();

            // 範囲チェック
            TileRangeCheckJob rangeCheckJob = new TileRangeCheckJob
            {
                LoadDistances = NativeLoadDistances,
                Distances = NativeDistances
            };
            JobHandle rangeHandle = rangeCheckJob.Schedule(NativeTileBounds.Length, 64, distHandle);
            rangeHandle.Complete();

            // タイルの穴埋め処理
            FillTileHolesJob fillHolesJob = new FillTileHolesJob
            {
                Childrens = NativeChildrens,
                Distances = NativeDistances
            };
            JobHandle fillHolesHandle = fillHolesJob.Schedule(NativeTileBounds.Length, 64, JobHandle.CombineDependencies(distHandle, rangeHandle));
            fillHolesHandle.Complete();

            // 距離が近い順にソート
            JobHandle sortHandle = new SortDistancesJob { Distances = NativeDistances }.Schedule(JobHandle.CombineDependencies(distHandle, rangeHandle, fillHolesHandle));
            sortHandle.Complete();

            try
            {
                await ExecuteLoadTask(loadTask.LoadTaskCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                loadTask.DebugLog("タイルのロードTaskがキャンセルされました。");
            }
            finally
            {
                loadTask.PostLoadTask();
            }
        }

        /// <summary>
        /// タイルのロード状態に応じて、非同期でロードまたはアンロードを実行する。
        /// </summary>
        private async Task ExecuteLoadTask(CancellationToken token)
        {
            int loadFailCount = 0;

            for (int i = 0; i < NativeDistances.Length; i++)
            {
                var distanceWithIndex = NativeDistances[i];
                var distance = distanceWithIndex.Distance;
                var index = distanceWithIndex.Index;
                var tile = dynamicTiles[index];

                var nextLoadState = LoadState.None;
                //if (loadTask.TileManager.WithinTheRange(distance, tile))
                //{
                //    nextLoadState = LoadState.Load;
                //}
                //else
                //{
                //    nextLoadState = LoadState.Unload;
                //}
                nextLoadState = distanceWithIndex.State;

                tile.DistanceFromCamera = distance;
                tile.NextLoadState = nextLoadState;
                token.ThrowIfCancellationRequested();
            }

            // タイルの穴埋め処理
            //await loadTask.FillTileHoles(token);

            // タイルのロード状態に応じて、非同期でロードまたはアンロードを実行
            foreach (var tile in dynamicTiles)
            {
                if (tile.NextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (tile.NextLoadState == LoadState.Load && !tile.LoadHandle.IsValid())
                {
                    var result = await loadTask.PrepareLoadTile(tile);
                    if (result != PLATEAUTileManager.LoadResult.Success)
                        loadFailCount++;

                }
                else if (tile.NextLoadState == LoadState.Unload && tile.LoadHandle.IsValid())
                {
                    loadTask.PrepareUnloadTile(tile);
                }
                token.ThrowIfCancellationRequested();
            }

            loadTask.DebugLog($"タイルのロードTaskが完了しました。ロード失敗数: {loadFailCount}");
        }
    }

}
