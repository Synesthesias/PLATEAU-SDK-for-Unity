using PLATEAU.DynamicTile;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CameraMovement
{
    [InitializeOnLoad]
    public class SceneViewCameraTracker
    {
        private static PLATEAUTileManager tileManageer;

        private static Vector3 lastPosition = Vector3.zero;
        private static Quaternion lastRotation = Quaternion.identity;

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

                Camera sceneCamera = sceneView.camera;
            if (sceneCamera != null && tileManageer != null)
            {
                Vector3 currentPosition = sceneCamera.transform.position;
                Quaternion currentRotation = sceneCamera.transform.rotation;

                if (currentPosition != lastPosition || currentRotation != lastRotation)
                {
                    //Debug.Log($"SceneView Camera Moved! Position: {currentPosition}, Rotation: {currentRotation}");
                    lastPosition = currentPosition;
                    lastRotation = currentRotation;

                    tileManageer.UpdateAssetByCameraPosition(currentPosition);
                }
            }
        }
    }

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
            Debug.Log("Unity Editor���N�����܂����I");
            EditorApplication.update -= OnEditorUpdate; // ��x�������s

            InitView();
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            Debug.Log($"�V�[���ۑ��O�̏���: {scene.name}, �ۑ���: {path}");
            // ������Addressables�̃A�����[�h���������s
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer != null)
            {
                tileManageer.ClearAll();
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("Play Mode �ɓ���O�̏��������s");

                var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
                if (tileManageer != null)
                {
                    tileManageer.ClearAll();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("Play Mode ����߂�����̏��������s");
                //SceneViewCameraTracker.Initialize();
                InitView();
            }
        }


        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            Debug.Log($"Scene Opened: {scene.name}");
            InitView();
        }

        static void InitView()
        {
            var tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer != null)
            {
                tileManageer.ClearAll();
                SceneViewCameraTracker.Initialize();

                Debug.Log($"Tile Manager Clear All Called.");
            }
        }
    }

    [CustomEditor(typeof(PLATEAUTileManager))]
    public class PLATEAUTileManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // �ʏ��Inspector�\��

            PLATEAUTileManager myComponent = (PLATEAUTileManager)target;

            if (GUILayout.Button("Clear All"))
            {
                myComponent.ClearAll();

                SceneViewCameraTracker.Initialize();
            }

            if (GUILayout.Button("Show Debug Extent"))
            {
                myComponent.ShowBounds();
            }
        }
    }
}
