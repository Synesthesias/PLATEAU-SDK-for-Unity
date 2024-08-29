using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    public interface IEventBuffer
    {
        public Vector2 MousePosition { get; }
        public bool MouseDown { get; }
        public bool MouseUp { get; }
        public Vector3 CameraPosition { get; }
    }

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
                //Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //ray = new Ray(new Vector3(700.50f, 8.84f, -615.75f) + Vector3.up, Vector3.down);
                //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1500, Color.red, 2.0F);
                mousePosition = e.mousePosition;
                //Physics.Raycast(ray, out RaycastHit hit, 1000);
                //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                //Debug.DrawRay(hit.point, Vector3.up * 100, Color.magenta);
                Debug.Log("mouse move");
            }
            else
            {
                Debug.Log($"eve {e.type}");
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

            

            //if (e.type == EventType.MouseDown)
            //{
            //    // left click, right click, middle click
            //    if (e.button == 0 || e.button == 1 || e.button == 2)
            //    {
            //        ev = e;
            //        mousePosition = e.mousePosition;
            //        foreach (var item in clickEventReceivers)
            //        {
            //            item.ReserveExecute(this);
            //        }
            //    }
            //}
        }

    }
}
