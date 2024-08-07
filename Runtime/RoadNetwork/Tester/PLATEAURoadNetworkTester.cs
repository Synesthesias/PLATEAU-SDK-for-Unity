using PLATEAU.CityGML;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using PLATEAUCityObjectGroup = PLATEAU.CityInfo.PLATEAUCityObjectGroup;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class PLATEAURoadNetworkTester : MonoBehaviour, IRoadNetworkObject
    {

        // --------------------
        // start:フィールド
        // --------------------
        [field: SerializeField]
        public RnModelDrawerDebug Drawer { get; set; } = new RnModelDrawerDebug();

        [field: SerializeField] private RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        public RnModel RoadNetwork { get; set; }

        // シリアライズ用フィールド
        [SerializeField]
        private RoadNetworkStorage storage;

        public RoadNetworkStorage Storage { get => storage; set => storage = value; }

        [Serializable]
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();
        [SerializeField] private bool targetAll = false;
        public string targetPresetName = "";

        [Serializable]
        public enum CreateMode
        {
            ConvertCityObject,
            MergeConvertCityObject,
            SeparateConvertCityObject,
            TranMesh,
            RoadNetwork,
            All
        }
        [SerializeField]
        private CreateMode createMode = CreateMode.ConvertCityObject;

        // --------------------
        // end:フィールド
        // --------------------

        public List<SubDividedCityObject> SubDividedCityObjects => Factory.midStageData.convertedCityObjects.cityObjects;

        public RGraph RGraph => Factory.midStageData.Graph;
        public RGraphDrawerDebug RGraphDrawer => Factory.midStageData.rGraph.drawer;

        public void OnDrawGizmos()
        {
            Drawer?.Draw(RoadNetwork);
        }

        private List<PLATEAUCityObjectGroup> GetTargetCityObjects()
        {
            var ret = targetAll
                ? (IList<PLATEAUCityObjectGroup>)GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                : savedTargets
                    .FirstOrDefault(s => s.name == targetPresetName)
                    ?.targets;
            if (ret == null)
                return new List<PLATEAUCityObjectGroup>();

            return ret
                .Where(c => c.transform.childCount == 0)
                .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                .Distinct()
                .ToList();
        }


        public async Task CreateNetwork()
        {
            try
            {
                RoadNetwork = await Factory.CreateRnModelAsync(GetTargetCityObjects());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void CreateRGraph()
        {
            Factory.midStageData.CreateGraph();
        }

        public async Task CreateRoadNetworkByGraphAsync()
        {
            RoadNetwork = await Factory.CreateRnModelAsync(Factory.midStageData.Graph);
        }

        public void Serialize()
        {
            if (RoadNetwork == null)
                return;
            storage = RoadNetwork.Serialize();
        }

        public void Deserialize()
        {
            RoadNetwork ??= new RnModel();
            RoadNetwork.Deserialize(storage);
        }

        /// <summary>
        /// 同名のCityObjectGroupがあった場合に最大のLODのもの以外を非表示にする
        /// </summary>
        public void RemoveSameNameCityObjectGroup()
        {
            var groups = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>();

            foreach (var g in groups.GroupBy(g => g.gameObject.name)
                         .Where(g => g.Count() > 1))
            {
                var level = g.Select(a => a.GetLodLevel()).Max();
                g.TryFindMax(a => a.GetLodLevel(), out var maxG);

                foreach (var a in g)
                {
                    if (a != maxG)
                    {
                        a.gameObject.SetActive(false);
                    }
                }
            }
        }

        public RoadNetworkDataGetter GetRoadNetworkDataGetter()
        {
            return new RoadNetworkDataGetter(storage);
        }
    }
}