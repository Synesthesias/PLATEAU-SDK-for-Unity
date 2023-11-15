using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Windows
{
    /// <summary>
    /// 範囲選択画面で操作方法を表示するウィンドウです。
    /// </summary>
    internal static class AreaSelectorGuideWindow
    {
#if UNITY_EDITOR
        private static Rect currentAreaSelectorGuideWindowRect;
        private static readonly Rect AreaSelectorGuideWindowRect = new(0, 0, 300f, 0);

        private static readonly GUIStyle LabelStyle = new(EditorStyles.label)
            { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

        private static GUIStyle windowStyle;
#endif
        public static void Enable()
        {
#if UNITY_EDITOR
            currentAreaSelectorGuideWindowRect = AreaSelectorGuideWindowRect;
            SceneView.duringSceneGui += OnGUI;
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
            windowStyle ??= new GUIStyle(GUI.skin.window)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            Handles.BeginGUI();
            currentAreaSelectorGuideWindowRect.x =
                sceneView.position.size.x - currentAreaSelectorGuideWindowRect.width - 15;
            currentAreaSelectorGuideWindowRect.y =
                sceneView.position.size.y - currentAreaSelectorGuideWindowRect.height - 40;
            currentAreaSelectorGuideWindowRect = GUILayout.Window(3, currentAreaSelectorGuideWindowRect,
                DrawAreaSelectorGuideInsideWindow, "操作方法", windowStyle);
            Handles.EndGUI();
        }
#endif
        private static void DrawAreaSelectorGuideInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(10f);
            GUILayout.Label("クリック：選択切替", LabelStyle);
            GUILayout.Space(5f);
            GUILayout.Label("ドラッグ：矩形を選択範囲に追加", LabelStyle);
            GUILayout.Space(5f);
            GUILayout.Label("Shift＋ドラッグ：矩形を選択範囲から除外", LabelStyle);
            GUILayout.Space(10f);
#endif
        }
    }
}