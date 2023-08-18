using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PLATEAU.Util;

namespace PLATEAU.CityImport.AreaSelector
{
    internal static class LodLegendGUI
    {
#if UNITY_EDITOR
        private static Rect currentLodLegendWindowRect;
        private static readonly Rect LodLegendWindowRect = new(15f, Screen.height - 340f, 50f, 0f);
        private static readonly string IconDirPath = PathUtil.SdkPathToAssetPath("Images/AreaSelect");
        private static readonly List<Texture> LodLegendIcons = new();
        private static readonly GUIStyle IconStyle = new(EditorStyles.label) { fixedHeight = 30f, fixedWidth = 30f };
#endif
        public static void Enable()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui += OnGUI;
            currentLodLegendWindowRect = LodLegendWindowRect;
            if (0 < LodLegendIcons.Count)
                return;
                
            foreach (var iconName in new List<string>{"lod01.png", "lod02.png", "lod03.png", "lod04.png"})
            {
                var path = Path.Combine(IconDirPath, iconName).Replace('\\', '/');
                var texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (texture == null)
                {
                    Debug.LogError($"Icon image file is not found : {path}");
                    continue;
                }
                LodLegendIcons.Add(texture);
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
            currentLodLegendWindowRect.y = sceneView.position.size.y - currentLodLegendWindowRect.height * EditorGUIUtility.pixelsPerPoint;
            currentLodLegendWindowRect = GUILayout.Window(2, currentLodLegendWindowRect, DrawLodLegendInsideWindow, "LOD");
            Handles.EndGUI();
        }
#endif
        private static void DrawLodLegendInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(5f);
            foreach (var lodLegendIcon in LodLegendIcons) 
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(lodLegendIcon, IconStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5f);
#endif
        }        
    }
}
