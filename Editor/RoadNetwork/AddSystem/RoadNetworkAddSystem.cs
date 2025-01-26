using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadNetwork.Structure;
using UnityEditor;
using UnityEngine;
using PLATEAU.Editor.RoadNetwork.AddSystem;
using System.Collections.Generic;

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
                    new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase> { obj }), crosswalkExists ? CrosswalkFrequency.All : CrosswalkFrequency.Delete, true);

                    // スケルトン更新
                    Context.SkeletonData.ReconstructIncludeNeighbors(obj);
                }
            };

            IntersectionAddSystem = new IntersectionAddSystem(Context);
            IntersectionAddSystem.OnIntersectionAdded = (intersection) =>
            {
                // 交差点モデル再生成
                var generatedObj = new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase>() { intersection }), CrosswalkFrequency.All, true);
                if (generatedObj != null)
                {
                    intersection.AddTargetTran(generatedObj);
                }

                // 隣接道路更新
                // TODO: 道路が更新された場合のみ更新するようにする
                foreach (var neighbor in intersection.Neighbors)
                {
                    if (neighbor.Road != null)
                        new RoadReproducer().Generate(new RrTargetRoadBases(Context.RoadNetwork, new List<RnRoadBase>() { neighbor.Road }), CrosswalkFrequency.All, true);
                }

                // スケルトン更新
                Context.SkeletonData.ReconstructIncludeNeighbors(intersection);
            };
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
