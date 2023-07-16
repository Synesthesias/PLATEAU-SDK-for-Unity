using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PLATEAU.Util;

namespace PLATEAU.CityImport.AreaSelector
{
    internal static class AreaSelectorGUI
    {
        private static Rect currentAreaSelectionWindowRect;
        private static Rect currentLodWindowRect;
        private static readonly Rect AreaSelectionWindowRect = new(15f, 15f, 150f, 0f);
        private static readonly Rect LodWindowRect = new(15f, Screen.height - 340f, 50f, 0f);
        private static readonly string IconDirPath = PathUtil.SdkPathToAssetPath("Images/AreaSelect");
        private static AreaSelectorBehaviour areaSelector;
        private static readonly List<Texture> LODIcons = new();
        private static readonly GUIStyle IconStyle = new(EditorStyles.label) { fixedHeight = 30f, fixedWidth = 30f };

        public static void Enable(AreaSelectorBehaviour areaSelectorArg)
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui += OnGUI;
            currentAreaSelectionWindowRect = AreaSelectionWindowRect;
            currentLodWindowRect = LodWindowRect;
            areaSelector = areaSelectorArg;
            foreach (var iconName in new List<string>{"lod01.png", "lod02.png", "lod03.png", "lod04.png"})
            {
                var path = Path.Combine(IconDirPath, iconName).Replace('\\', '/');
                var texture =  AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (texture == null)
                {
                    Debug.LogError($"Icon image file is not found : {path}");
                    continue;
                }
                LODIcons.Add(texture);
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
            currentAreaSelectionWindowRect = GUILayout.Window(1, currentAreaSelectionWindowRect, DrawAreaSelectionInsideWindow, "範囲選択");
            currentLodWindowRect.y = sceneView.position.size.y - currentLodWindowRect.height * EditorGUIUtility.pixelsPerPoint;
            currentLodWindowRect = GUILayout.Window(2, currentLodWindowRect, DrawLodInsideWindow, "LOD");
            
            Handles.EndGUI();
        }
#endif

        private static void DrawAreaSelectionInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(5f);
            if (GUILayout.Button("決定"))
            {
                areaSelector.EndAreaSelection();
            }
            if (GUILayout.Button("キャンセル"))
            {
                areaSelector.CancelAreaSelection();
            }
            GUILayout.Space(5f);
#endif
        }
        
        private static void DrawLodInsideWindow(int id)
        {
#if UNITY_EDITOR
            GUILayout.Space(5f);
            foreach (var lodIcon in LODIcons) 
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(lodIcon, IconStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5f);
#endif
        }        
    }
}
