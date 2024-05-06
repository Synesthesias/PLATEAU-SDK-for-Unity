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
        public RnId<RoadNetworkDataLink> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.NextNode))]
        public RnId<RoadNetworkDataNode> NextNode { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.PrevNode))]
        public RnId<RoadNetworkDataNode> PrevNode { get; set; }

        // 本線レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.MainLanes))]
        public List<RnId<RoadNetworkDataLane>> MainLanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();

        // 右折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.RightLanes))]
        public List<RnId<RoadNetworkDataLane>> RightLanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();

        // 左折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLink.LeftLanes))]
        public List<RnId<RoadNetworkDataLane>> LeftLanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();
    }
}