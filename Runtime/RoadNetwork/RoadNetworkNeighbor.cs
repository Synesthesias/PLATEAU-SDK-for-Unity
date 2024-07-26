using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkNeighbor
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 他レーンとの境界線
        public RoadNetworkWay Border { get; set; }

        // Link
        public RoadNetworkLink Link { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}