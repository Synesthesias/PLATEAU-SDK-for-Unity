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
    [Serializable]
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

        public string loadPresetName = "";

        [field: SerializeField] private RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        [field: SerializeField] public RoadNetworkModel RoadNetwork { get; set; }

        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);
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
                // èdï°ÇÕîrèúÇ∑ÇÈ
                targets = targets.Distinct().ToList();
                RoadNetwork = Factory.CreateNetwork(targets);
            }
        }

    }
}