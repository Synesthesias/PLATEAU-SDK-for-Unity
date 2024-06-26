using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkNeighbor : ARoadNetworkParts<RoadNetworkNeighbor>
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

        /// <summary>
        /// この境界とつながっているレーン
        /// </summary>
        /// <returns></returns>
        public RoadNetworkLane GetConnectedLane()
        {
            if (Border == null)
            {
                return null;
            }
            return Link.AllLanes.FirstOrDefault(lane => lane.AllBorders.Any(b => b.LineString == Border.LineString));
        }
    }
}