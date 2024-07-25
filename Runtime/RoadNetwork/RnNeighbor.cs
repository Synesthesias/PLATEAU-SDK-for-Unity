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
            return GetConnectedLanes().FirstOrDefault();
        }

        /// <summary>
        /// この境界とつながっているレーンリスト
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetConnectedLanes()
        {
            if (Border == null)
                yield break;
            if (Link == null)
                yield break;
            foreach (var lane in Link.AllLanes
                         .Where(lane => lane.AllBorders.Any(b => b.IsSameLine(Border)))
                    )
            {
                yield return lane;
            }
        }
    }
}