using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;

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
            // カメラ中心にもっとも近いメッシュコードについて、利用可能なLODを検索します。
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

        public async Task LoadAsync(MeshCode meshCode)
        {
            var packageLods = await Task.Run(() => this.searcher.LoadLodsInMeshCode(meshCode.ToString()));
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
