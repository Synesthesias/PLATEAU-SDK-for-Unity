using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
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

        //private static Vector3 lastPosition = Vector3.zero;
        //private static Quaternion lastRotation = Quaternion.identity;

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
                //Quaternion currentRotation = sceneCamera.transform.rotation;

                if (currentPosition != tileManageer.LastCameraPosition)
                {
                    //Debug.Log($"SceneView Camera Moved! Position: {currentPosition}, Rotation: {currentRotation}");
                    //lastPosition = currentPosition;
                    //lastRotation = currentRotation;

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

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("Play Mode about to start");
                tileManageer.ClearAll();
                
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("Play Mode ended (Entered Edit Mode)");
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

        static void InitView()
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            tileManageer.ClearAll();
            SceneViewCameraTracker.Initialize();
        }
    }

    /// <summary>
    /// PLATEAUTileManagerのEditor拡張クラス。
    /// TODO: 専用ファイルにする
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
            }
        }
    }
}