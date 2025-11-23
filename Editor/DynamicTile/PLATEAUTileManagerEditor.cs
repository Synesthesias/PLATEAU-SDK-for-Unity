using PLATEAU.Editor.DynamicTile.TileModule;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private SerializedProperty onTileInstantiatedProperty;
        private SerializedProperty beforeTileUnloadProperty;

        // 強制高解像度タイル選択UI用のフィールド
        private string searchQuery = "";
        private HashSet<string> selectedTileAddresses = new HashSet<string>();
        private Vector2 scrollPosition = Vector2.zero;
        private PLATEAUDynamicTile[] filteredTileCacheArray = null;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
            DrawCatalogPathWithOpenButton(tileManager);
            if (GUILayout.Button("シーンビューで都市にフォーカス"))
            {
                TileManagerGenerator.FocusSceneViewCameraToTiles(tileManager);
                SceneView.lastActiveSceneView?.Repaint();
            }
            
            DrawForceHighResolutionTileEditor();

            // Event表示
            EditorGUILayout.PropertyField(onTileInstantiatedProperty);
            EditorGUILayout.PropertyField(beforeTileUnloadProperty);

            // デバッグ表示トグル
            var debugInfoProperty = serializedObject.FindProperty("showDebugTileInfo");
            EditorGUILayout.PropertyField(debugInfoProperty, new GUIContent("SDKデバッグ用情報を表示"));

            // デバッグON時のみ付随オプションを表示
            if (debugInfoProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugLog"));
            }

            if (debugInfoProperty.boolValue)
            {
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
                        SceneView.lastActiveSceneView?.Repaint();
                    }
                }


                if (GUILayout.Button("Cancel Load Task"))
                {
                    _= tileManager.CancelLoadTask();
                }

                if (!PLATEAUSceneViewCameraTracker.IsRunning)
                {
                    if (GUILayout.Button("Enable Realtime Load On Editor"))
                    {
                        PLATEAUSceneViewCameraTracker.Initialize();
                    }
                }
                else
                {
                    if (GUILayout.Button("Disable Realtime Load On Editor"))
                    {
                        PLATEAUSceneViewCameraTracker.Release();
                    }
                }

                if (GUILayout.Button("Update Assets By Camera Position"))
                {
                    var currentCamera = EditorApplication.isPlaying ? Camera.main : SceneView.currentDrawingSceneView?.camera ?? SceneView.lastActiveSceneView?.camera;
                    if (currentCamera != null)
                        _ = tileManager.UpdateAssetsByCameraPosition(currentCamera.transform.position);
                }
                
                EditorGUILayout.LabelField("OutputPath: " + tileManager.OutputPath);
                EditorGUILayout.Space();

                // Tile情報の表示
                var dynamicTiles = tileManager.DynamicTiles;
                EditorGUILayout.LabelField($"State: ", tileManager.State.ToString());
                EditorGUILayout.LabelField($"TileCreationInProgress: ", PLATEAUEditorEventListener.disableProjectChangeEvent.ToString());
                EditorGUILayout.IntField($"Tile num: ", dynamicTiles.Count);
                EditorGUILayout.LabelField($"Load Task: ", tileManager.HasCurrentTask ? "Running" : "Complete", new GUIStyle(EditorStyles.label) { normal = { textColor = tileManager.HasCurrentTask ? Color.red : Color.green } });
                EditorGUILayout.LabelField($"Instantiate Coroutine: ", tileManager.IsCoroutineRunning? "Running" : "Complete", new GUIStyle(EditorStyles.label) { normal = { textColor = tileManager.IsCoroutineRunning ? Color.red : Color.green } });

                // Zoom Levelごとのロード距離
                foreach (var dist in tileManager.loadDistances)
                {
                    var zoomLevel = dist.Key;
                    var (min, max) = dist.Value;
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                    EditorGUILayout.IntField($"ZoomLevel: ", zoomLevel);
                    EditorGUILayout.Vector2Field($"Load Distance: ", new Vector2(min, max));
                }

                // 各タイルの情報を表示
                foreach (var tile in dynamicTiles)
                {
                    if (tile == null) continue;
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                    EditorGUILayout.LabelField($"Tile Address: {tile.Address}");
                    EditorGUILayout.BoundsField($"Extent Bounds: ", tile.Extent);
                    EditorGUILayout.IntField($"ZoomLevel: ", tile.ZoomLevel);
                    EditorGUILayout.ObjectField($"LoadedObject: ", tile.LoadedObject, typeof(GameObject), true);
                    EditorGUILayout.LabelField($"NextLoadState: ", tile.NextLoadState.ToString());
                    EditorGUILayout.LabelField($"LoadHandle Valid: ", tile.LoadHandle.IsValid().ToString());
                    EditorGUILayout.LabelField($"LastLoadResult: ", tile.LastLoadResult.ToString());
                    EditorGUILayout.FloatField($"DistanceFromCamera: ", tile.DistanceFromCamera);
                }

                if (dynamicTiles.Count > 0)
                    Repaint();
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// カタログパスと、その横に「参照」ボタンを描画します。
        /// </summary>
        private void DrawCatalogPathWithOpenButton(PLATEAUTileManager tileManager)
        {
            var catalogProp = serializedObject.FindProperty("catalogPath");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(catalogProp, new GUIContent("Addressableカタログパス"));
            bool clickedOpen = GUILayout.Button("参照", GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();
            if (clickedOpen)
            {
                var initialDir = string.IsNullOrEmpty(catalogProp.stringValue)
                    ? Application.dataPath
                    : Path.GetDirectoryName(catalogProp.stringValue);
                var selected = EditorUtility.OpenFilePanelWithFilters(
                    "Addressablesのカタログを選択してください",
                    initialDir,
                    new[] { "カタログファイル", "json,bin", "すべてのファイル", "*" }
                );
                if (!string.IsNullOrEmpty(selected))
                {
                    selected = selected.Replace('\\', '/');
                    catalogProp.stringValue = selected;
                    serializedObject.ApplyModifiedProperties();

                    tileManager.ClearTiles();
                    tileManager.InitializeTiles().ContinueWithErrorCatch();
                }
            }
        }

        /// <summary>
        /// ZoomレベルをSceneViewに描画
        /// </summary>
        private bool isShowingZoomLevel = false;

        public void OnEnable()
        {
            isShowingZoomLevel = false;
            onTileInstantiatedProperty = serializedObject.FindProperty(nameof(PLATEAUTileManager.onTileInstantiated));
            beforeTileUnloadProperty = serializedObject.FindProperty(nameof(PLATEAUTileManager.beforeTileUnload));
        }

        public void OnDisable()
        {
            SceneView.duringSceneGui -= DrawZoomLevel;
            isShowingZoomLevel = false;
        }

        private Dictionary<int, Color> zoomLevelColors = new Dictionary<int, Color>
        {
            { 9, Color.red },
            { 10, Color.yellow },
            { 11, Color.green },
        };

        private Color GetLabelColor(PLATEAUDynamicTile tile)
        {
            if (tile.LoadHandle.IsValid() && tile.LoadHandle.IsDone)
                return tile.LoadedObject == null ? Color.white : zoomLevelColors[tile.ZoomLevel];

            return Color.black; // Loading中は黒
        }

        private void DrawZoomLevel(SceneView sceneView)
        {
            if(!isShowingZoomLevel)
            {
                SceneView.duringSceneGui -= DrawZoomLevel;
                return;
            }

            PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
            foreach (var tile in tileManager.DynamicTiles)
            {
                if (tile.NextLoadState == LoadState.Load )
                {
                    var center = tile.Extent.center;
                    Handles.BeginGUI();

                    Handles.Label(center, $"{tile.ZoomLevel}", new GUIStyle
                    {
                        fontSize = 32,
                        normal = new GUIStyleState { textColor = GetLabelColor(tile) },
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
                DebugEx.DrawBounds(tile.Extent, Color.red, 3f);
            }
        }

        private void DrawForceHighResolutionTileEditor()
        {
            //重いのでキャッシュ
            if (filteredTileCacheArray == null)
            {
                PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
                filteredTileCacheArray = tileManager.DynamicTiles.Where(m => m.ZoomLevel == 11).ToArray();
                selectedTileAddresses = tileManager.ForceHighResolutionTileAddresses?.Any() ?? false
                    ? new HashSet<string>(tileManager.ForceHighResolutionTileAddresses)
                    : new HashSet<string>();
            }

            if (filteredTileCacheArray == null || filteredTileCacheArray.Length == 0)
            {
                EditorGUILayout.HelpBox("タイルが読み込まれていません", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("常に高解像度で表示するタイル選択", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 検索ウィンドウ
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("検索", GUILayout.Width(40));
            searchQuery = EditorGUILayout.TextField(searchQuery);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            // 選択数の表示
            EditorGUILayout.LabelField($"選択中: {selectedTileAddresses.Count} タイル");


            // フィルタリングされたタイルリストを作成
            var filteredTiles = new List<PLATEAUDynamicTile>();
            foreach (var tile in filteredTileCacheArray)
            {
                if (tile == null) continue;

                // 検索クエリでフィルタリング
                if (string.IsNullOrEmpty(searchQuery) ||
                    tile.Address.Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase))
                {
                    filteredTiles.Add(tile);
                }
            }
            
            // リストビューヘッダー
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("選択", GUILayout.Width(50));
            EditorGUILayout.LabelField("タイルアドレス", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            foreach (var tile in filteredTiles)
            {
                EditorGUILayout.BeginHorizontal();

                // チェックボックス
                bool isSelected = selectedTileAddresses.Contains(tile.Address);
                bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(50));

                if (newSelected != isSelected)
                {
                    if (newSelected)
                    {
                        selectedTileAddresses.Add(tile.Address);
                    }
                    else
                    {
                        selectedTileAddresses.Remove(tile.Address);
                    }
                }

                // タイルアドレス（選択・コピー可能）
                EditorGUILayout.SelectableLabel(tile.Address, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            
            // ボタン領域
            EditorGUILayout.BeginHorizontal();
            
            // 編集完了ボタン
            if (GUILayout.Button($"保存/反映({selectedTileAddresses.Count}個選択中)", GUILayout.Height(30)))
            {
                Debug.Log($"編集完了: {selectedTileAddresses.Count}個のタイルが選択されました");
                PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
                tileManager.SetForceHighResolutionTileAddresses(selectedTileAddresses.ToArray());
                EditorUtility.SetDirty(tileManager);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog(nameof(PLATEAUTileManager),"強制高解像度タイル情報を更新しました。","OK");
            }


            // リセットボタン
            if (GUILayout.Button("リセット", GUILayout.Height(30)))
            {
                selectedTileAddresses.Clear();
                searchQuery = "";
                PLATEAUTileManager tileManager = (PLATEAUTileManager)target;
                tileManager.SetForceHighResolutionTileAddresses(selectedTileAddresses.ToArray());
                EditorUtility.DisplayDialog(nameof(PLATEAUTileManager),"強制高解像度タイル情報をリセットしました。","OK");

            }

           
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

    }
}