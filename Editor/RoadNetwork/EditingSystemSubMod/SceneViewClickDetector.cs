using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    [InitializeOnLoad]
    public class SceneViewClickDetector
    {
        static SceneViewClickDetector()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static SceneViewClickDetector Instance { get; set; }
        private List<ClickEventReceiver> clickEventReceivers = new List<ClickEventReceiver>();
        private Event ev;
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
            public void Execute(System.Action<Event, Vector2> onClick);
            public Vector2 MousePosition { get; }
        }

        private static SceneViewClickDetector CreateOrGet()
        {
            if (Instance == null)
            {
                Instance = new SceneViewClickDetector();
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
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDown || e.type == EventType.MouseUp)
            {
            }
            else
            {
                return;
            }
            ev = e;
            mousePosition = e.mousePosition;
            foreach (var item in clickEventReceivers)
            {
                item.ReserveExecute(this);
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

            private SceneViewClickDetector refDetecter;

            public Vector2 MousePosition => refDetecter.mousePosition;

            public void ReserveExecute(SceneViewClickDetector refDetecter)
            {
                this.refDetecter = refDetecter;
            }

            public void Execute(System.Action<Event, Vector2> onClick)
            {
                if (refDetecter != null)
                {
                    onClick?.Invoke(refDetecter.ev, refDetecter.mousePosition);
                    //refDetecter = null;   // 仮コメントアウト　mousepositionを取得するために残す
                }
            }
        }
    }
}
