﻿using PLATEAU.RoadNetwork;
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

        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAURoadNetworkTester;
            if (!cog)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Create"))
                cog.CreateNetwork();

            if (GUILayout.Button("Serialize"))
                cog.RoadNetwork.Serialize();

            if (GUILayout.Button("Deserialize"))
                cog.RoadNetwork.Deserialize();

            if (GUILayout.Button("SplitCityObject"))
                cog.SplitCityObjectAsync();
        }
    }
}