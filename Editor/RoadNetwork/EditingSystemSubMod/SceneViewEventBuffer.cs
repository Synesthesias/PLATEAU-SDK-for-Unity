using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    [InitializeOnLoad]
    public class SceneViewEventBuffer
    {
        static SceneViewEventBuffer()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static SceneViewEventBuffer Instance { get; set; }
        private List<ClickEventReceiver> clickEventReceivers = new List<ClickEventReceiver>();
        private Vector2 mousePosition;
        public static IClickEventReceiver CreateReceiver()
        {
            var instance = CreateOrGet();
            var receiver = new ClickEventReceiver();
            instance.clickEventReceivers.Add(receiver);
            return receiver;
        }
        public interface IClickEventReceiver
        {
            public Vector2 MousePosition { get; }
        }

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
            if (e.type == EventType.MouseMove)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                //ray = new Ray(new Vector3(700.50f, 8.84f, -615.75f) + Vector3.up, Vector3.down);
                //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1500, Color.red, 2.0F);
                mousePosition = e.mousePosition;
                //Physics.Raycast(ray, out RaycastHit hit, 1000);
                //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                //Debug.DrawRay(hit.point, Vector3.up * 100, Color.magenta);
                Debug.Log("mouse move");
            }

            if (e.type == EventType.MouseMove || e.type == EventType.MouseDown || e.type == EventType.MouseUp)
            {
                foreach (var item in clickEventReceivers)
                {
                    item.ReserveExecute(this);
                }
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

        private static void RemoveReceiver(IClickEventReceiver receiver)
        {
            var instance = CreateOrGet();
            instance.clickEventReceivers.Remove(receiver as ClickEventReceiver);
        }

        public class ClickEventReceiver : IClickEventReceiver
        {
            public ClickEventReceiver()
            {
            }

            private SceneViewEventBuffer refDetecter;

            public Vector2 MousePosition => mousePosition;
            private Vector2 mousePosition;

            public void ReserveExecute(SceneViewEventBuffer refDetecter)
            {
                this.refDetecter = refDetecter;
                mousePosition = refDetecter.mousePosition;
            }

        }
    }
}
