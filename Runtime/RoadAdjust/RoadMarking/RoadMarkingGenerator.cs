using PLATEAU.RoadAdjust.RoadMarking.Crosswalk;
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
            
            // 路面標示のうち、道路ごとに結合するものをこの道路forループの中に書きます。
            for (int i = 0; i < roadBases.Length; i++)
            {
                var rb = roadBases[i];
                string roadName = rb.TargetTrans == null || rb.TargetTrans.Count == 0
                    ? "UnknownRoad"
                    : rb.TargetTrans.First().name;
                progressDisplay.SetProgress("道路標示生成", (float)i / roadBases.Length,
                    $"[{i + 1} / {roadBases.Length}] {roadName}");
                var innerTarget = new RrTargetRoadBases(target.Network(), new[] { rb });

                var combiner = new RoadMarkingCombiner();
                
                // 白線を生成します。
                combiner.AddRange(GenerateRoadLines(innerTarget)); 
                

                // 交差点前の矢印を生成します。
                var arrowComposer = new DirectionalArrowComposer(innerTarget);
                combiner.AddRange(arrowComposer.Compose());

                var dstMesh = combiner.Combine(out var materials);
                GenerateGameObj(dstMesh, materials, dstParent, rb, ReproducedRoadType.LaneLineAndArrow, ReproducedRoadDirection.None);
            }
            
            // 交差点を生成します。
            var crosswalkInstances = new CrosswalkComposer().Compose(target);
            foreach (var crosswalk in crosswalkInstances)
            {
                var crosswalkCombiner = new RoadMarkingCombiner();
                crosswalkCombiner.Add(crosswalk.RoadMarkingInstance);
                var crosswalkMesh = crosswalkCombiner.Combine(out var crosswalkMats);
                GenerateGameObj(crosswalkMesh, crosswalkMats, dstParent, crosswalk.SrcRoad, ReproducedRoadType.Crosswalk, crosswalk.Direction);
            }
            
        }

        private IEnumerable<RoadMarkingInstance> GenerateRoadLines(IRrTarget innerTarget)
        {
            // 道路の白線を位置を計算します。
            var ways = new MarkedWayListComposer().ComposeFrom(innerTarget);


            ways.AddRange(new StopLineComposer().ComposeFrom(innerTarget));
            
            foreach (var way in ways.MarkedWays)
            {
                // 道路の白線の位置からメッシュインスタンスに変換します。
                var gen = way.Type.ToLineMeshGenerator(way.IsReversed);
                var points = way.Line.Points;
                var instance = gen.GenerateMeshInstance(points.ToArray());

                yield return instance;
            }
        }
        

        /// <summary>
        /// 道路標示をゲームオブジェクトとして配置します。
        /// 引数の最初の2つを除くものをキーとし、シーン上に同じキーのものがあればそれを置き換えます。なければ新規作成します。
        /// </summary>
        private void GenerateGameObj(Mesh mesh, Material[] materials, Transform dstParent, RnRoadBase srcRoad,
            ReproducedRoadType reproducedType, ReproducedRoadDirection direction)
        {
            var targetRoads = srcRoad?.TargetTrans;
            var targetRoad = targetRoads == null || targetRoads.Count == 0 ? null : targetRoads.First();
            var targetName = targetRoad == null ? "UnknownRoad" : targetRoad.name;
            string dstName = $"{reproducedType.ToGameObjName()}-{targetName}";
            if(direction != ReproducedRoadDirection.None)
            {
                dstName += $"-{direction}";
            }
            GameObject dstObj = null;
            if (targetRoad != null)
            {
                // シーンに同じものがあれば、それを置き換えます。
                dstObj = PLATEAUReproducedRoad.Find(reproducedType, targetRoad.transform, direction);
            }

            if (dstObj == null)
            {
                // シーンに同じものがなければ新規作成します。
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
            comp.Init(reproducedType, targetRoad == null ? null : targetRoad.transform, direction);
        }
    }
}