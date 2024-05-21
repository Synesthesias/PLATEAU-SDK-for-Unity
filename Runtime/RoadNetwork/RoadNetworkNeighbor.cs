using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;

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