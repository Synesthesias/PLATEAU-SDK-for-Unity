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

        /// <summary> 路面標示をメッシュとして生成します。 </summary>
        public void Generate()
        {
            if (targetNetwork == null)
            {
                Debug.LogError("道路ネットワークが見つかりませんでした。");
                return;
            }
            
            // 道路の線を取得します。
            var ways = new MarkedWayListComposer().ComposeFrom(targetNetwork);
            
            var combines = new List<CombineInstance>(ways.Get.Count);
            foreach (var way in ways.Get)
            {
                // 道路の線をメッシュに変換します。
                var gen = way.Type.ToLineMeshGenerator(way.Direction);
                var mesh = gen.GenerateMesh(way.Way.Points.Select(p => p.Vertex).ToArray());
                
                combines.Add(new CombineInstance{mesh = mesh, transform = Matrix4x4.identity});
            }

            var dstMesh = new Mesh();
            dstMesh.CombineMeshes(combines.ToArray());
            GenerateGameObj(dstMesh, null);
        }

        

        private GameObject GenerateGameObj(Mesh mesh, Transform dstParent)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
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