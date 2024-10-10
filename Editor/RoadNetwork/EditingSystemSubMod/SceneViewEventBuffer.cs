using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// バッファの値を返すインターフェース
    /// </summary>
    public interface IEventBuffer
    {
        public Vector2 MousePosition { get; }
        public bool MouseDown { get; }
        public bool MouseUp { get; }
        public Vector3 CameraPosition { get; }
    }

    /// <summary>
    /// OnSceneGUIのイベントをバッファリングするクラス
    /// 実際の処理を行っているのが別の更新フローでこのイベントを取得できないため、このクラスを作成。
    /// ベストは実際の処理をこのイベントに合わせた方がいいが対応コストがかかるため現状はこのまま対応。
    /// </summary>
    [InitializeOnLoad]
    public class SceneViewEventBuffer : IEventBuffer
    {
        static SceneViewEventBuffer()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static SceneViewEventBuffer Instance { get; set; }
        private Vector2 mousePosition;
        private bool mouseDown;
        private bool mouseUp;
        private Vector3 cameraPosition;


        private EventType preEvent;

        public static IEventBuffer GetBuffer()
        {
            var instance = CreateOrGet();
            return instance;
        }

        public Vector2 MousePosition { get => mousePosition; }
        public bool MouseDown { get => mouseDown; }

        public bool MouseUp { get => mouseUp; }

        public Vector3 CameraPosition { get => cameraPosition; }

        private static SceneViewEventBuffer CreateOrGet()
        {
            if (Instance == null)
            {
                Instance = new SceneViewEventBuffer();
            }

            return Instance;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var instance = CreateOrGet();
            instance.OnSceneGUIOnInstance(sceneView);
        }

        private void OnSceneGUIOnInstance(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                cameraPosition = sceneView.camera.transform.position;
            }

            // 一部のイベントは連続で取得できないので、前回のイベントを参照して更新する
            bool mousePositionUpdate = e.type == EventType.Used && 
                (preEvent == EventType.MouseMove ||
                preEvent == EventType.MouseDown || preEvent == EventType.MouseUp || preEvent == EventType.MouseDrag ||
                preEvent == EventType.DragPerform || preEvent == EventType.DragExited || preEvent == EventType.DragUpdated);
            preEvent = e.type;
            if (e.type == EventType.MouseMove ||
                e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.MouseDrag ||
                e.type == EventType.DragPerform || e.type == EventType.DragExited || e.type == EventType.DragUpdated ||
                mousePositionUpdate
                ) 
            {
                mousePosition = e.mousePosition;
            }
            else
            {
                //Debug.Log($"eve {e.type}");
            }

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    mouseDown = true;
                    mouseUp = false;
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    mouseDown = false;
                    mouseUp = true;
                }
            }

            if (e.type == EventType.MouseMove || e.type == EventType.MouseDown || e.type == EventType.MouseUp)
            {
            }
            else
            {
                return;
            }
        }

    }
}
