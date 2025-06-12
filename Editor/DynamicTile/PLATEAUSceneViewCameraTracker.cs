using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Editor Mode 時のSceneViewカメラの位置を監視し、PLATEAUTileManagerに通知するクラス。
    /// </summary>
    [InitializeOnLoad]
    public class PLATEAUSceneViewCameraTracker
    {
        private static PLATEAUTileManager tileManager;

        static PLATEAUSceneViewCameraTracker()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (EditorApplication.isPlaying)
                return;

            tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            tileManager.ClearTileAssets();
        }

        private static async void OnSceneGUI(SceneView sceneView)
        {
            if (EditorApplication.isPlaying)
                return;

            if (tileManager == null)
                return;

            Camera sceneCamera = sceneView.camera;
            if (sceneCamera != null)
            {
                Vector3 currentPosition = sceneCamera.transform.position;
                if (currentPosition != tileManager.LastCameraPosition)
                {
                    await tileManager.UpdateAssetsByCameraPosition(currentPosition);
                }
            }
        }
    }

    /// <summary>
    /// Unity Editorの起動時やシーンオープン時にPLATEAUTileManagerを初期化や、SceneViewCameraTrackerの初期化を行うするクラス。
    /// </summary>
    [InitializeOnLoad]
    public class PLATEAUEditorEventListener : UnityEditor.AssetModificationProcessor
    {
        // EditorのEvent発行時にデバッグログを表示するかどうかのフラグ
        public const bool ShowDebugLog = false;

        static PLATEAUEditorEventListener()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            EditorApplication.projectChanged -= OnProjectChanged; // プロジェクトが変更されたときにOnEditorUpdateを呼び出す
            EditorApplication.projectChanged += OnProjectChanged;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorSceneManager.sceneSaving += OnSceneSaving;

            InitView();
        }

        static void OnEditorUpdate()
        {
            Log("Unity Editor Started");
            EditorApplication.update -= OnEditorUpdate; // 一度だけ実行

            InitView();
        }

        static void OnProjectChanged()
        {
            Log("Project Changed");

            InitView();
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            var tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            Log($"Scene Saving : {scene.name}, save path : {path}");

            // Addressablesのアンロード処理を実行
            tileManager.ClearTileAssets();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            var tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Log("Play Mode about to start");
                tileManager.ClearTileAssets();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Log("Play Mode about to end");
                PLATEAURuntimeCameraTracker.StopCameraTracking();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Log($"Play Mode ended (Entered Edit Mode) {EditorApplication.isPlayingOrWillChangePlaymode}");
                InitView();
            }
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            var tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            Log($"Scene Opened: {scene.name}");
            InitView();
        }
        static async void InitView()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Log("InitView");

            var tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            await tileManager.InitializeTiles();
            PLATEAUSceneViewCameraTracker.Initialize();
        }

        static void Log(string message)
        {
            if (ShowDebugLog)
            {
                Debug.Log(message);
            }
        }
    }
}