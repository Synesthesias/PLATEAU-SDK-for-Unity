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
        private static PLATEAUTileManager tileManageer;

        static PLATEAUSceneViewCameraTracker()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (EditorApplication.isPlaying)
                return;

            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            tileManageer.ClearTileAssets();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!Application.isEditor)
                return;

            if (EditorApplication.isPlaying)
                return;

            if (tileManageer == null)
                return;

            Camera sceneCamera = sceneView.camera;
            if (sceneCamera != null && tileManageer != null)
            {
                Vector3 currentPosition = sceneCamera.transform.position;
                if (currentPosition != tileManageer.LastCameraPosition)
                {
                    tileManageer.UpdateAssetsByCameraPosition(currentPosition);
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
            Debug.Log("Unity Editor Started");
            EditorApplication.update -= OnEditorUpdate; // 一度だけ実行

            InitView();
        }

        static void OnProjectChanged()
        {
            Debug.Log("Project Changed");

            InitView();
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;
            
            Debug.Log($"Scene Saving : {scene.name}, save path : {path}");

            // Addressablesのアンロード処理を実行
            tileManageer.ClearTileAssets();  
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("Play Mode about to start");
                tileManageer.ClearTileAssets();        
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Play Mode about to end");
                PLATEAURuntimeCameraTracker.StopCameraTracking();
                tileManageer.ClearTileAssets();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log($"Play Mode ended (Entered Edit Mode) {EditorApplication.isPlayingOrWillChangePlaymode}");
                InitView();
            }
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            Debug.Log($"Scene Opened: {scene.name}");          
            InitView();
        }

        static async void InitView()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Debug.Log("InitView");

            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            tileManageer.InitializeTiles();
            PLATEAUSceneViewCameraTracker.Initialize();
        }
    }
}