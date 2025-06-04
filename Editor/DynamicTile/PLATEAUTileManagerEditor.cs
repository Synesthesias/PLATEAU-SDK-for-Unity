using PLATEAU.Editor.Window.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUTileManagerのEditor拡張クラス。
    /// </summary>
    [CustomEditor(typeof(PLATEAUTileManager))]
    public class PLATEAUTileManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // 通常のInspector表示
  
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty debugInfoProperty = serializedObject.FindProperty("showDebugTileInfo");

            if (debugInfoProperty.boolValue)
            {
                PLATEAUTileManager tileManager = (PLATEAUTileManager)target;

                if (GUILayout.Button("Clear Tiles"))
                {
                    tileManager.ClearTileAssets();
                    SceneViewCameraTracker.Initialize();
                }

                if (GUILayout.Button("Load Tiles"))
                {
                    var task = tileManager.InitializeTiles();
                }

                if (GUILayout.Button("Show Tile Bounds"))
                {
                    tileManager.ShowBounds();
                }

                // Tile情報の表示
                var dynamicTiles = tileManager.DynamicTiles;
                EditorGUILayout.LabelField($"Tile num {dynamicTiles.Count}");
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

                foreach (var tile in dynamicTiles)
                {
                    if (tile == null) continue;
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                    EditorGUILayout.LabelField($"Tile Address: {tile.Address}");
                    EditorGUILayout.IntField($"LOD: ", tile.Lod);
                    EditorGUILayout.LabelField($"Extent: ", tile.Extent.ToString());
                    EditorGUILayout.LabelField($"LoadedObject: ", tile.LoadedObject != null ? tile.LoadedObject.name : "-");
                    EditorGUILayout.LabelField($"NextLoadState: ", tile.NextLoadState.ToString());
                    EditorGUILayout.FloatField($"DistanceFromCamera: ", tile.DistanceFromCamera);
                }
                
                Repaint();
            }
        }
    }
}