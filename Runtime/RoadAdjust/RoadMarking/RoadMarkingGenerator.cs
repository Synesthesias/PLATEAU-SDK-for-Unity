using PLATEAU.RoadNetwork.Structure;
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
        private readonly RnModel targetNetwork;
        private const string MeshName = "RoadMarkingMesh";
        
        
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
            ways.AddRange(new StopLineComposer().ComposeFrom(targetNetwork));
            
            var instances = new RoadMarkingCombiner(ways.MarkedWays.Count);
            foreach (var way in ways.MarkedWays)
            {
                // 道路の線をメッシュに変換します。
                var gen = way.Type.ToLineMeshGenerator(way.IsReversed);
                new MWLineSmoother().Smooth(way.Line);
                var points = way.Line.Points;
                var instance = gen.GenerateMesh(points.ToArray());
                
                instances.Add(instance);
            }

            var dstMesh = instances.Combine();
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
            meshRenderer.sharedMaterials = RoadMarkingMaterialExtension.Materials();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // 道路と重なっているので影は不要
            obj.transform.parent = dstParent;
            mesh.name = "RoadMarkingMesh";
            return obj;
        }
    }
}