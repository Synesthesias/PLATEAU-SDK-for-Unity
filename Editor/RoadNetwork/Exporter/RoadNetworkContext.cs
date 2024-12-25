using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークからの情報を提供するクラス
    /// </summary>
    public class RoadNetworkContext
    {
        public RoadNetworkDataGetter RoadNetworkGetter { get; private set; }

        public GeoReference GeoReference { get; private set; }

        public RoadNetworkContext()
        {
            var rnStructureModel = GameObject.FindObjectOfType<PLATEAURnStructureModel>();

            if (rnStructureModel)
            {
                RoadNetworkGetter = rnStructureModel.GetRoadNetworkDataGetter();
            }

            var cityModelInstance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>();

            if (cityModelInstance)
            {
                GeoReference = cityModelInstance.GeoReference;
            }
        }
    }
}