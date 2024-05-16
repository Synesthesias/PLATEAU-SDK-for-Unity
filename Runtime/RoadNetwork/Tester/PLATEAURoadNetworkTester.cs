using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork
{
    public class PLATEAURoadNetworkTester : MonoBehaviour
    {
        [Serializable]
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();

        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();

        [SerializeField] private bool targetAll = false;

        [SerializeField] private RoadNetworkDrawerDebug drawer = new RoadNetworkDrawerDebug();

        [SerializeField] public List<PLATEAUCityObjectGroup> geoTestTargets = new List<PLATEAUCityObjectGroup>();


        public string loadPresetName = "";

        [SerializeField] private bool showGeoTest = false;

        [field: SerializeField] private RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        [field: SerializeField] public RoadNetworkModel RoadNetwork { get; set; }

        [field: SerializeField] private RoadNetworkStorage Storage { get; set; }

        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);

            if (showGeoTest)
            {
                var vertices = geoTestTargets
                    .Select(x => x.GetComponent<MeshCollider>())
                    .Where(x => x)
                    .SelectMany(x => x.sharedMesh.vertices.Select(a => a.Xz()))
                    .ToList();
                var convex = GeoGraph2d.ComputeConvexVolume(vertices);
                DebugUtil.DrawArrows(convex.Select(x => x.Xay()));
            }
        }

        public void Draw(PLATEAUCityObjectGroup cityObjectGroup)
        {
            var collider = cityObjectGroup.GetComponent<MeshCollider>();
            var cMesh = collider.sharedMesh;
            var isClockwise = GeoGraph2d.IsClockwise(cMesh.vertices.Select(v => new Vector2(v.x, v.y)));
            if (isClockwise)
            {
                DebugUtil.DrawArrows(cMesh.vertices.Select(v => v + Vector3.up * 0.2f));
            }
            else
            {
                DebugUtil.DrawArrows(cMesh.vertices.Reverse().Select(v => v + Vector3.up * 0.2f));
            }
        }

        public void CreateNetwork()
        {
            if (targetAll)
            {
                var allTargets = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                    .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                    .ToList();

                RoadNetwork = Factory.CreateNetwork(allTargets);
            }
            else
            {
                // 重複は排除する
                targets = targets.Distinct().ToList();
                RoadNetwork = Factory.CreateNetwork(targets);
            }
        }

        public void Serialize()
        {
            if (RoadNetwork == null)
                return;
            var serializer = new RoadNetworkSerializer();
            Storage = serializer.Serialize(RoadNetwork);
        }

        public void Deserialize()
        {
            if (Storage == null)
                return;

            var serializer = new RoadNetworkSerializer();
            RoadNetwork = serializer.Deserialize(Storage);
        }
    }
}