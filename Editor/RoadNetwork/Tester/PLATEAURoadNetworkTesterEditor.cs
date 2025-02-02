using PLATEAU.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Linq;
using System.Threading.Tasks;
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

            if (GUILayout.Button("Check Lod"))
                obj.RemoveSameNameCityObjectGroup();

            var cityObjects = obj.GetComponent<PLATEAUSubDividedCityObjectGroup>();
            if (GUILayout.Button("Create RGraph"))
            {

                var graph = cityObjects.gameObject.GetOrAddComponent<PLATEAURGraph>();

                var subDividedCityObjects
                    = cityObjects
                        .CityObjects
                        .Where(x => x.CityObjectGroup != null)
                        .Select(x => (x, x.CityObjectGroup.transform.localToWorldMatrix)).ToList();

                var graphTask = Task.Run(() =>
                {
                    graph.Graph = obj.Factory.GraphFactory.CreateGraph(subDividedCityObjects);
                });
                graphTask.ContinueWithErrorCatch();
                //graph.Graph = obj.Factory.GraphFactory.CreateGraph(cityObjects.CityObjects);
            }

            if (GUILayout.Button("Create SubDivided City Object"))
            {
                cityObjects = obj.gameObject.GetOrAddComponent<PLATEAUSubDividedCityObjectGroup>();
                var subDividedRes =
                    SubDividedCityObjectFactory.ConvertCityObjects(obj.GetTargetCityObjects(), useContourMesh: obj.Factory.UseContourMesh);
                cityObjects.CityObjects = subDividedRes.ConvertedCityObjects;
            }
        }
    }
}