using UnityEngine;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;

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
        
        public bool IsInitSucceed { get; }

        /// <summary>
        /// コンストラクタ
        /// 都市モデルと道路ネットワークから必要なデータを取得します。
        /// </summary>
        public RoadNetworkContext(PLATEAURnStructureModel rnStructureModel)
        {
            RoadNetworkGetter = rnStructureModel.GetRoadNetworkDataGetter();

            var cityModelInstance = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>();

            if (cityModelInstance == null)
            {
                Dialogue.Display("3D都市モデルの道路がシーン中に見つかりませんでした。", "OK");
                return;
            }

            GeoReference = cityModelInstance.GeoReference;
            IsInitSucceed = true;
        }
    }
}