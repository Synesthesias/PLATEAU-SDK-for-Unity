using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュに変換します。
    /// </summary>
    public class RoadNetworkToMesh
    {
        private readonly RnModel model;
        
        public RoadNetworkToMesh(RnModel model)
        {
            if (model == null)
            {
                Debug.LogError("道路ネットワークがありません。");
            }
            this.model = model;
        }

        public void Generate()
        {
            var contours = new RnmContourGenerator().Generate(model);
            foreach (var contour in contours)
            {
                var obj = new GameObject("RoadNetworkToMeshDebug");
                var comp = obj.AddComponent<PLATEAURoadNetworkToMeshDebug>();
                comp.Init(contour);
                var mesh = new ContourToMesh().Generate(contour);
                var renderer = obj.AddComponent<MeshRenderer>();
                var filter = obj.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                if (contour.SourceObject != null)
                {
                    var srcRenderer = contour.SourceObject.GetComponent<MeshRenderer>();
                    var srcMats = srcRenderer.sharedMaterials;
                    if (srcMats != null)
                    {
                        renderer.materials = srcMats;
                    }
                }
            }
            
        }
        
        
        
    }
}