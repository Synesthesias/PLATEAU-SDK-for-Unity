using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector
{
    internal static class AreaSelectorGUI
    {
        private static Rect currentAreaSelectionWindowRect;
        private static readonly Rect AreaSelectionWindowRect = new(15f, 15f, 150f, 0f);
        private static AreaSelectorBehaviour areaSelector;

        public static void Enable(AreaSelectorBehaviour areaSelectorArg)
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui += OnGUI;
            currentAreaSelectionWindowRect = AreaSelectionWindowRect;
            areaSelector = areaSelectorArg;
#endif
        }

        public static void Disable()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnGUI;
#endif
        }

#if UNITY_EDITOR
        private static void OnGUI(SceneView sceneView)
        {
            Handles.BeginGUI();
            currentAreaSelectionWindowRect = GUILayout.Window(1, currentAreaSelectionWindowRect, DrawAreaSelectionInsideWindow, "範囲選択");
            Handles.EndGUI();
        }
#endif

        private static void DrawAreaSelectionInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(5f);
            if (GUILayout.Button("全選択解除"))
            {
                areaSelector.ResetSelectedArea();
            }
            EditorGUI.BeginDisabledGroup(!areaSelector.IsSelectedArea());
            if (GUILayout.Button("決定"))
            {
                areaSelector.EndAreaSelection(); 
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("キャンセル"))
            {
                areaSelector.CancelAreaSelection();
            }
            GUILayout.Space(5f);
            GUI.DragWindow();
#endif
        }        
    }
}
