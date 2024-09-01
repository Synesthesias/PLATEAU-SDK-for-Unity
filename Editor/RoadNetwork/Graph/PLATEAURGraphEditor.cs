using PLATEAU.Editor.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Graph.Drawer;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.RoadNetwork.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Graph
{
    [CustomEditor(typeof(PLATEAURGraph))]
    public class PLATEAURGraphEditor : UnityEditor.Editor
    {

        private class RGraphInstanceHelper : RGraphDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURGraph target;

            private PLATEAURGraphDrawerDebug drawer;

            public RGraphInstanceHelper(PLATEAURGraph target)
            {
                this.target = target;
            }

            public PLATEAURGraph GetGraph()
            {
                return target;
            }

            public PLATEAURGraph CreateGraph()
            {
                //target.CreateRGraph();
                //return target.RGraph;
                return null;
            }

            public HashSet<RFace> TargetFaces => Drawer.TargetFaces;
            public HashSet<REdge> TargetEdges => Drawer.TargetEdges;
            public HashSet<RVertex> TargetVertices => Drawer.TargetVertices;


            public bool IsTarget(RFace face)
            {
                return RnEx.IsEditorSceneSelected(face.CityObjectGroup);
            }

            //public void CreateRnModel()
            //{
            //    //target.CreateRoadNetworkByGraphAsync();
            //}

            public PLATEAURGraphDrawerDebug Drawer
            {
                get
                {
                    if (drawer)
                        return drawer;
                    return drawer = target.GetComponent<PLATEAURGraphDrawerDebug>();
                }
            }
        }


        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURGraph;
            if (!obj)
                return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Open RGraph Editor"))
                RGraphDebugEditorWindow.OpenWindow(new RGraphInstanceHelper(obj), true);
        }
    }
}