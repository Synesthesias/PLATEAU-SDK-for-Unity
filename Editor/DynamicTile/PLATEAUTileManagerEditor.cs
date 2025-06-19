using log4net.Util;
using PLATEAU.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

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
                    ShowBounds();
                    SceneView.lastActiveSceneView?.Repaint();
                }

                if (isShowingZoomLevel)
                {
                    if (GUILayout.Button("Hide Zoom Level"))
                    {
                        SceneView.duringSceneGui -= DrawZoomLevel;
                        isShowingZoomLevel = false;
                        SceneView.lastActiveSceneView?.Repaint();
                    }
                }
                else
                {
                    if (GUILayout.Button("Show Zoom Level"))
                    {
                        SceneView.duringSceneGui -= DrawZoomLevel;
                        SceneView.duringSceneGui += DrawZoomLevel;
                        isShowingZoomLevel = true;
                    }
                }


                if (GUILayout.Button("Cancel Load Task"))
                {
                    _= tileManager.CancelLoadTask();
                }

                // Tile情報の表示
                var dynamicTiles = tileManager.DynamicTiles;
                EditorGUILayout.LabelField($"State: ", tileManager.State.ToString());
                EditorGUILayout.LabelField($"TileCreationInProgress: ", PLATEAUEditorEventListener.IsTileCreationInProgress.ToString());
                EditorGUILayout.IntField($"Tile num: ", dynamicTiles.Count);
                EditorGUILayout.LabelField($"Task Running: ", tileManager.HasCurrentTask.ToString(), new GUIStyle(EditorStyles.label) { normal = { textColor = tileManager.HasCurrentTask ? Color.red : Color.green } });
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

        public void OnEnable()
        {
            isShowingZoomLevel = false;
        }

        public void OnDisable()
        {
            SceneView.duringSceneGui -= DrawZoomLevel;
            isShowingZoomLevel = false;
        }

        /// <summary>
        /// ZoomレベルをSceneViewに描画
        /// </summary>
        private bool isShowingZoomLevel = false;
        private void DrawZoomLevel(SceneView sceneView)
        {
            if(!isShowingZoomLevel)
            {
                SceneView.duringSceneGui -= DrawZoomLevel;
                return;
            }

            Dictionary<int, Color> zoomLevelColors = new Dictionary<int, Color>
            {
                { 9, Color.red },
                { 10, Color.yellow },
                { 11, Color.green },
            };

            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
            foreach (var tile in tileManager.DynamicTiles)
            {
                if (tile.LoadHandle.IsDone && tile.LoadedObject != null)
                {
                    var center = tile.Extent.center;
                    Handles.BeginGUI();
                    Handles.Label(center, $"{tile.ZoomLevel}", new GUIStyle
                    {
                        fontSize = 32,
                        normal = new GUIStyleState { textColor = zoomLevelColors[ tile.ZoomLevel ] },
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    });
                    Handles.EndGUI();
                    DebugEx.DrawBounds(tile.Extent, zoomLevelColors[tile.ZoomLevel], 0.01f);
                }
            }
            sceneView.Repaint();
        }

        // Debug用Bounds表示
        private void ShowBounds()
        {
            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
            foreach (var tile in tileManager.DynamicTiles)
            {
                DebugEx.DrawBounds(tile.Extent, Color.red, 30f);
            }
        }

    }
}