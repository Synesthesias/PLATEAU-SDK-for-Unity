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
    public class SceneViewCameraTracker
    {
        private static PLATEAUTileManager tileManageer;

        static SceneViewCameraTracker()
        {
            Initialize();
        }

        public static void Initialize()
        {
            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            if(SceneView.currentDrawingSceneView != null)
                OnSceneGUI(SceneView.currentDrawingSceneView);
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
                    tileManageer.UpdateAssetByCameraPosition(currentPosition);
                }
            }
        }
    }

    /// <summary>
    /// Unity Editorの起動時やシーンオープン時にPLATEAUTileManagerを初期化や、SceneViewCameraTrackerの初期化を行うするクラス。
    /// </summary>
    [InitializeOnLoad]
    public class SceneOpenListener : UnityEditor.AssetModificationProcessor
    {
        static SceneOpenListener()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorSceneManager.sceneSaving += OnSceneSaving;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            InitView();
        }

        static void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying)
                return;

            Debug.Log("Unity Editor Started");
            EditorApplication.update -= OnEditorUpdate; // 一度だけ実行


            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            InitView();
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;
            
            Debug.Log($"Scene Saving : {scene.name}, save path : {path}");

            // Addressablesのアンロード処理を実行
            tileManageer.ClearAll();            
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            var task = tileManageer.ReinitializeFromCatalog();

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("Play Mode about to start");
                tileManageer.ClearAll();        
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("Play Mode ended (Entered Edit Mode)");
                tileManageer.ClearAll();
                RuntimeCameraTracker.StopCameraTracking();
                SceneViewCameraTracker.Initialize();
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

        static void InitView()
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            tileManageer.ClearAll();
            var task = tileManageer.ReinitializeFromCatalog();
            SceneViewCameraTracker.Initialize();
        }
    }
}