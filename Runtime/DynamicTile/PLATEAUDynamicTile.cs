using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PLATEAU.DynamicTile
{
    public enum LoadState
    {
        None,
        Load,
        Unload
    }

    /// <summary>
    /// DynamicTileの情報を保持するクラス。
    /// </summary>
    public class PLATEAUDynamicTile
    {
        /// <summary>
        /// Addressablesのアドレスを保持する。
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// タイルが除外されているかどうかを示すフラグ。
        /// </summary>
        public bool IsExcludeTile { get; set; }

        /// <summary>
        /// AddressablesでロードされたGameObjectを保持する。
        /// </summary>
        public GameObject LoadedObject { get; internal set; } = null;

        /// <summary>
        /// Addressablesのロードハンドルを保持する。
        /// </summary>
        public AsyncOperationHandle<GameObject> LoadHandle { get; internal set; } = default;

        /// <summary>
        /// Addressablesのロードハンドルのキャンセルトークンを保持する。
        /// Tileごとのキャンセルをサポートするためのもの。
        /// 現状、AddressablesはCancelをサポートしていないので、普通のフラグでも良いが、将来のために保持しておく。
        /// </summary>
        public CancellationTokenSource LoadHandleCancellationTokenSource { get; internal set; }

        public int ZoomLevel { get; private set; } = 0;
        
        public string GroupName { get; private set; }

        /// <summary>
        /// タイルの範囲を示すBoundsを保持する。
        /// </summary>
        public Bounds Extent { get; set; }

        /// <summary>
        /// 上位ZoomLevelのタイルを保持する。
        /// (現状未使用）
        /// </summary>
        public PLATEAUDynamicTile ParentTile { get; set; }

        /// <summary>
        /// 下位ZoomLevelのタイルを保持する。
        /// </summary>
        public List<PLATEAUDynamicTile> ChildrenTiles { get; set; }

        /// <summary>
        /// カメラからの距離を保持する。デバッグ用。
        /// </summary>
        public float DistanceFromCamera { get; set; } = 0f;

        /// <summary>
        /// 次にAddressablesをロードする状態を示す。
        /// </summary>
        public LoadState NextLoadState { get; set; } = LoadState.None;

        public PLATEAUTileManager.LoadResult LastLoadResult { get; set; } = PLATEAUTileManager.LoadResult.None;

        // Job Systemで使用するための構造体を返す
        public TileBounds GetTileBoundsStruct()
        {
            if (Extent.size == Vector3.zero)
                return default;
            return new TileBounds(Extent.min, Extent.max, ZoomLevel);
        }

        /// <summary>
        /// Addressからパッケージを取得する。
        /// </summary>
        private PredefinedCityModelPackage? package = null;
        public PredefinedCityModelPackage Package
        {
            get
            {
                if (package == null)
                    package = DynamicTileTool.GetPackage(Address);
                return package ?? PredefinedCityModelPackage.None;
            }
        }

        /// <summary>
        /// PLATEAUDynamicTileのコンストラクタ。
        /// </summary>
        /// <param name="info">Scriptable Object データ</param>
        public PLATEAUDynamicTile(PLATEAUDynamicTileMetaInfo info)
        {
            Address = info.AddressName;
            Extent = info.Extent;
            ZoomLevel = info.ZoomLevel;
            ChildrenTiles = new List<PLATEAUDynamicTile>();
            GroupName = info.GroupName;
        }

        /// <summary>
        /// タイルのロード状態をリセットする。
        /// LoadHandle.Resultが存在する場合は、Reset前に必ずAddressablesのReleaseを行うこと。
        /// </summary>
        public void Reset()
        {
            LoadHandle = default; // ハンドルをリセット
            try
            {
                LoadHandleCancellationTokenSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {}
            LoadHandleCancellationTokenSource?.Dispose();
            LoadHandleCancellationTokenSource = null;
        }

        /// <summary>
        /// カメラからの距離を計算する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="ignoreY"></param>
        /// <returns></returns>
        public float GetDistance(Vector3 position, bool ignoreY)
        {
            if (Extent.size == Vector3.zero)
            {
                return float.MaxValue;
            }

            float distance = 0f;
            if (ignoreY)
            {
                var extent2d = new Bounds(new Vector3(Extent.center.x, 0, Extent.center.z), new Vector3(Extent.size.x, 0, Extent.size.z));
                var position2d = new Vector3(position.x, 0, position.z);
                Vector3 closestPoint2d = extent2d.ClosestPoint(position2d);
                distance = Vector2.Distance(new Vector2(position2d.x, position2d.z),new Vector2(closestPoint2d.x, closestPoint2d.z));
            }
            else
            {
                Vector3 closestPoint = Extent.ClosestPoint(position);
                distance = Vector3.Distance(position, closestPoint);
            }

            DistanceFromCamera = distance;
            return distance;
        }
    }
} 