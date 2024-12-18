using PLATEAU.RoadAdjust.RoadMarking.DirectionalArrow;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
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
        private readonly IRrTarget targetBeforeCopy;
        
        
        public RoadMarkingGenerator(IRrTarget target)
        {
            
            if (target != null)
            {
                this.targetBeforeCopy = target;
            }
            else
            {
                Debug.LogError("target road network is null.");
            }
            
        }

        /// <summary> 路面標示をメッシュとして生成します。 </summary>
        public void Generate()
        {
            if (targetBeforeCopy == null)
            {
                Debug.LogError("道路ネットワークが見つかりませんでした。");
                return;
            }
            

            using var progressDisplay = new ProgressDisplayDialogue();
            progressDisplay.SetProgress("道路標示生成", 5f, "道路ネットワークをコピー中");
            
            var target = targetBeforeCopy.Copy(); // 道路ネットワークを処理中だけ調整したいのでディープコピーを対象にします。
            
            progressDisplay.SetProgress("道路標示生成", 10f, "道路ネットワークをスムージング中");
            new RoadNetworkLineSmoother().Smooth(target);

            var dstParent = RoadReproducer.GenerateDstParent();

            var roadBases = target.RoadBases().ToArray();
            for (int i = 0; i < roadBases.Length; i++) // 道路オブジェクトごとに結合します。
            {
                var rb = roadBases[i];
                string roadName = rb.TargetTrans == null || rb.TargetTrans.Count == 0
                    ? "UnknownRoad"
                    : rb.TargetTrans.First().name;
                progressDisplay.SetProgress("道路標示生成", (float)i / roadBases.Length,
                    $"[{i + 1} / {roadBases.Length}] {roadName}");
                var innerTarget = new RrTargetRoadBases(target.Network(), new[] { rb });

                // 道路の線を取得します。
                var ways = new MarkedWayListComposer().ComposeFrom(innerTarget);


                ways.AddRange(new StopLineComposer().ComposeFrom(innerTarget));


                var instances = new RoadMarkingCombiner();
                foreach (var way in ways.MarkedWays)
                {
                    // 道路の線をメッシュに変換します。
                    var gen = way.Type.ToLineMeshGenerator(way.IsReversed);
                    var points = way.Line.Points;
                    var instance = gen.GenerateMeshInstance(points.ToArray());

                    instances.Add(instance);
                }

                // 交差点前の矢印を生成します。
                var arrowComposer = new DirectionalArrowComposer(innerTarget);
                instances.AddRange(arrowComposer.Generate());

                var dstMesh = instances.Combine(out var materials);
                GenerateGameObj(dstMesh, dstParent, rb, materials);
            }
        }

        

        private void GenerateGameObj(Mesh mesh, Transform dstParent, RnRoadBase srcRoad, Material[] materials)
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
            meshRenderer.sharedMaterials = materials;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // 道路と重なっているので影は不要
            dstObj.transform.parent = dstParent;
            mesh.name = dstName;
            var comp = dstObj.GetOrAddComponent<PLATEAUReproducedRoad>();
            comp.Init(ReproducedRoadType.RoadMarking, targetRoad == null ? null : targetRoad.transform);
        }
    }
}