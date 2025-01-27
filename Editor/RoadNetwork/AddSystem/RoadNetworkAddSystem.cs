using PLATEAU.CityInfo;
using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadNetwork.Structure;
using UnityEditor;
using UnityEngine;
using PLATEAU.Editor.RoadNetwork.AddSystem;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.Editor.RoadNetwork
{
    internal class RoadNetworkAddSystem
    {
        public static RoadNetworkAddSystem Active { get; private set; }

        public RoadNetworkAddSystemContext Context { get; private set; }
        public RnRoadAddSystem RoadAddSystem { get; private set; }
        public IntersectionAddSystem IntersectionAddSystem { get; private set; }

        private RoadNetworkAddSystem(PLATEAURnStructureModel structureModel)
        {
            Context = new RoadNetworkAddSystemContext(structureModel);

            RoadAddSystem = new RnRoadAddSystem(Context);
            RoadAddSystem.OnRoadAdded = (dirtyObjects) =>
            {
                foreach (var obj in dirtyObjects)
                {
                    if (obj is RnIntersection)
                    {
                        ((RnIntersection)obj).BuildTracks();
                    }

                    // 道路モデル再生成
                    var road = new RoadReproduceSource(obj);
                    bool crosswalkExists = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, road, ReproducedRoadDirection.Next);
                    var generatedObjs = new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase> { obj }), crosswalkExists ? CrosswalkFrequency.All : CrosswalkFrequency.Delete, true);
                    UpdateTargetTrans(obj, generatedObjs);

                    // スケルトン更新
                    Context.SkeletonData.ReconstructIncludeNeighbors(obj);
                }
            };

            IntersectionAddSystem = new IntersectionAddSystem(Context);
            IntersectionAddSystem.OnIntersectionAdded = (intersection, removedRoad) =>
            {
                // 旧道路削除
                if (removedRoad != null)
                {
                    var road = new RoadReproduceSource(removedRoad);
                    var meshObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.RoadMesh, road, ReproducedRoadDirection.None);
                    var crosswalkObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.Crosswalk, road, ReproducedRoadDirection.None);
                    var lineObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.LaneLineAndArrow, road, ReproducedRoadDirection.None);
                    if (meshObj != null)
                    {
                        Object.DestroyImmediate(meshObj);
                    }
                    if (crosswalkObj != null)
                    {
                        Object.DestroyImmediate(crosswalkObj);
                    }
                    if (lineObj != null)
                    {
                        Object.DestroyImmediate(lineObj);
                    }
                }

                // 隣接道路更新
                // TODO: 道路が更新された場合のみ更新するようにする
                foreach (var neighbor in intersection.Neighbors)
                {
                    if (neighbor.Road != null)
                    {
                        var generatedObjs = new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase>() { neighbor.Road }), CrosswalkFrequency.All, true);
                        UpdateTargetTrans(neighbor.Road, generatedObjs);
                    }
                }

                {
                    // 交差点モデル再生成
                    var generatedObjs = new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase>() { intersection }), CrosswalkFrequency.All, true);
                    UpdateTargetTrans(intersection, generatedObjs);
                }

                // スケルトン更新
                Context.SkeletonData.ReconstructIncludeNeighbors(intersection);
                Context.SkeletonData.Roads.RemoveAll(s => s.Road == removedRoad);
            };
        }

        /// <summary> 生成後、TargetTransを更新します。ただし更新が必要な場合に限ります。 </summary>
        private void UpdateTargetTrans(RnRoadBase roadBase, List<PLATEAUReproducedRoad> reproducedRoads)
        {
            if (reproducedRoads == null || reproducedRoads.Count == 0) return;
            
            // TargetTransがすでに存在する場合はそのままにします。存在しない場合は生成後に付け替えます。
            // こうすることで、道路編集で何を対象に編集したのかの判定に一貫性を持たせます。
            if (roadBase.TargetTrans.All(t => t == null))
            {
                // 道路ネットワークのTargetTransを更新
                roadBase.TargetTrans.Clear();
                roadBase.TargetTrans.AddRange(reproducedRoads.Select(g => g.GetComponent<PLATEAUCityObjectGroup>()).Where(c => c != null));

                // PLATEAUReproducedRoadのsourceを更新
                foreach (var r in reproducedRoads)
                {
                    r.reproduceSource = new RoadReproduceSource(roadBase);
                }
            }
        }

        public static bool TryInitializeGlobal()
        {
            Active?.Terminate();

            var structureModel = Object.FindObjectOfType<PLATEAURnStructureModel>();
            if (structureModel == null)
                return false;

            Active = new RoadNetworkAddSystem(structureModel);

            // SceneViewの更新イベントにフック
            SceneView.duringSceneGui += Active.OnSceneGUI;

            // マウスカーソルをシーンビューにホバーしないとシーンビューが更新されないため、強制的に再描画
            SceneView.RepaintAll();

            return true;
        }

        public static void TerminateGlobal()
        {
            if (Active == null)
                return;

            Active.Terminate();

            // SceneViewの更新イベントから除外
            SceneView.duringSceneGui -= Active.OnSceneGUI;

            Active = null;

            return;
        }

        private void Terminate()
        {
            RoadAddSystem.Deactivate();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            RoadAddSystem.HandleSceneGUI(sceneView);
            IntersectionAddSystem.HandleSceneGUI(sceneView);

            // マウスカーソルをシーンビューにホバーしないとシーンビューが更新されないため、強制的に再描画
            SceneView.RepaintAll();
        }
    }
}
