using PLATEAU.Util;
using System;
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
    public readonly struct DistanceWithIndex : IComparable<DistanceWithIndex>
    {
        public readonly float Distance;
        public readonly int Index;
        public DistanceWithIndex(float distance, int index)
        {
            Distance = distance;
            Index = index;
        }
        public int CompareTo(DistanceWithIndex other)
        {
            return Distance.CompareTo(other.Distance);
        }

    }

    /// <summary>
    /// タイルの距離を計算するJobSystemのJob
    /// </summary>
    public struct TileDistanceCheckJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public NativeArray<TileBounds> TileStates;
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

        private DynamicTileCollection dynamicTiles; // タイルリスト
        private PLATEAUDynamicTileLoadTask loadTask;
        private TileLoadDistanceCollection tileLoadDistances;
        private ConditionalLogger logger;

        public int TileCount => dynamicTiles?.Count ?? 0; // タイルの数を取得するプロパティ

        /// <summary>
        /// NativeArrayを初期化し、タイルマネージャーとタイルリストを設定する。
        /// </summary>
        internal void Initialize(PLATEAUDynamicTileLoadTask loadTask, TileLoadDistanceCollection tileLoadDistancesArg, DynamicTileCollection tiles, ConditionalLogger loggerArg)
        {
            dynamicTiles = tiles;
            tileLoadDistances = tileLoadDistancesArg;
            this.loadTask = loadTask;
            logger = loggerArg;

            // 既存の配列を破棄（リーク防止）
            DisposeNativeArrays();

            NativeTileBounds = new NativeArray<TileBounds>(dynamicTiles.Count, Allocator.Persistent);
            NativeDistances = new NativeArray<DistanceWithIndex>(dynamicTiles.Count, Allocator.Persistent);
            dynamicTiles.ComposeTileBounds(NativeTileBounds);
        }

        /// <summary>
        /// NativeArrayを破棄します。
        /// </summary>
        private void DisposeNativeArrays()
        {
            if (NativeTileBounds.IsCreated)
            {
                NativeTileBounds.Dispose();
                NativeTileBounds = default;
            }
            if (NativeDistances.IsCreated)
            {
                NativeDistances.Dispose();
                NativeDistances = default;
            }
        }

        public void Dispose()
        {
            DisposeNativeArrays();
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position)
        {
            // NativeArrayが初期化されていない場合は早期リターン
            if (!NativeTileBounds.IsCreated || !NativeDistances.IsCreated)
            {
                logger?.LogWarn("NativeArrayが初期化されていません。");
                return;
            }

            try
            {
                // 距離計算
                TileDistanceCheckJob job = new TileDistanceCheckJob 
                { 
                    TileStates = NativeTileBounds, 
                    Distances = NativeDistances, 
                    CameraPosition = position, 
                    IgnoreY = false 
                };
                var distHandle = job.Schedule(NativeTileBounds.Length, 64);
                distHandle.Complete();

                // 距離が近い順にソート
                var sortHandle = new SortDistancesJob { Distances = NativeDistances }.Schedule(distHandle);
                sortHandle.Complete();

                await ExecuteLoadTask(loadTask.LoadTaskCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                logger?.LogWarn("タイルのロードTaskがキャンセルされました。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"タイルの更新中にエラーが発生しました: {ex.Message}");
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
                var tile = dynamicTiles.At(index);

                var nextLoadState = LoadState.None;
                if (tileLoadDistances.IsWithinRange(distance, tile.ZoomLevel))
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
                    var result = await loadTask.PrepareLoadTile(tile);
                    if (result != PLATEAUTileManager.LoadResult.Success)
                        loadFailCount++;
                    
                }
                else if (nextLoadState == LoadState.Unload && tile.LoadHandle.IsValid())
                {
                    loadTask.PrepareUnloadTile(tile);
                }
                token.ThrowIfCancellationRequested();
            }

            logger.Log($"タイルのロードTaskが完了しました。ロード失敗数: {loadFailCount}");
        }
    }

}
