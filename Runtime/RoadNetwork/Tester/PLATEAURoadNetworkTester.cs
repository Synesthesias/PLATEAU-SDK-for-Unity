using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class PLATEAURoadNetworkTester : MonoBehaviour, IRoadNetworkObject
    {
        [SerializeField] private RoadNetworkDrawerDebug drawer = new RoadNetworkDrawerDebug();

        [field: SerializeField] private RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        [field: SerializeField] public RoadNetworkModel RoadNetwork { get; set; }

        [Serializable]
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();

        [SerializeField] private bool targetAll = false;

        [Serializable]
        public class SplitCityObjectTestParam
        {
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
            public bool doDestroySrcObject = false;
        }
        public SplitCityObjectTestParam splitCityObjectTestParam = new SplitCityObjectTestParam();

        public string targetPresetName = "";

        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);
        }

        public async Task SplitCityObjectAsync()
        {
            var p = splitCityObjectTestParam;
            // 分割結合の設定です。
            // https://project-plateau.github.io/PLATEAU-SDK-for-Unity/manual/runtimeAPI.html
            var conf = new GranularityConvertOptionUnity(new GranularityConvertOption(ConvertGranularity.PerAtomicFeatureObject, 1),
                p.targets.Select(t => t.gameObject).ToArray(), p.doDestroySrcObject);
            var d = await new CityGranularityConverter().ConvertAsync(conf);
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
                var targets = savedTargets.FirstOrDefault(s => s.name == targetPresetName);
                if (targets != null)
                {
                    targets.targets = targets.targets.Distinct().ToList();
                    RoadNetwork = Factory.CreateNetwork(targets.targets);
                }
            }
        }

    }
}