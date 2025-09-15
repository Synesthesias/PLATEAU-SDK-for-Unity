using PLATEAU.Util.Async;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Editor Mode 時のSceneViewカメラの位置を監視し、PLATEAUTileManagerに通知するクラス。
    /// </summary>
    [InitializeOnLoad]
    public class PLATEAUSceneViewCameraTracker
    {
        private static PLATEAUTileManager tileManager;

        public static bool IsRunning { get; private set; }

        static PLATEAUSceneViewCameraTracker()
        {
            Initialize();
        }

        public static void Release()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            tileManager = null;

            IsRunning = false;
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

            IsRunning = true;
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
                if (tileManager.CheckIfCameraPositionHasChanged(currentPosition))
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
    public class PLATEAUEditorEventListener : AssetModificationProcessor
    {
        // EditorのEvent発行時にデバッグログを表示するかどうかのフラグ
        public const bool ShowDebugLog = false;

        public static volatile bool disableProjectChangeEvent = false;

        public static bool IsRunning { get; private set; }

        static PLATEAUEditorEventListener()
        {
            Initialize();
        }

        public static void Initialize()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            // コード更新前にリセットすることでドメインリロード後も動作するようにします
            CompilationPipeline.compilationStarted -= BeforeScriptCompile;
            CompilationPipeline.compilationStarted += BeforeScriptCompile;

            InitView().ContinueWithErrorCatch();

            IsRunning = true;
        }

        public static void Release()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            PLATEAUSceneViewCameraTracker.Release();

            IsRunning = false;
        }

        static void OnEditorUpdate()
        {
            Log("Unity Editor Started");
            EditorApplication.update -= OnEditorUpdate; // 一度だけ実行

            if (BuildPipeline.isBuildingPlayer) return;
            InitView().ContinueWithErrorCatch();
        }
        
        static void OnProjectChanged()
        {
            if (disableProjectChangeEvent) 
                return;

            Log("Project Changed");

            if (BuildPipeline.isBuildingPlayer) return;
            InitView().ContinueWithErrorCatch();
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
                tileManager.DestroyLoadTask();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Log("Play Mode about to end");
                tileManager.ClearTileAssets();
                PLATEAURuntimeCameraTracker.StopCameraTracking();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Log($"Play Mode ended (Entered Edit Mode) {EditorApplication.isPlayingOrWillChangePlaymode}");
                InitView().ContinueWithErrorCatch();
            }
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            var tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null)
                return;

            if (BuildPipeline.isBuildingPlayer) return;
            Log($"Scene Opened: {scene.name}");
            InitView().ContinueWithErrorCatch();
        }
        
        private static void BeforeScriptCompile(object _) 
        {
            Log("BeforeScriptCompile"); 
            if (BuildPipeline.isBuildingPlayer) return;
            var tileManager = Object.FindObjectOfType<PLATEAUTileManager>();
            if (tileManager == null) return;
            tileManager.ClearTileAssets();
            ReinitializeAddressables();
        }

        private static void ReinitializeAddressables()
        {
            try
            {
                var handle = Addressables.InitializeAsync();
                handle.WaitForCompletion();
                if (handle.IsValid()) Addressables.Release(handle);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Addressables init failed: " + e);
            }
            
            // Addressables.ClearResourceLocators();
            // AssetBundle.UnloadAllAssetBundles(true); 
            // Resources.UnloadUnusedAssets();
        }

        private static async Task InitView()
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