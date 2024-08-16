using PLATEAU.Editor.RoadNetwork.Graph;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAURoadNetworkTester))]
    public class PLATEAURoadNetworkTesterEditor : UnityEditor.Editor
    {
        public void OnSceneGUI()
        {
            // RoadNetworkを所持しているオブジェクトに表示するGUIシステムを更新する処理
            UpdateRoadNetworkGUISystem();

            void UpdateRoadNetworkGUISystem()
            {
                var hasOpen = RoadNetworkEditorWindow.HasOpenInstances();
                if (hasOpen == false)
                {
                    return;
                }

                var editorInterface = RoadNetworkEditorWindow.GetEditorInterface();
                if (editorInterface == null)
                    return;

                //if (Event.current.type != EventType.Repaint)
                //    return;

                var guiSystem = editorInterface.SceneGUISystem;
                guiSystem.OnSceneGUI(target as PLATEAURoadNetworkTester);
            }
        }

        private class RnModelInstanceHelper : RnModelDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURoadNetworkTester target;

            public RnModelInstanceHelper(PLATEAURoadNetworkTester target)
            {
                this.target = target;
            }

            public RnModel GetModel()
            {
                return target.RoadNetwork;
            }

            public HashSet<RnRoad> TargetRoads => target.Drawer.TargetRoads;
            public HashSet<RnIntersection> TargetIntersections => target.Drawer.TargetIntersections;
            public HashSet<RnLane> TargetLanes => target.Drawer.TargetLanes;
            public HashSet<RnWay> TargetWays => target.Drawer.TargetWays;
            public HashSet<RnSideWalk> TargetSideWalks => target.Drawer.TargetSideWalks;

            public bool IsTarget(RnRoadBase roadBase)
            {
                return RnEx.IsEditorSceneSelected(roadBase.CityObjectGroup);
            }
        }

        private class RGraphInstanceHelper : RGraphDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURoadNetworkTester target;

            public RGraphInstanceHelper(PLATEAURoadNetworkTester target)
            {
                this.target = target;
            }

            public RGraph GetGraph()
            {
                return target.RGraph;
            }

            public RGraph CreateGraph()
            {
                target.CreateRGraph();
                return target.RGraph;
            }

            public HashSet<RFace> TargetFaces => target.RGraphDrawer.TargetFaces;
            public HashSet<REdge> TargetEdges => target.RGraphDrawer.TargetEdges;
            public HashSet<RVertex> TargetVertices => target.RGraphDrawer.TargetVertices;

            public void CreateTranMesh()
            {
                target.Factory.midStageData.CreateAll(target.GetTargetCityObjects());
            }

            public bool IsTarget(RFace face)
            {
                return RnEx.IsEditorSceneSelected(face.CityObjectGroup);
            }

            public void CreateRnModel()
            {
                target.CreateRoadNetworkByGraphAsync();
            }

            public RGraphDrawerDebug GetDrawer()
            {
                return target.RGraphDrawer;
            }
        }

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURoadNetworkTester;
            if (!obj)
                return;

            base.OnInspectorGUI();
            GUILayout.Label($"ConvertedCityObjectVertexCount : {obj.SubDividedCityObjects.Sum(c => c.Meshes.Sum(m => m.Vertices.Count))}");

            if (GUILayout.Button("Create"))
                obj.CreateNetwork();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Serialize"))
                    obj.Serialize();

                if (GUILayout.Button("Deserialize"))
                    obj.Deserialize();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (obj.RGraph != null && GUILayout.Button("Open RGraph Editor"))
                    RGraphDebugEditorWindow.OpenWindow(new RGraphInstanceHelper(obj), true);

                if (GUILayout.Button("RnModel Debug Editor"))
                    RnModelDebugEditorWindow.OpenWindow(new RnModelInstanceHelper(obj), true);
            }

            if (GUILayout.Button("Check Lod"))
                obj.RemoveSameNameCityObjectGroup();
        }
    }
}