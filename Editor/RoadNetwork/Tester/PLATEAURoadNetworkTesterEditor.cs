using PLATEAU.Editor.RoadNetwork.Graph;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure;
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

            public long TargetLaneId
            {
                get => target.Drawer?.laneOp?.showLaneId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.Drawer.laneOp.showLaneId = value;
                    }
                }
            }

            public long TargetRoadId
            {
                get => target.Drawer?.roadOp?.showRoadId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.Drawer.roadOp.showRoadId = value;
                    }
                }
            }

            public long TargetIntersectionId
            {
                get => target.Drawer?.intersectionOp?.showIntersectionId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.Drawer.intersectionOp.showIntersectionId = value;
                    }
                }
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

            public long TargetFaceId
            {
                get => target.RGraphDrawer?.faceOption?.targetId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.RGraphDrawer.faceOption.targetId = (int)value;
                    }
                }
            }

            public long TargetEdgeId
            {
                get => target.RGraphDrawer?.edgeOption?.targetId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.RGraphDrawer.edgeOption.targetId = (int)value;
                    }
                }
            }

            public long TargetVertexId
            {
                get => target.RGraphDrawer?.vertexOption?.targetId ?? -1;
                set
                {
                    if (target.Drawer != null)
                    {
                        target.RGraphDrawer.vertexOption.targetId = (int)value;
                    }
                }
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