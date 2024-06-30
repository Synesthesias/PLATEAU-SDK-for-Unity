using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkNeighbor))]
    public class RoadNetworkDataNeighbor
    {
        // 他レーンとの境界線
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkNeighbor.Border))]
        public RnID<RoadNetworkDataWay> Border { get; set; }

        // Link
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Link { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}