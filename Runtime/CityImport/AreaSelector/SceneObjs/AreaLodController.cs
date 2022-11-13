using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    public class AreaLodController
    {
        private readonly AreaLodSearcher searcher;
        private readonly ConcurrentDictionary<MeshCode, AreaLodView> viewDict = new ConcurrentDictionary<MeshCode, AreaLodView>();
        private Task loadTask = null;
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

        public async Task LoadAsync(MeshCode meshCode)
        {
            var packageLods = await Task.Run(() => this.searcher.LoadLodsInMeshCode(meshCode).ToArray());
            var position = this.geoReference.Project(meshCode.Extent.Center).ToUnityVector();
            this.viewDict.AddOrUpdate(meshCode,
                code => new AreaLodView(packageLods, position),
                (code, view) => new AreaLodView(packageLods, position));
        }

        public void DrawSceneGUI()
        {
            foreach (var view in this.viewDict.Values)
            {
                view?.DrawHandles();
            }
        }
    }
}
