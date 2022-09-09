using PLATEAU.CityLoader.AreaSelector;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Import.AreaSelect
{
    public static class AreaSelectorGUI
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

        private static void OnGUI(SceneView sceneView)
        {
            #if UNITY_EDITOR
            Handles.BeginGUI();
            windowRect = GUILayout.Window(1, windowRect, DrawInsideWindow, "範囲選択");
            Handles.EndGUI();
            #endif
        }

        private static void DrawInsideWindow(int id)
        {
            #if UNITY_EDITOR
            if (GUILayout.Button("キャンセル"))
            {
                   areaSelector.OnCancelButtonPushed();
            }
            GUI.DragWindow();
            #endif
        }
    }
}
