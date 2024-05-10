using PLATEAU.RoadNetwork.Data;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkTrack
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        //識別用ID：TrackID
        public RnID<RoadNetworkDataTrack> MyId { get; set; }

        //接続上流リンク識別用ID：LinkID
        public RoadNetworkLink NextLink { get; set; }

        //接続上流レーン識別用ID：LaneID
        public RoadNetworkLane NextLane { get; set; }

        //接続上流リンク下流端からの距離[m] : double
        //public double distanceFromNextLink;

        //接続下流リンク識別用ID : LinkID

        public RoadNetworkLink PrevLink { get; set; }

        //接続下流レーン識別用ID: LaneID

        public RoadNetworkLane PrevLane { get; set; }
        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}