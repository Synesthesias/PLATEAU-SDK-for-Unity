using PLATEAU.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Tester
{
    [CustomEditor(typeof(PLATEAURoadNetworkTester))]
    public class PLATEAURoadNetworkTesterEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURoadNetworkTester;
            if (!obj)
                return;


            base.OnInspectorGUI();
            if (GUILayout.Button("Create"))
                obj.CreateNetwork().ContinueWithErrorCatch();

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

            var cityObjects = obj.GetComponent<PLATEAUSubDividedCityObjectGroup>();
            if (GUILayout.Button("Create RGraph"))
            {
                var graph = cityObjects.gameObject.GetOrAddComponent<PLATEAURGraph>();
                graph.Graph = obj.Factory.GraphFactory.CreateGraph(cityObjects.CityObjects);
            }
        }
    }
}