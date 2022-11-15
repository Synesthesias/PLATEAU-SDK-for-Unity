using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
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

        public AreaLodController(string rootPath, GeoReference geoReference, IEnumerable<MeshCode> allMeshCodes)
        {
            this.searcher = new AreaLodSearcher(rootPath);
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
            var meshCode = CalcNearestUnloadMeshCode(cameraExtent.Center);
            if (meshCode == null) return;
            this.loadTask = Task.Run(async() =>
            {
                await LoadAsync(meshCode.Value);
            }).ContinueWithErrorCatch();
        }

        /// <summary>
        /// またLODを検索していないメッシュコードのうち、 <paramref name="geoCoordinate"/> に最も近いものを返します。
        /// </summary>
        private MeshCode? CalcNearestUnloadMeshCode(GeoCoordinate geoCoordinate)
        {
            double minSqrDist = float.MaxValue;
            MeshCode? nearestMeshCode = null;
            foreach (var meshCode in this.viewDict.Keys)
            {
                // 読込済みのものは飛ばします
                if (this.viewDict.TryGetValue(meshCode, out var areaLodView) && areaLodView != null) continue;
                
                double sqrDist = (meshCode.Extent.Center - geoCoordinate).SqrMagnitudeLatLon;
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
        private async Task LoadAsync(MeshCode meshCode)
        {
            var packageLods = await Task.Run(() => this.searcher.LoadLodsInMeshCode(meshCode.ToString()));
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
