using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnIntersection))]
    public class RnDataIntersection : RnDataRoadBase
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RnDataIntersection> MyId { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnIntersection.TargetTran))]
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 隣接情報
        [field: SerializeField]
        [RoadNetworkSerializeMember("neighbors")]
        public List<RnDataNeighbor> Neighbors { get; set; } = new List<RnDataNeighbor>();

        // 車線
        [field: SerializeField, RoadNetworkSerializeMember("lanes")]
        public List<RnID<RnDataLane>> Lanes { get; set; } = new List<RnID<RnDataLane>>();
    }
}