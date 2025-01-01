using UnityEngine;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークからの情報を提供するクラス
    /// </summary>
    public class RoadNetworkContext
    {
        /// <summary>
        /// 道路ネットワークデータを取得するゲッタ
        /// </summary>
        public RoadNetworkDataGetter RoadNetworkGetter { get; private set; }

        /// <summary>
        /// ジオリファレンス
        /// </summary>
        public GeoReference GeoReference { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// 都市モデルと道路ネットワークから必要なデータを取得します。
        /// </summary>
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