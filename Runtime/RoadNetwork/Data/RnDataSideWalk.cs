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
        public RnWay OutsideWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("insideWay")]
        public RnWay InsideWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("startEdgeWay")]
        public RnWay StartEdgeWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember("endEdgeWay")]
        public RnWay EndEdgeWay { get; set; }
    }
}