using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector
{
    public static class AreaSelectorGUI
    {
        private static Rect windowRect;
        private static readonly Rect initialWindowRect = new Rect(15f, 30f, 150f, 90f);
        private static AreaSelectorBehaviour areaSelector;

        public static void Enable(AreaSelectorBehaviour areaSelectorArg)
        {
            SceneView.duringSceneGui += OnGUI;
            windowRect = initialWindowRect;
            areaSelector = areaSelectorArg;
        }

        public static void Disable()
        {
            SceneView.duringSceneGui -= OnGUI;
        }

        private static void OnGUI(SceneView sceneView)
        {
            Handles.BeginGUI();
            windowRect = GUILayout.Window(1, windowRect, DrawInsideWindow, "範囲選択");
            Handles.EndGUI();
        }

        private static void DrawInsideWindow(int id)
        {
            if (GUILayout.Button("キャンセル"))
            {
                   areaSelector.OnCancelButtonPushed();
            }
            GUI.DragWindow();
        }
    }
}
