using PLATEAU.Editor.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Tester;
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
                if (obj.Factory.midStageData?.convertedCityObjects?.cityObjects?.Any() ?? false)
                {
                    if (GUILayout.Button("Open SubDividedCityObject Editor"))
                        SubDividedCityObjectDebugEditorWindow.OpenWindow(new SubDividedCityObjectInstanceHelper(obj), true);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var model = obj.GetComponent<PLATEAURnStructureModel>();
                if (model && model.RoadNetwork != null)
                {
                    if (GUILayout.Button("Create Empty Road"))
                        model.RoadNetwork.CreateEmptyRoadBetweenInteraction();

                    if (GUILayout.Button("Remove Empty Road"))
                        model.RoadNetwork.RemoveEmptyRoadBetweenIntersection();
                }
                if (model && model.RoadNetwork != null)
                {
                    if (GUILayout.Button("Create Empty Intersection"))
                        model.RoadNetwork.CreateEmptyIntersectionBetweenRoad();

                    if (GUILayout.Button("Remove Empty Intersection"))
                        model.RoadNetwork.RemoveEmptyIntersectionBetweenRoad();
                }
            }
            if (GUILayout.Button("Check Lod"))
                obj.RemoveSameNameCityObjectGroup();
        }
    }
}