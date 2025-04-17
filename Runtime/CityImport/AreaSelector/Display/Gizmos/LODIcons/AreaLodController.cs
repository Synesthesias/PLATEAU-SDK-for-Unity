using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos.LODIcons
{
    /// <summary>
    /// 範囲選択画面で、利用可能なLODをメッシュコードごとに検索して表示します。<br />
    /// 構成: <see cref="AreaLodController"/> -> (所有) -> [ <see cref="AreaLodSearcher"/> , (多)<see cref="AreaLodView"/> ]
    /// </summary>
    public class AreaLodController
    {
        private readonly AreaLodSearcher searcher;
        private readonly ConcurrentDictionary<GridCode, AreaLodView> viewDict = new ConcurrentDictionary<GridCode, AreaLodView>();
        private Task loadTask;
        private readonly GeoReference geoReference;
        private readonly HashSet<int> showLods;

        public AreaLodController(IDatasetSourceConfig datasetSourceConfig, GeoReference geoReference, NativeVectorGridCode allGridCodes)
        {
            this.searcher = new AreaLodSearcher(datasetSourceConfig);
            this.geoReference = geoReference;
            for (int i = 0; i < allGridCodes.Length; i++)
            {
                var gridCode = allGridCodes.At(i);
                this.viewDict.TryAdd(gridCode, null);
            }
            showLods = new HashSet<int> { 1, 2, 3, 4 };
            AreaLodView.Init();
        }

        /// <summary>
        /// LOD未検索のメッシュコードのうち、カメラにもっとも近いメッシュコードのLODを検索して、
        /// 範囲選択画面の地図に表示します。
        /// ただし、すでに検索処理が動いている場合は何もしません。
        /// </summary>
        public void Update(Extent cameraExtent)
        {
            if (this.loadTask is { IsCompleted: false }) return;
            var gridCode = CalcNearestUnloadGridCode(cameraExtent.Center);
            if (gridCode == null) return;
            this.loadTask = Task.Run(() =>
            {
                Load(gridCode);
            }).ContinueWithErrorCatch();
        }

        /// <summary>
        /// またLODを検索していないメッシュコードで、地域レベルが引数で与えられたもの以上のもののうち、
        /// <paramref name="geoCoordinate"/> に最も近い（補正あり）ものを返します。
        /// </summary>
        private GridCode CalcNearestUnloadGridCode(GeoCoordinate geoCoordinate)
        {
            double minSqrDist = double.MaxValue;
            GridCode nearestGridCode = null;
            foreach (var gridCode in this.viewDict.Keys)
            {
                // 広域は飛ばします
                if (!gridCode.IsNormalGmlLevel && !gridCode.IsSmallerThanNormalGml) continue;
                // 読込済みのものは飛ばします
                if (this.viewDict.TryGetValue(gridCode, out var areaLodView) && areaLodView != null) continue;

                var distFromCenter = gridCode.Extent.Center - geoCoordinate;
                
                // 緯度・経度の値でそのまま距離を取ると、探索範囲が縦長になり、画面の上下にはみ出した箇所が探索されがちになります。
                // これを補正するため、縦よりも横を優先します。
                // 探索範囲が横長になり、横に長いディスプレイに映る範囲が優先的に探索されるようにします。
                distFromCenter.Longitude *= 0.5; 
                
                double sqrDist = distFromCenter.SqrMagnitudeLatLon;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearestGridCode = gridCode;
                }
            }
            return nearestGridCode;
        }
        
        /// <summary>
        /// 与えられたメッシュコードで利用可能なパッケージとLODを検索し、ビューに渡します。
        /// </summary>
        private void Load(GridCode gridCode)
        {
            var packageLods = this.searcher.LoadLodsInGridCode(gridCode.StringCode);
            var extent = gridCode.Extent;
            var positionUpperLeft = this.geoReference.Project(new GeoCoordinate(extent.Max.Latitude, extent.Min.Longitude, 0)).ToUnityVector();
            var positionLowerRight = this.geoReference
                .Project(new GeoCoordinate(extent.Min.Latitude, extent.Max.Longitude, 0)).ToUnityVector();
            if (gridCode.IsNormalGmlLevel || gridCode.IsSmallerThanNormalGml)
            {
                this.viewDict.AddOrUpdate(gridCode,
                    _ => new AreaLodView(packageLods, positionUpperLeft, positionLowerRight),
                    (_, _) => new AreaLodView(packageLods, positionUpperLeft, positionLowerRight));
            }
        }

        /// <summary>
        /// ビューに描画させます。
        /// </summary>
        public void DrawSceneGUI(Camera camera)
        {
            foreach (var view in this.viewDict.Values)
            {
                view?.DrawHandles(camera, showLods);
            }
        }

        /// <summary>
        /// 表示するLODアイコンを切り替える
        /// </summary>
        /// <param name="lod"></param>
        /// <param name="isCheck"></param>
        public void SwitchLodIcon(int lod, bool isCheck)
        {
            if (isCheck)
            {
                showLods.Add(lod);
            }
            else
            {
                showLods.Remove(lod);
            }
#if UNITY_EDITOR
            foreach (var view in this.viewDict)
            {
                #if UNITY_EDITOR
                view.Value?.CalculateLodViewParam(SceneView.lastActiveSceneView.camera, showLods);
                #endif
            }
#endif
        }
    }
}
