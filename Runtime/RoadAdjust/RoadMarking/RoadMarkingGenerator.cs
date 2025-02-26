using PLATEAU.RoadAdjust.RoadMarking.Crosswalk;
using PLATEAU.RoadAdjust.RoadMarking.DirectionalArrow;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
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
    internal class RoadMarkingGenerator
    {
        private readonly IRrTarget targetBeforeCopy;
        private CrosswalkFrequency crosswalkFrequency;
        
        
        public RoadMarkingGenerator(IRrTarget target, CrosswalkFrequency crosswalkFrequency)
        {
            
            if (target != null)
            {
                this.targetBeforeCopy = target;
            }
            else
            {
                Debug.LogError("target road network is null.");
            }

            this.crosswalkFrequency = crosswalkFrequency;
        }

        /// <summary>
        /// 路面標示をメッシュとして生成します。
        /// 生成されたもののリストを返します。
        /// </summary>
        public List<PLATEAUReproducedRoad> Generate(ISmoothingStrategy smoothingStrategy)
        {
            var resultList = new List<PLATEAUReproducedRoad>();
            if (targetBeforeCopy == null)
            {
                Debug.LogError("道路ネットワークが見つかりませんでした。");
                return resultList;
            }
            

            using var progressDisplay = new ProgressDisplayDialogue();
            progressDisplay.SetProgress("道路標示生成", 5f, "道路ネットワークをコピー中");
            
            var target = targetBeforeCopy.Copy(); // 道路ネットワークを処理中だけ調整したいのでディープコピーを対象にします。
            
            progressDisplay.SetProgress("道路標示生成", 10f, "道路ネットワークをスムージング中");
            new RoadNetworkLineSmoother().Smooth(target, smoothingStrategy);

            var dstParent = RoadReproducer.GenerateDstParent();

            var roadBases = target.RoadBases().ToArray();
            var roadSources = roadBases.Select(rb => new RoadReproduceSource(rb)).ToArray();
            
            // 路面標示のうち、道路ごとに結合するものをこの道路forループの中に書きます。
            for (int i = 0; i < roadSources.Length; i++)
            {
                var roadSource = roadSources[i];
                string roadName = roadSource.GetName();
                progressDisplay.SetProgress("道路標示生成", (float)i / roadSources.Length,
                    $"[{i + 1} / {roadSources.Length}] {roadName}");
                var innerTarget = new RrTargetRoadBases(target.Network(), new[]{roadBases[i]});

                var combiner = new RoadMarkingCombiner();
                
                // 白線を生成します。
                combiner.AddRange(GenerateRoadLines(innerTarget)); 
                

                // 交差点前の矢印を生成します。
                var arrowComposer = new DirectionalArrowComposer(innerTarget);
                combiner.AddRange(arrowComposer.Compose());

                var dstMesh = combiner.Combine(out var materials);
                var gen = GenerateGameObj(dstMesh, materials, dstParent, roadSource, ReproducedRoadType.LaneLineAndArrow, ReproducedRoadDirection.None);
                resultList.Add(gen);
            }
            
            // 横断歩道を生成します。
            var crosswalkInstances = new CrosswalkComposer().Compose(target, crosswalkFrequency);
            foreach (var crosswalk in crosswalkInstances)
            {
                var crosswalkCombiner = new RoadMarkingCombiner();
                crosswalkCombiner.Add(crosswalk.RoadMarkingInstance);
                var crosswalkMesh = crosswalkCombiner.Combine(out var crosswalkMats);
                var srcRoad = new RoadReproduceSource(crosswalk.SrcRoad);
                var gen = GenerateGameObj(crosswalkMesh, crosswalkMats, dstParent, srcRoad, ReproducedRoadType.Crosswalk, crosswalk.Direction);
                resultList.Add(gen);
            }

            return resultList;
        }

        private IEnumerable<RoadMarkingInstance> GenerateRoadLines(IRrTarget innerTarget)
        {
            // 道路の白線を位置を計算します。
            var ways = new MarkedWayListComposer().ComposeFrom(innerTarget);

            // 停止線を生成します
            var stopLines = new StopLineComposer().ComposeFrom(innerTarget);
            ways.AddRange(stopLines);
            
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
        private PLATEAUReproducedRoad GenerateGameObj(Mesh mesh, Material[] materials, Transform dstParent, RoadReproduceSource srcRoad,
            ReproducedRoadType reproducedType, ReproducedRoadDirection direction)
        {
            var targetName = srcRoad.GetName();
            string dstName = $"{reproducedType.ToGameObjName()}-{targetName}";
            if(direction != ReproducedRoadDirection.None)
            {
                dstName += $"-{direction}";
            }
            // シーンに同じものがあれば、それを置き換えます。
            GameObject dstObj = PLATEAUReproducedRoad.Find(reproducedType, srcRoad, direction);

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
            comp.Init(reproducedType, srcRoad, direction);
            return comp;
        }
    }
}