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


        //// レーンを構成する道
        //// 左側が若いインデックスになる
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Ways))]

        public List<RnID<RoadNetworkDataWay>> Ways { get; set; } = new List<RnID<RoadNetworkDataWay>>();

        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Borders))]

        public List<RnID<RoadNetworkDataWay>> Borders { get; set; } = new List<RnID<RoadNetworkDataWay>>();

        // 車線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Lanes))]

        public List<RnID<RoadNetworkDataLane>> Lanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();
    }
}