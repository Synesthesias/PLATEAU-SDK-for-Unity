using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
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
        private readonly IRrTarget target;
        
        
        public RoadMarkingGenerator(IRrTarget target)
        {
            // 道路ネットワークを処理中だけ調整したいのでディープコピーを対象にします。
            if (target != null)
            {
                this.target = target.Copy();
            }
            else
            {
                Debug.LogError("target road network is null.");
            }
            
        }

        /// <summary> 路面標示をメッシュとして生成します。 </summary>
        public void Generate()
        {
            if (target == null)
            {
                Debug.LogError("道路ネットワークが見つかりませんでした。");
                return;
            }
            new RoadNetworkLineSmoother().Smooth(target);

            var dstParent = RoadReproducer.GenerateDstParent();

            foreach (var rb in target.RoadBases()) // 道路オブジェクトごとに結合します。
            {
                
                var innerTarget = new RrTargetRoadBases(target.Network(), new[] { rb });
                
                // 道路の線を取得します。
                var ways = new MarkedWayListComposer().ComposeFrom(innerTarget);
            
            
                ways.AddRange(new StopLineComposer().ComposeFrom(innerTarget));
            
                var instances = new RoadMarkingCombiner(ways.MarkedWays.Count);
                foreach (var way in ways.MarkedWays)
                {
                    // 道路の線をメッシュに変換します。
                    var gen = way.Type.ToLineMeshGenerator(way.IsReversed);
                    var points = way.Line.Points;
                    var instance = gen.GenerateMesh(points.ToArray());
                
                    instances.Add(instance);
                }
            
                var dstMesh = instances.Combine();
                GenerateGameObj(dstMesh, dstParent, rb);
            }
        }

        

        private void GenerateGameObj(Mesh mesh, Transform dstParent, RnRoadBase srcRoad)
        {
            var targetRoads = srcRoad.TargetTrans;
            var targetRoad = targetRoads == null || targetRoads.Count == 0 ? null : targetRoads.First();
            var targetName = targetRoad == null ? "UnknownRoad" : targetRoad.name;
            string dstName = $"RoadMarking-{targetName}";
            GameObject dstObj = null;
            if (targetRoad != null)
            {
                dstObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.RoadMarking, targetRoad.transform);
            }

            if (dstObj == null)
            {
                dstObj = new GameObject(dstName);
            }
            
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            
            
            var meshFilter = dstObj.GetOrAddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            var meshRenderer = dstObj.GetOrAddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = RoadMarkingMaterialExtension.Materials();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // 道路と重なっているので影は不要
            dstObj.transform.parent = dstParent;
            mesh.name = dstName;
            var comp = dstObj.GetOrAddComponent<PLATEAUReproducedRoad>();
            comp.Init(ReproducedRoadType.RoadMarking, targetRoad == null ? null : targetRoad.transform);
        }
    }
}