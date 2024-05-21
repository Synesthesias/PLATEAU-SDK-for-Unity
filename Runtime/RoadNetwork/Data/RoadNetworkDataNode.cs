using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkNode))]
    public class RoadNetworkDataNode : IPrimitiveData
    {
        // 自分自身を表すId
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.MyId))]
        public RnID<RoadNetworkDataNode> MyId { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.TargetTran))]
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Borders))]
        public List<RnID<RoadNetworkDataWay>> Borders { get; set; } = new List<RnID<RoadNetworkDataWay>>();

        // 隣接情報
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Neighbors))]
        public List<RoadNetworkDataNeighbor> Neighbors { get; set; } = new List<RoadNetworkDataNeighbor>();

        // 車線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Lanes))]
        public List<RnID<RoadNetworkDataLane>> Lanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();
    }
}