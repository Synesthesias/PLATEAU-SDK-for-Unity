﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 範囲選択画面で、利用可能なLODをメッシュコードごとに検索して表示します。<br />
    /// 構成: <see cref="AreaLodController"/> -> (所有) -> [ <see cref="AreaLodSearcher"/> , (多)<see cref="AreaLodView"/> ]
    /// </summary>
    public class AreaLodController
    {
        private readonly AreaLodSearcher searcher;
        private readonly ConcurrentDictionary<MeshCode, AreaLodView> viewDict = new ConcurrentDictionary<MeshCode, AreaLodView>();
        private Task loadTask;
        private readonly GeoReference geoReference;

        public AreaLodController(DatasetSourceConfig datasetSourceConfig, GeoReference geoReference, IEnumerable<MeshCode> allMeshCodes)
        {
            this.searcher = new AreaLodSearcher(datasetSourceConfig);
            this.geoReference = geoReference;
            foreach (var meshCode in allMeshCodes)
            {
                this.viewDict.TryAdd(meshCode, null);
            }
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
            var meshCode = CalcNearestUnloadMeshCode(cameraExtent.Center, 3);
            if (meshCode == null) return;
            this.loadTask = Task.Run(() =>
            {
                Load(meshCode.Value);
            }).ContinueWithErrorCatch();
        }

        /// <summary>
        /// またLODを検索していないメッシュコードで、地域レベルが与えられたもののうち、
        /// <paramref name="geoCoordinate"/> に最も近い（補正あり）ものを返します。
        /// </summary>
        private MeshCode? CalcNearestUnloadMeshCode(GeoCoordinate geoCoordinate, int level)
        {
            double minSqrDist = double.MaxValue;
            MeshCode? nearestMeshCode = null;
            foreach (var meshCode in this.viewDict.Keys)
            {
                if (meshCode.Level != level) continue;
                // 読込済みのものは飛ばします
                if (this.viewDict.TryGetValue(meshCode, out var areaLodView) && areaLodView != null) continue;

                var distFromCenter = meshCode.Extent.Center - geoCoordinate;
                
                // 緯度・経度の値でそのまま距離を取ると、探索範囲が縦長になり、画面の上下にはみ出した箇所が探索されがちになります。
                // これを補正するため、縦よりも横を優先します。
                // 探索範囲が横長になり、横に長いディスプレイに映る範囲が優先的に探索されるようにします。
                distFromCenter.Longitude *= 0.5; 
                
                double sqrDist = distFromCenter.SqrMagnitudeLatLon;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearestMeshCode = meshCode;
                }
            }
            return nearestMeshCode;
        }
        
        /// <summary>
        /// 与えられたメッシュコードで利用可能なパッケージとLODを検索し、ビューに渡します。
        /// </summary>
        private void Load(MeshCode meshCode)
        {
            var packageLods = this.searcher.LoadLodsInMeshCode(meshCode.ToString());
            var extent = meshCode.Extent;
            var positionUpperLeft = this.geoReference.Project(new GeoCoordinate(extent.Max.Latitude, extent.Min.Longitude, 0)).ToUnityVector();
            var positionLowerRight = this.geoReference
                .Project(new GeoCoordinate(extent.Min.Latitude, extent.Max.Longitude, 0)).ToUnityVector();
            if (meshCode.Level >= 3)
            {
                this.viewDict.AddOrUpdate(meshCode,
                    code => new AreaLodView(packageLods, positionUpperLeft, positionLowerRight),
                    (code, view) => new AreaLodView(packageLods, positionUpperLeft, positionLowerRight));
            }
        }

        /// <summary>
        /// ビューに描画させます。
        /// </summary>
        public void DrawSceneGUI(Camera camera)
        {
            foreach (var view in this.viewDict.Values)
            {
                view?.DrawHandles(camera);
            }
        }
    }
}
