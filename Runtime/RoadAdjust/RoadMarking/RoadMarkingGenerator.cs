using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークをもとに、車線の線となるメッシュを生成します。
    /// </summary>
    public class RoadMarkingGenerator
    {
        private RnModel targetNetwork;
        
        

        private static string materialFolder = "PlateauRoadMarkingMaterials";
        private static string materialNameWhite = "PlateauRoadMarkingWhite";
        private static string materialNameYellow = "PlateauRoadMarkingYellow";
        private static Material materialWhite = Resources.Load<Material>(materialFolder + "/" + materialNameWhite);
        private static Material materialYellow = Resources.Load<Material>(materialFolder + "/" + materialNameYellow);
        
        public RoadMarkingGenerator(RnModel targetNetwork)
        {
            this.targetNetwork = targetNetwork;
        }

        public void Generate()
        {
            var ways = MarkedWayList.ComposeFrom(targetNetwork);
            var parent = new GameObject("RoadMarking").transform;
            foreach (var way in ways.Get)
            {
                GenerateWayMarking(way, parent);
            }
        }
        

        private GameObject GenerateWayMarking(MarkedWay way, Transform dstParent)
        {
            var gen = way.Type.ToLineMeshGenerator(way.Direction);
            if (gen == null) return null;
            var mesh = gen.GenerateMesh(way.Way.Points.Select(p => p.Vertex).ToArray());
            if (mesh == null) return null;
            return GenerateGameObj(mesh, dstParent);
        }

        

        private GameObject GenerateGameObj(Mesh mesh, Transform dstParent)
        {
            var obj = new GameObject("RoadMarking");
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(materialWhite);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // 道路と重なっているので影は不要
            obj.transform.parent = dstParent;
            return obj;
        }
    }
}