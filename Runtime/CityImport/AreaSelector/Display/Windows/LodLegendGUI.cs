using System.Collections.Generic;
using System.IO;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Windows
{
    /// <summary>
    /// 範囲選択画面で、凡例としてLODと色の対応関係を表示するウィンドウのクラスです。
    /// </summary>
    internal static class LodLegendGUI
    {
#if UNITY_EDITOR
        private static Rect currentLodLegendWindowRect;
        private static readonly Rect LodLegendWindowRect = new(15f, 0, 0, 0);
        private static readonly string IconDirPath = PathUtil.SdkPathToAssetPath("Images/AreaSelect");
        private static readonly List<bool> LodLegendCheckStates = new();
        private static readonly List<UnityEngine.Texture> LodLegendTextures = new();
        private static readonly GUIStyle IconStyle = new(EditorStyles.label) { fixedHeight = 30f, fixedWidth = 30f };
        private static readonly GUIStyle ToggleStyle = new(EditorStyles.toggle) { margin = new RectOffset(8, 0, 10, 0)};
        private static AreaSelectorBehaviour areaSelector;

#endif
        public static void Enable(AreaSelectorBehaviour areaSelectorArg)
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui += OnGUI;
            currentLodLegendWindowRect = LodLegendWindowRect;
            areaSelector = areaSelectorArg;

            if (0 < LodLegendTextures.Count)
            {
                LodLegendCheckStates.Clear();
                for (var i = 0; i < LodLegendTextures.Count; i++)
                {
                    LodLegendCheckStates.Add(true);
                }

                return;
            }
                
            foreach (var iconName in new List<string>{"lod01.png", "lod02.png", "lod03.png", "lod04.png"})
            {
                var path = Path.Combine(IconDirPath, iconName).Replace('\\', '/');
                var texture = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(path);
                if (texture == null)
                {
                    Debug.LogError($"Icon image file is not found : {path}");
                    continue;
                }

                LodLegendCheckStates.Add(true);
                LodLegendTextures.Add(texture);
            }
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
            currentLodLegendWindowRect.y = sceneView.position.height - currentLodLegendWindowRect.height - 40;
            currentLodLegendWindowRect = GUILayout.Window(2, currentLodLegendWindowRect, DrawLodLegendInsideWindow, "LOD");
            Handles.EndGUI();
        }
#endif
        private static void DrawLodLegendInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(10f);
            for (var i = 0; i < LodLegendCheckStates.Count; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(10f);
                    GUILayout.Box(LodLegendTextures[i], IconStyle);
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        LodLegendCheckStates[i] = GUILayout.Toggle(LodLegendCheckStates[i], "", ToggleStyle);
                        if (scope.changed)
                        {
                            areaSelector.SwitchLodIcon(i + 1, LodLegendCheckStates[i]);
                        }
                    }
                    GUILayout.Space(10f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10f);
#endif
        }        
    }
}
