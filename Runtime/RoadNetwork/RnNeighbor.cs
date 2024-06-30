using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RnNeighbor : ARnParts<RnNeighbor>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 他レーンとの境界線
        public RnWay Border { get; set; }

        // Link
        public RnLink Link { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// この境界とつながっているレーン
        /// </summary>
        /// <returns></returns>
        public RnLane GetConnectedLane()
        {
            if (Border == null)
            {
                return null;
            }
            return Link.AllLanes.FirstOrDefault(lane => lane.AllBorders.Any(b => b.LineString == Border.LineString));
        }
    }
}