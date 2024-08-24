using PLATEAU.Editor.RoadNetwork.CityObject;
using PLATEAU.Editor.RoadNetwork.Graph;
using PLATEAU.Editor.RoadNetwork.Structure;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAURoadNetworkTester))]
    public class PLATEAURoadNetworkTesterEditor : UnityEditor.Editor
    {
        private class RGraphInstanceHelper : RGraphDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURoadNetworkTester target;

            public RGraphInstanceHelper(PLATEAURoadNetworkTester target)
            {
                this.target = target;
            }

            public RGraph GetGraph()
            {
                return target.RGraph;
            }

            public RGraph CreateGraph()
            {
                target.CreateRGraph();
                return target.RGraph;
            }

            public HashSet<RFace> TargetFaces => target.RGraphDrawer.TargetFaces;
            public HashSet<REdge> TargetEdges => target.RGraphDrawer.TargetEdges;
            public HashSet<RVertex> TargetVertices => target.RGraphDrawer.TargetVertices;

            public void CreateTranMesh()
            {
                target.Factory.midStageData.CreateAll(target.GetTargetCityObjects());
            }

            public bool IsTarget(RFace face)
            {
                return RnEx.IsEditorSceneSelected(face.CityObjectGroup);
            }

            public void CreateRnModel()
            {
                target.CreateRoadNetworkByGraphAsync();
            }

            public RGraphDrawerDebug GetDrawer()
            {
                return target.RGraphDrawer;
            }
        }

        private class SubDividedCityObjectInstanceHelper : SubDividedCityObjectDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURoadNetworkTester target;

            public SubDividedCityObjectInstanceHelper(PLATEAURoadNetworkTester target)
            {
                this.target = target;
            }

            public RsFactoryMidStageData.SubDividedCityObjectWrap GetCityObjects()
            {
                return target.Factory.midStageData.convertedCityObjects;
            }

            public bool IsTarget(SubDividedCityObject cityObject)
            {
                return RnEx.IsEditorSceneSelected(cityObject.CityObjectGroup);
            }

            public HashSet<SubDividedCityObject> TargetCityObjects =>
                target.Factory.midStageData.convertedCityObjects.drawer.TargetCityObjects;

            public void CreateRnModel()
            {
                target.CreateRoadNetworkByGraphAsync();
            }

            public RGraphDrawerDebug GetDrawer()
            {
                return target.RGraphDrawer;
            }

            public void CreateTranMesh()
            {
                target.Factory.midStageData.CreateAll(target.GetTargetCityObjects());
            }
        }

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURoadNetworkTester;
            if (!obj)
                return;

            base.OnInspectorGUI();
            GUILayout.Label($"ConvertedCityObjectVertexCount : {obj.SubDividedCityObjects.Sum(c => c.Meshes.Sum(m => m.Vertices.Count))}");

            if (GUILayout.Button("Create"))
                obj.CreateNetwork();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (obj.RGraph != null && GUILayout.Button("Open RGraph Editor"))
                    RGraphDebugEditorWindow.OpenWindow(new RGraphInstanceHelper(obj), true);


                if (obj.Factory.midStageData?.convertedCityObjects?.cityObjects?.Any() ?? false)
                {
                    if (GUILayout.Button("Open SubDividedCityObject Editor"))
                        SubDividedCityObjectDebugEditorWindow.OpenWindow(new SubDividedCityObjectInstanceHelper(obj), true);
                }
            }

            if (GUILayout.Button("Check Lod"))
                obj.RemoveSameNameCityObjectGroup();
        }
    }
}