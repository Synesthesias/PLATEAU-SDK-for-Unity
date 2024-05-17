using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkLink))]
    public class RoadNetworkDataLink : IPrimitiveData
    {
        // 自分自身を表すId
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.MyId))]
        public RnID<RoadNetworkDataLink> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.NextNode))]
        public RnID<RoadNetworkDataNode> NextNode { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.PrevNode))]
        public RnID<RoadNetworkDataNode> PrevNode { get; set; }

        // 本線レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.MainLanes))]
        public List<RnID<RoadNetworkDataLane>> MainLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 右折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.RightLanes))]
        public List<RnID<RoadNetworkDataLane>> RightLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 左折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.LeftLanes))]
        public List<RnID<RoadNetworkDataLane>> LeftLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();
    }
}