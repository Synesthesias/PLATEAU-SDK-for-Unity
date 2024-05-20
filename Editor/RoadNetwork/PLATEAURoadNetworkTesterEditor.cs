using PLATEAU.RoadNetwork;
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
                guiSystem.SetEditingTarget(target as PLATEAURoadNetworkTester);
                guiSystem.OnSceneGUI();
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
                cog.Serialize();

            if (GUILayout.Button("Deserialize"))
                cog.Deserialize();
            if (GUILayout.Button("Save as presets"))
            {
                cog.savedTargets.Add(new PLATEAURoadNetworkTester.TestTargetPresets
                {
                    name = $"Name_{cog.savedTargets.Count}",
                    targets = cog.targets.ToList()
                });
            }
            var preset = cog.savedTargets.FirstOrDefault(c => c.name == cog.loadPresetName);
            if (preset != null && GUILayout.Button("Load from presets"))
            {
                cog.targets = preset.targets.ToList();
            }
        }
    }
}