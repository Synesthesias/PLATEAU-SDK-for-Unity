using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Structure
{
    [CustomEditor(typeof(PLATEAURnStructureModel))]
    public class PLATEAURnStructureModelEditor : UnityEditor.Editor
    {
        private class RnModelInstanceHelper : RnModelDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURnStructureModel target;

            private PLATEAURnModelDrawerDebug Drawer
            {
                get
                {
                    if (!target)
                        return null;
                    return target.gameObject.GetOrAddComponent<PLATEAURnModelDrawerDebug>();
                }
            }

            public RnModelInstanceHelper(PLATEAURnStructureModel target)
            {
                this.target = target;
            }

            public RnModel GetModel()
            {
                return target.RoadNetwork;
            }

            public HashSet<object> InVisibleObjects => Drawer.InVisibleObjects;
            public HashSet<object> SelectedObjects => Drawer.SelectedObjects;

            public bool IsSceneSelected(RnRoadBase roadBase)
            {
                return RnEx.IsEditorSceneSelected(roadBase.CityObjectGroup);
            }
        }


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
                guiSystem.OnSceneGUI(target as PLATEAURnStructureModel);
            }
        }
        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURnStructureModel;
            if (!obj)
                return;

            base.OnInspectorGUI();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Serialize"))
                    obj.Serialize();

                if (GUILayout.Button("Deserialize"))
                    obj.Deserialize();
            }

            if (GUILayout.Button("RnModel Debug Editor"))
                RnModelDebugEditorWindow.OpenWindow(new RnModelInstanceHelper(obj), true);
        }
    }
}