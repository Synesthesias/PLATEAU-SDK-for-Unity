using PLATEAU.Editor.RoadNetwork.Graph;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
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

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURoadNetworkTester;
            if (!obj)
                return;

            base.OnInspectorGUI();
            GUILayout.Label($"ConvertedCityObjectVertexCount : {obj.convertedCityObjects.Sum(c => c.Meshes.Sum(m => m.Vertices.Count))}");

            if (GUILayout.Button("Create"))
                obj.CreateNetwork();

            if (GUILayout.Button("Serialize"))
                obj.Serialize();

            if (GUILayout.Button("Deserialize"))
                obj.Deserialize();

            if (GUILayout.Button("SplitCityObject"))
                obj.SplitCityObjectAsync();

            if (GUILayout.Button("RGraph"))
                obj.CreateRGraph();

            if (obj.RGraph != null && GUILayout.Button("Open RGraph Editor"))
                RGraphDebugEditorWindow.OpenWindow(obj.RGraph, true);

            if (GUILayout.Button("Check Lod"))
                obj.RemoveSameNameCityObjectGroup();

            if (GUILayout.Button("RnModel Debug Editor"))
                RnModelDebugEditorWindow.OpenWindow(new RnModelInstanceHelper(obj), true);
        }
    }
}