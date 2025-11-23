using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

namespace PLATEAU.DynamicTile
{
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
        public float CalcDistance(Vector3 cameraPosition, bool ignoreY, bool lightweight = false)
        {
            if (ignoreY)
            {
                Vector3 position2d = new(cameraPosition.x, 0, cameraPosition.z);
                Vector3 closestPoint3d = lightweight ? 
                    ClosestPointOnBoundsLight(cameraPosition, BoundsMin, BoundsMax) : 
                    ClosestPointOnBounds(cameraPosition, BoundsMin, BoundsMax);
                Vector3 closestPoint2d = new(closestPoint3d.x, 0, closestPoint3d.z);
                return Vector3.Distance(position2d, closestPoint2d);
            }
            else
            {
                Vector3 closestPoint = lightweight ? 
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
    /// タイルの距離、元のインデックス、ロード状態を保持する構造体。
    /// <see cref="SortDistancesJob"/>でのソート用にIComparableを実装。
    /// </summary>
    public struct TileDistanceInfo : IComparable<TileDistanceInfo>
    {
        public readonly float Distance;
        public readonly int Index;
        public readonly int ZoomLevel;
        public LoadState State;
        public bool WithinMaxRange;

        public TileDistanceInfo(float distance, int index, int zoomLevel)
        {
            Distance = distance;
            Index = index;
            ZoomLevel = zoomLevel;
            State = LoadState.None; // 初期状態はNone
            WithinMaxRange = false; // 最大範囲内かどうかのフラグ
        }

        /// <summary>
        /// タイルの状態を更新するメソッド。
        /// </summary>
        public TileDistanceInfo WithUpdatedState(LoadState state)
        {
            this.State = state;
            return this;
        }

        /// <summary>
        /// <see cref="SortDistancesJob"/>でのSort用の比較メソッド。
        /// </summary>
        public int CompareTo(TileDistanceInfo other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    /// <summary>
    /// タイルの子タイル(下位ズームレベル)を保持する構造体。
    /// <see cref="PLATEAUDynamicTile.ChildrenTiles"/>の情報を配列indexで保持する。
    /// fixed配列だとunsafeにする必要があるため、構造体で保持する。
    /// </summary>
    public readonly struct ChildTileIndices
    {
        public readonly int tile1;
        public readonly int tile2;
        public readonly int tile3;
        public readonly int tile4;
        public readonly int Length;

        public ChildTileIndices(IEnumerable<int> tiles)
        {
            int t1 = 0, t2 = 0, t3 = 0, t4 = 0;
            int len = 0;
            if (tiles != null)
            {
                using (var e = tiles.GetEnumerator())
                {
                    while (len< 4 && e.MoveNext())
                    {
                        switch (len)
                        {
                            case 0: t1 = e.Current; break;
                            case 1: t2 = e.Current; break;
                            case 2: t3 = e.Current; break;
                            case 3: t4 = e.Current; break;
                        }
                        len++;
                    }
                }
            }
            tile1 = t1; tile2 = t2; tile3 = t3; tile4 = t4; 
            Length = len; // 最大4つの子タイルを保持する
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

        public NativeArray<int> ToNativeArray(Allocator allocator)
        {
            var nativeArray = new NativeArray<int>(Length, allocator);
            for (int i = 0; i < Length; i++)
            {
                nativeArray[i] = i switch
                {
                    0 => tile1,
                    1 => tile2,
                    2 => tile3,
                    3 => tile4,
                    _ => 0
                };
            }
            return nativeArray;
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
    /// 親タイルの情報を保持する構造体。
    /// <see cref="NativeHighResolutionTileConfig"/>で使用される。
    /// </summary>
    public struct ParentTileInfo
    {
        public int TileIndex;
        public ChildTileIndices Children;

        public ParentTileInfo(int tileIndex, ChildTileIndices children)
        {
            TileIndex = tileIndex;
            Children = children;
        }
    }

    /// <summary>
    /// 高解像度タイルの設定を保持する構造体
    /// </summary>
    public struct NativeHighResolutionTileConfig
    {
        public int TargetTileIndex;
        public NativeHashMap<int, ParentTileInfo> ParentsTileIndices;
        public bool IsEnabled; // nullチェックの代わりに使用

        public NativeHighResolutionTileConfig(int targetTileIndex, Allocator allocator)
        {
            TargetTileIndex = targetTileIndex;
            ParentsTileIndices = new NativeHashMap<int, ParentTileInfo>(10, allocator);
            IsEnabled = true;
        }
    }

    /// <summary>
    /// タイルの距離を計算するJobSystemのJob
    /// </summary>
    [BurstCompile]
    public struct TileDistanceCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<TileBounds> TileStates;
        [WriteOnly] public NativeArray<TileDistanceInfo> Distances;

        public Vector3 CameraPosition;
        public bool IgnoreY;

        void IJobParallelFor.Execute(int index)
        {
            var tile = TileStates[index];
            var zoomLevel = tile.ZoomLevel;
            Distances[index] = new TileDistanceInfo(tile.CalcDistance(CameraPosition, IgnoreY), index, zoomLevel);
        }
    }

    /// <summary>
    /// タイルの範囲をチェックするJobSystemのJob
    /// 範囲内であればLoadStateをLoadに設定し、範囲外であればUnloadに設定する。
    /// </summary>
    [BurstCompile]
    public struct TileRangeCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeHashMap<int, FloatMinMax> LoadDistances;
        public NativeArray<TileDistanceInfo> Distances;

        void IJobParallelFor.Execute(int index)
        {
            var distanceWithIndex = Distances[index];
            var zoomLevel = distanceWithIndex.ZoomLevel;

            if (LoadDistances.TryGetValue(zoomLevel, out var range) && range.WithinTheRange(distanceWithIndex.Distance))
            {
                // 距離が範囲内の場合はLoadStateをLoadに設定
                distanceWithIndex.State = LoadState.Load;
                distanceWithIndex.WithinMaxRange = true;
            }
            else
            {
                // 距離が範囲外の場合はLoadStateをUnloadに設定
                distanceWithIndex.State = LoadState.Unload;
                distanceWithIndex.WithinMaxRange = range.WithinMaxRange(distanceWithIndex.Distance);
            }
            Distances[index] = distanceWithIndex;
        }
    }

    /// <summary>
    /// タイルの穴埋め処理を行うJobSystemのJob
    /// <see cref="PLATEAUDynamicTileLoadTask.FillTileHoles"/>と同様の処理
    /// 注意：ソート処理でNativeArray<DistanceWithIndex>のindexが変更される前に実行する必要がある。
    /// Burst対応（LINQ/Arrayを使用せず、NativeArray/NativeListを使用）
    /// </summary>
    [BurstCompile]
    public struct FillTileHolesJob : IJob
    {
        [ReadOnly] public NativeArray<ChildTileIndices> Childrens;

        public NativeArray<TileDistanceInfo> Distances;

        /// <summary>
        /// ZoomLevelとLoadStateでフィルタリングされたタイルを取得するメソッド。
        /// </summary>
        /// <returns></returns>
        private NativeList<TileDistanceInfo> Filter(Allocator allocator, int zoomLevel, LoadState loadState, bool withinMaxRange)
        {
            NativeList<TileDistanceInfo> filtered = new NativeList<TileDistanceInfo>(allocator);
            for (int i = 0; i < Distances.Length; i++)
            {
                var dist = Distances[i];
                if (dist.State == loadState && dist.ZoomLevel == zoomLevel && dist.WithinMaxRange == withinMaxRange)
                    filtered.Add(dist);
            }
            return filtered;
        }

        /// <summary>
        /// LoadStateでフィルタリングされた子タイルの情報を取得するメソッド。
        /// </summary>
        private NativeList<TileDistanceInfo> GetChildrenByFiltering(NativeArray<int> indices, Allocator allocator, LoadState loadState)
        {
            NativeList<TileDistanceInfo> children = new NativeList<TileDistanceInfo>(allocator);
            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < 0 || indices[i] >= Distances.Length)
                {
                    // 不正なインデックスはスキップ
                    continue;
                }
                var dist = Distances[indices[i]];
                if (dist.State == loadState)
                    children.Add(dist);
            }
            return children;
        }

        public void Execute()
        {
            var allocator = Allocator.TempJob;

            // タイルの穴埋め処理
            var z9UnloadedTiles = Filter(allocator, 9, LoadState.Unload, true);
            foreach (var z9Unloaded in z9UnloadedTiles)
            {
                // indexから子タイル情報を取得
                var z10ChildrenUnloaded = GetChildrenByFiltering(Childrens[z9Unloaded.Index].ToNativeArray(Allocator.Temp), allocator, LoadState.Unload);
                foreach (var z10Unloaded in z10ChildrenUnloaded)
                {
                    var z11ChildrenUnloaded = GetChildrenByFiltering(Childrens[z10Unloaded.Index].ToNativeArray(Allocator.Temp), allocator, LoadState.Unload);
                    if (z11ChildrenUnloaded.Length == Childrens[z10Unloaded.Index].Length) // 子が全てUnloadの場合
                    {
                        Distances[z10Unloaded.Index] = Distances[z10Unloaded.Index].WithUpdatedState(LoadState.Load); // 上位タイルをロード状態に更新
                    }
                    else
                    {
                        // 子のうち一部がロード状態の場合は、子の全てをロード状態にする
                        foreach (var z11Unloaded in z11ChildrenUnloaded)
                        {
                            Distances[z11Unloaded.Index] = Distances[z11Unloaded.Index].WithUpdatedState(LoadState.Load); // 子タイルをロード状態に更新
                        }
                    }
                    z11ChildrenUnloaded.Dispose();
                }
                z10ChildrenUnloaded.Dispose();
            }
            z9UnloadedTiles.Dispose();
        }
    }

    /// <summary>
    /// タイルの距離でソートするJobSystemのJob
    /// </summary>
    [BurstCompile]
    public struct SortDistancesJob : IJob
    {
        public NativeArray<TileDistanceInfo> Distances;

        public void Execute()
        {
            Distances.Sort(); // NativeSortExtension を利用
        }
    }

    /// <summary>
    /// 常に高解像度で表示したいタイルが設定されている際に関連タイルのロード状態を更新するJobSystemのJob
    /// </summary>
    [BurstCompile]
    public struct HighResolutionTileLoadStateUpdateJob : IJob
    {
        [ReadOnly] public NativeHighResolutionTileConfig Config;
        public NativeArray<TileDistanceInfo> Distances;

        public void Execute()
        {
            // 設定が無効な場合は処理をスキップ
            if (!Config.IsEnabled)
                return;

            // 関連タイルのロード状態をチェック( ZoomLevel 9 または 10 が表示対象となっているか判定する)
            bool isLoadedParentTiles = false;
            foreach (var kvp in Config.ParentsTileIndices)
            {
                var parentInfo = kvp.Value;
                //親タイルがロードされている（高解像度対象のタイルを表示する必要がある）かチェック
                if (Distances[parentInfo.TileIndex].State == LoadState.Load)
                {
                    isLoadedParentTiles = true;
                    break;
                }
            }
            
            // ターゲットとなってるタイルそのものが表示対象になってる場合も考慮
            if (Distances[Config.TargetTileIndex].State == LoadState.Load)
            {
                isLoadedParentTiles = true;
            }
            

            if (!isLoadedParentTiles)
                return;

            // ZoomLevel 9 と 10 の親タイル情報を取得
            if (!Config.ParentsTileIndices.TryGetValue(9, out var zoomLevel9Info) ||
                !Config.ParentsTileIndices.TryGetValue(10, out var zoomLevel10Info))
            {
                // ZoomLevel 9 または 10 の情報が見つからない場合は処理をスキップ
                return;
            }
            // ターゲットとなるタイルを表示させるために、重なってしまうタイルたちを全てアンロードしていく
            // ZoomLevel 9 の親タイルをアンロード
            Distances[zoomLevel9Info.TileIndex] = Distances[zoomLevel9Info.TileIndex]
                .WithUpdatedState(LoadState.Unload);

            // ZoomLevel 9 の子タイル（ZoomLevel 10）を処理
            var z9Children = zoomLevel9Info.Children;
            for (int i = 0; i < z9Children.Length; i++)
            {
                int childIndex = i switch
                {
                    0 => z9Children.tile1,
                    1 => z9Children.tile2,
                    2 => z9Children.tile3,
                    3 => z9Children.tile4,
                    _ => -1
                };

                if (childIndex < 0 || childIndex >= Distances.Length)
                    continue;
                
                //一旦全てLoadに設定
                Distances[childIndex] = Distances[childIndex].WithUpdatedState(LoadState.Load);
            }
            
            // 高解像度化対象のタイルの親にあたるZoomLevel10のタイルをアンロードする
            Distances[zoomLevel10Info.TileIndex] = Distances[zoomLevel10Info.TileIndex]
                .WithUpdatedState(LoadState.Unload);

            // ZoomLevel 10 の子タイル（ZoomLevel 11）を全てロード
            // これにより対象タイルの周辺のみ（ZoomLevel10の範囲）は全てZoomLevel11の高解像度タイルとなる
            var z10Children = zoomLevel10Info.Children;
            for (int i = 0; i < z10Children.Length; i++)
            {
                int childIndex = i switch
                {
                    0 => z10Children.tile1,
                    1 => z10Children.tile2,
                    2 => z10Children.tile3,
                    3 => z10Children.tile4,
                    _ => -1
                };

                if (childIndex < 0 || childIndex >= Distances.Length)
                    continue;

                Distances[childIndex] = Distances[childIndex].WithUpdatedState(LoadState.Load);
            }
            
            Distances[Config.TargetTileIndex] = Distances[Config.TargetTileIndex].WithUpdatedState(LoadState.Load);
        }
    }

    /// <summary>
    /// Jobsystemを使用したタイルロード処理
    /// </summary>
    public class PLATEAUDynamicTileJobSystem : IDisposable
    {
        private NativeArray<TileBounds> NativeTileBounds;
        private NativeArray<TileDistanceInfo> NativeTileDistances;

        // タイルの子タイル(下位ズームレベル)を保持する配列
        private NativeArray<ChildTileIndices> NativeChildrens;

        // 各Zoomレベルごとのカメラからのロード距離 Dictionary<int, (float, float)> loadDistances
        private NativeHashMap<int, FloatMinMax> NativeLoadDistances;

        private List<PLATEAUDynamicTile> dynamicTiles; // タイルリスト
        private PLATEAUDynamicTileLoadTask loadTask;

        // Native対応の高解像度タイル設定（複数のConfigを保持）
        private List<NativeHighResolutionTileConfig> NativeHighResConfigs = new List<NativeHighResolutionTileConfig>();

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
        internal void Initialize(PLATEAUDynamicTileLoadTask loadTask, List<PLATEAUDynamicTile> tiles, IEnumerable<string> forceHighResTileAddresses = null)
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
            if (!NativeTileDistances.IsCreated)
                NativeTileDistances = new NativeArray<TileDistanceInfo>(dynamicTiles.Count, Allocator.Persistent);
            if (!NativeChildrens.IsCreated)
                NativeChildrens = new NativeArray<ChildTileIndices>(dynamicTiles.Count, Allocator.Persistent);

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
            
            // タイル -> インデックス マップを事前構築
            var indexMap = new Dictionary<PLATEAUDynamicTile, int>(dynamicTiles.Count);
            for (int i = 0; i < dynamicTiles.Count; i++) indexMap[dynamicTiles[i]] = i;          
            for (int i = 0; i < dynamicTiles.Count; i++)
            {
                var tile = dynamicTiles[i];
                NativeTileBounds[i] = tile.GetTileBoundsStruct();


                // 子タイルのインデックスを取得
                if (tile.ChildrenTiles == null || tile.ChildrenTiles.Count == 0)
                    NativeChildrens[i] = new ChildTileIndices(new int[0]);
                else
                    NativeChildrens[i] = new ChildTileIndices(tile.ChildrenTiles.Where(t => t != null).Select(t => indexMap[t]));
            }
            
            // 既存のNativeHighResConfigsがあればDisposeする
            foreach (var config in NativeHighResConfigs)
            {
                if (config.ParentsTileIndices.IsCreated)
                {
                    config.ParentsTileIndices.Dispose();
                }
            }
            NativeHighResConfigs.Clear();

            //高解像度タイルが設定されてる場合は設定用データを作成する
            if (forceHighResTileAddresses != null)
            {
                foreach (var address in forceHighResTileAddresses)
                {
                    var sb = new StringBuilder();
                    //対象タイルを取得
                    var targetTile = dynamicTiles.FirstOrDefault(m => m.Address.Contains(address));
                    sb.AppendLine($"高解像度タイル設定開始！: Address={address}, 対象タイル={(targetTile != null ? targetTile.Address : "Not Found")}");
                    if (targetTile == null)
                    {
                        continue;
                    }
                    var config = new NativeHighResolutionTileConfig(indexMap[targetTile], Allocator.Persistent);
                    //親タイルを辿ってZoomLevel10,9のタイルのIndexと子タイル情報を保持する
                    var tmpTile = targetTile;
                    while (tmpTile.ParentTile != null)
                    {
                        sb.AppendLine("親タイル追加: " + tmpTile.ParentTile.Address　+ "ZoomLevel=" + tmpTile.ParentTile.ZoomLevel);
                        tmpTile = tmpTile.ParentTile;
                        var parentInfo = new ParentTileInfo(indexMap[tmpTile], NativeChildrens[indexMap[tmpTile]]);
                        config.ParentsTileIndices.TryAdd(tmpTile.ZoomLevel, parentInfo);
                    }
                    NativeHighResConfigs.Add(config);
                    
                    Debug.Log(sb.ToString());

                }
            }
        }
        // NativeArray PersistentなのでDispose必須　（メモリリーク防止のため）
        public void Dispose()
        {
            if (NativeTileBounds.IsCreated)
                NativeTileBounds.Dispose();
            if (NativeTileDistances.IsCreated)
                NativeTileDistances.Dispose();
            if (NativeChildrens.IsCreated)
                NativeChildrens.Dispose();
            if (NativeLoadDistances.IsCreated)
                NativeLoadDistances.Dispose();

            // 全てのConfigをDispose
            foreach (var config in NativeHighResConfigs)
            {
                if (config.ParentsTileIndices.IsCreated)
                    config.ParentsTileIndices.Dispose();
            }
            NativeHighResConfigs.Clear();
        }

        /// <summary>
        /// 各タイルごとにカメラの距離に応じてロード状態を更新する。
        /// </summary>
        /// <param name="position"></param>
        public async Task UpdateAssetsByCameraPosition(Vector3 position, bool ignoreY)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew(); // 処理時間計測用

            // 距離計算
            TileDistanceCheckJob distJob = new TileDistanceCheckJob 
            { 
                TileStates = NativeTileBounds, 
                Distances = NativeTileDistances, 
                CameraPosition = position, 
                IgnoreY = ignoreY 
            };
            JobHandle distHandle = distJob.Schedule(NativeTileBounds.Length, 64);

            // 範囲チェック
            TileRangeCheckJob rangeCheckJob = new TileRangeCheckJob
            {
                LoadDistances = NativeLoadDistances,
                Distances = NativeTileDistances
            };
            JobHandle rangeHandle = rangeCheckJob.Schedule(NativeTileBounds.Length, 64, distHandle);

            // タイルの穴埋め処理
            FillTileHolesJob fillHolesJob = new FillTileHolesJob
            {
                Childrens = NativeChildrens,
                Distances = NativeTileDistances
            };
            JobHandle currentHandle = fillHolesJob.Schedule(rangeHandle);

            // 複数の高解像度タイルConfigに対してJobをスケジュール
            foreach (var config in NativeHighResConfigs)
            {
                HighResolutionTileLoadStateUpdateJob highResJob = new HighResolutionTileLoadStateUpdateJob
                {
                    Config = config,
                    Distances = NativeTileDistances
                };
                currentHandle = highResJob.Schedule(currentHandle);
            }

            // 距離が近い順にソート
            JobHandle sortHandle = new SortDistancesJob { Distances = NativeTileDistances }.Schedule(currentHandle);
            sortHandle.Complete();

            //loadTask.DebugLog($"JobSystem Elapsed Time: {sw.Elapsed.TotalMilliseconds:F4} ms");
            
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

            // タイルのロード状態に応じて、非同期でロードまたはアンロードを実行
            for (int i = 0; i < NativeTileDistances.Length; i++)
            {
                var distanceWithIndex = NativeTileDistances[i];
                var index = distanceWithIndex.Index;
                var tile = dynamicTiles[index];

                if (tile == null)
                    continue;

                tile.DistanceFromCamera = distanceWithIndex.Distance;
                tile.NextLoadState = distanceWithIndex.State;

                if (tile.NextLoadState == LoadState.None)
                {
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
