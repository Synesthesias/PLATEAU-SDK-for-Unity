using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnBlock))]
    public class RnDataBlock : IPrimitiveData
    {
        // 自分自身を表すId
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnBlock.MyId))]
        public RnID<RnDataBlock> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnBlock.ParentLink))]
        public RnID<RnDataRoad> ParentLink { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnBlock.LaneType))]
        public int LaneType { get; set; }
    }
}