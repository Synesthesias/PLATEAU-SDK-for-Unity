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
        [RoadNetworkSerializeMember("line")]
        public RnID<RnDataLineString> Line { get; set; }
    }
}