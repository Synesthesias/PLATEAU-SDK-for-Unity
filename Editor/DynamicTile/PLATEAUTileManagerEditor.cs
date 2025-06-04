using PLATEAU.Editor.Window.Common;
using System.Collections.Generic;
using UnityEditor;
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

            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;

            if (PLATEAUTileManager.showDebugTileInfo)
            {
                if (GUILayout.Button("Clear All"))
                {
                    tileManager.ClearAll();
                    SceneViewCameraTracker.Initialize();
                }

                if (GUILayout.Button("Show Debug Extent"))
                {
                    tileManager.ShowBounds();
                }


                var dynamicTiles = tileManager.DynamicTiles;
                foreach (var tile in dynamicTiles)
                {
                    if (tile == null) continue;

                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

                    EditorGUILayout.LabelField($"Tile Address: {tile.Address}");
                    EditorGUILayout.IntField($"LOD: ", tile.Lod);
                    EditorGUILayout.LabelField($"Extent: ", tile.Extent.ToString());
                    EditorGUILayout.LabelField($"LoadedObject: ", tile.LoadedObject?.name ?? "-");
                    EditorGUILayout.LabelField($"NextLoadState: ", tile.NextLoadState.ToString());
                    EditorGUILayout.FloatField($"DistanceFromCamera: ", tile.DistanceFromCamera);
                }
                EditorGUILayout.LabelField($"Tile num {dynamicTiles.Count}");

            }
        }
    }
}
