using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnNode))]
    public class RnDataNode : RnDataRoadBase
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RnDataNode> MyId { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnNode.TargetTran))]
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 隣接情報
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnNode.Neighbors))]
        public List<RnDataNeighbor> Neighbors { get; set; } = new List<RnDataNeighbor>();

        // 車線
        [field: SerializeField, RoadNetworkSerializeMember("lanes")]
        public List<RnID<RnDataLane>> Lanes { get; set; } = new List<RnID<RnDataLane>>();
    }
}