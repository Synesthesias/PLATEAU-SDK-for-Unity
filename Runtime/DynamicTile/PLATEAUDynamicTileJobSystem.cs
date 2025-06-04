using System;
using System.Collections.Generic;
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
    /// タイルの境界を表す構造体。
    /// </summary>
    public struct TileBounds
    {
        public Vector3 BoundsMin;
        public Vector3 BoundsMax;

        public TileBounds(Vector3 boundsMin, Vector3 boundsMax)//, LoadedState currentState)
        {
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
        }

        public float CalcDistance(Vector3 cameraPosition, bool ignoreY)
        {
            if (ignoreY)
            {
                var position2d = new Vector3(cameraPosition.x, 0, cameraPosition.z);
                Vector3 closestPoint3d = ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                Vector3 closestPoint2d = new Vector3(closestPoint3d.x, 0, closestPoint3d.z);
                return Vector3.Distance(position2d, closestPoint2d);
            }
            else
            {
                Vector3 closestPoint = ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                return Vector3.Distance(cameraPosition, closestPoint);
            }
        }

        Vector3 ClosestPointOnBounds(Vector3 position, Vector3 boundsMin, Vector3 boundsMax)
        {
            var bounds = new Bounds();
            bounds.SetMinMax(boundsMin, boundsMax);
            return bounds.ClosestPoint(position);
        }

        Vector3 ClosestPointOnBoundsSimple(Vector3 position, Vector3 boundsMin, Vector3 boundsMax)
        {
            float x = Mathf.Clamp(position.x, boundsMin.x, boundsMax.x);
            float y = Mathf.Clamp(position.y, boundsMin.y, boundsMax.y);
            float z = Mathf.Clamp(position.z, boundsMin.z, boundsMax.z);
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// タイルの距離を計算するJobSystemのJob
    /// </summary>
    public struct TileDistanceCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<TileBounds> TileStates;
        [WriteOnly] public NativeArray<float> Distances;

        public Vector3 CameraPosition;
        public bool IgnoreY;

        void IJobParallelFor.Execute(int index)
        {
            var tile = TileStates[index];
            Distances[index] = tile.CalcDistance(CameraPosition, IgnoreY);
        }
    }

    /// <summary>
    /// Jobsystemを使用したタイルロード処理
    /// 今のところカメラ距離計算のみJob Systemを使用する。
    /// </summary>
    public class PLATEAUDynamicTileJobSystem : IDisposable
    {
        public NativeArray<TileBounds> NativeTileBounds;
        public NativeArray<float> NativeDistances;

        private List<PLATEAUDynamicTile> dynamicTiles; // タイルリスト
        private PLATEAUTileManager tileManager;

        public void Initialize(PLATEAUTileManager manager, List<PLATEAUDynamicTile> tiles)
        {
            dynamicTiles = tiles;
            tileManager = manager;

            if (!NativeTileBounds.IsCreated)
                NativeTileBounds = new NativeArray<TileBounds>(dynamicTiles.Count, Allocator.Persistent);
            if (!NativeDistances.IsCreated)
                NativeDistances = new NativeArray<float>(dynamicTiles.Count, Allocator.Persistent);

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
        
        public void UpdateAssetByCameraPosition(Vector3 position)
        {
            TileDistanceCheckJob job = new TileDistanceCheckJob { TileStates = NativeTileBounds, Distances = NativeDistances, CameraPosition = position, IgnoreY = true };
            JobHandle handle = job.Schedule(NativeTileBounds.Length, 64);
            handle.Complete();

            ExecuteLoadTask();
        }

        // カメラ距離に応じてタイルのロード状態を更新する
        private async void ExecuteLoadTask()
        {
            for (int i = 0; i < NativeDistances.Length; i++)
            {
                var distance = NativeDistances[i];
                var tile = dynamicTiles[i];

                var nextLoadState = LoadState.None;
                if (distance < PLATEAUTileManager.DefaultLoadDistance)
                {
                    nextLoadState = LoadState.Load;
                }
                else
                {
                    nextLoadState = LoadState.Unload;
                }

                // debug用
                tile.DistanceFromCamera = distance;
                tile.NextLoadState = nextLoadState;

                if (nextLoadState == LoadState.None)
                {
                    // 何もしない
                    continue;
                }
                else if (nextLoadState == LoadState.Load && !tile.IsLoadedOrLoading)
                {
                    await tileManager.Load(tile);
                }
                else if (nextLoadState == LoadState.Unload && tile.IsLoadedOrLoading)
                {
                    tileManager.Unload(tile);
                }
            }
        }

    }

}
