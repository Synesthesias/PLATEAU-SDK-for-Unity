using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// PLATEAUTileManagerのEditor拡張クラス。
    /// </summary>
    [CustomEditor(typeof(PLATEAUTileManager))]
    public class PLATEAUTileManagerEditor : UnityEditor.Editor
    {
        public async override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // 通常のInspector表示
  
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty debugInfoProperty = serializedObject.FindProperty("showDebugTileInfo");

            if (debugInfoProperty.boolValue)
            {
                PLATEAUTileManager tileManager = (PLATEAUTileManager)target;

                if (GUILayout.Button("Clear Tile Assets"))
                {
                    tileManager.ClearTileAssets();
                    PLATEAUSceneViewCameraTracker.Initialize();
                }

                if (GUILayout.Button("Clear Tile List"))
                {
                    tileManager.ClearTiles();
                    PLATEAUSceneViewCameraTracker.Initialize();
                }

                if (GUILayout.Button("Load Tile Scriptable Objects"))
                {
                    _ = tileManager.InitializeTiles();  
                }

                if (GUILayout.Button("Update Assets By Camera Position"))
                {
                    var currentCamera = EditorApplication.isPlaying ? Camera.main : SceneView.currentDrawingSceneView?.camera ?? SceneView.lastActiveSceneView?.camera;
                    if (currentCamera != null)
                        await tileManager.UpdateAssetsByCameraPosition(currentCamera.transform.position);
                }

                if (GUILayout.Button("Show Tile Bounds"))
                {
                    tileManager.ShowBounds();
                    SceneView.lastActiveSceneView?.Repaint();
                }

                if (GUILayout.Button("Cancel Load Task"))
                {
                    tileManager.CancelLoadTask();
                }

                // Tile情報の表示
                var dynamicTiles = tileManager.DynamicTiles;
                EditorGUILayout.LabelField($"State: ", tileManager.State.ToString());
                EditorGUILayout.LabelField($"TileCreationInProgress: ", PLATEAUEditorEventListener.IsTileCreationInProgress.ToString());
                EditorGUILayout.IntField($"Tile num: ", dynamicTiles.Count);
                foreach (var tile in dynamicTiles)
                {
                    if (tile == null) continue;
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                    EditorGUILayout.LabelField($"Tile Address: {tile.Address}");
                    EditorGUILayout.IntField($"LOD: ", tile.Lod);
                    EditorGUILayout.BoundsField($"Extent Bounds: ", tile.Extent);
                    EditorGUILayout.IntField($"ZoomLevel: ", tile.ZoomLevel);
                    EditorGUILayout.ObjectField($"LoadedObject: ", tile.LoadedObject, typeof(GameObject), true);
                    EditorGUILayout.LabelField($"NextLoadState: ", tile.NextLoadState.ToString());
                    EditorGUILayout.LabelField($"LoadHandle Valid: ", tile.LoadHandle.IsValid().ToString());
                    EditorGUILayout.FloatField($"DistanceFromCamera: ", tile.DistanceFromCamera);
                }

                if (dynamicTiles.Count > 0)
                    Repaint();
            }
        }
    }
}