using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnBlock))]
    public class RnDataBlock : IPrimitiveData
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnBlock.Parent))]
        public RnID<RnDataRoad> Parent { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnBlock.LaneType))]
        public int LaneType { get; set; }
    }
}