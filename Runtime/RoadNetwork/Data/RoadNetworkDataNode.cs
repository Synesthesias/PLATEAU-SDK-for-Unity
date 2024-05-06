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
        public RnId<RoadNetworkDataNode> MyId { get; set; }


        //// レーンを構成する道
        //// 左側が若いインデックスになる
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Ways))]

        public List<RnId<RoadNetworkDataWay>> Ways { get; set; } = new List<RnId<RoadNetworkDataWay>>();

        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Borders))]

        public List<RnId<RoadNetworkDataWay>> Borders { get; set; } = new List<RnId<RoadNetworkDataWay>>();

        // 車線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNode.Lanes))]

        public List<RnId<RoadNetworkDataLane>> Lanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();
    }
}