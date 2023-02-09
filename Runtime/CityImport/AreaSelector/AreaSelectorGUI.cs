using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector
{
    internal static class AreaSelectorGUI
    {
        private static Rect windowRect;
        private static readonly Rect initialWindowRect = new Rect(15f, 30f, 150f, 90f);
        private static AreaSelectorBehaviour areaSelector;

        public static void Enable(AreaSelectorBehaviour areaSelectorArg)
        {
            #if UNITY_EDITOR
            SceneView.duringSceneGui += OnGUI;
            windowRect = initialWindowRect;
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
            windowRect = GUILayout.Window(1, windowRect, DrawInsideWindow, "範囲選択");
            Handles.EndGUI();
        }
#endif

        private static void DrawInsideWindow(int id)
        {
            #if UNITY_EDITOR
            if (GUILayout.Button("決定"))
            {
                areaSelector.EndAreaSelection();
            }
            if (GUILayout.Button("キャンセル"))
            {
                areaSelector.CancelAreaSelection();
            }
            GUI.DragWindow();
            #endif
        }
    }
}
