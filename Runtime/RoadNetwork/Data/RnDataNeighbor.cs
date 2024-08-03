using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnNeighbor))]
    public class RnDataNeighbor
    {
        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnNeighbor.Border))]
        public RnID<RnDataWay> Border { get; set; }

        // Link
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Link { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}