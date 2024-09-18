using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnSideWalk))]
    public class RnDataSideWalk : IPrimitiveData
    {
        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember("parentRoad")]
        public RnID<RnDataRoadBase> ParentRoad { get; set; }


        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember("outsideWay")]
        public RnID<RnDataWay> OutsideWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("insideWay")]
        public RnID<RnDataWay> InsideWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("startEdgeWay")]
        public RnID<RnDataWay> StartEdgeWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("endEdgeWay")]
        public RnID<RnDataWay> EndEdgeWay { get; set; }
    }
}