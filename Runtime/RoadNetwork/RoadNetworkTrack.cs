using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // 親ノード
        public RoadNetworkNode ParentNode { get; set; }

        //接続上流レーン識別用ID：LaneID
        public RoadNetworkLane NextLane { get; set; }

        //接続上流リンク下流端からの距離[m] : double
        //public double distanceFromNextLink;

        //接続下流レーン識別用ID: LaneID

        public RoadNetworkLane PrevLane { get; set; }

        // 車線(左)
        public RoadNetworkWay LeftWay { get; private set; }

        // 車線(右)
        public RoadNetworkWay RightWay { get; private set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        //接続上流リンク識別用
        public RoadNetworkLink NextLink => NextLane?.ParentLink;

        //接続下流リンク識別用ID : LinkID
        public RoadNetworkLink PrevLink => PrevLane?.ParentLink;

        public RoadNetworkTrack() { }

        public RoadNetworkTrack(RoadNetworkWay leftWay, RoadNetworkWay rightWay)
        {
            LeftWay = leftWay;
            RightWay = rightWay;
        }
        // 左右両方のWayを返す
        public IEnumerable<RoadNetworkWay> BothWays
        {
            get
            {
                return Enumerable.Repeat(LeftWay, 1).Concat(Enumerable.Repeat(RightWay, 1)).Where(w => w != null);
            }
        }
    }
}