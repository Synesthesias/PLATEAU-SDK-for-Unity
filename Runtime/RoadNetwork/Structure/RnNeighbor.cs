using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnNeighbor : ARnParts<RnNeighbor>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // Roadとの境界線
        public RnWay Border { get; set; }

        // 隣接Road基本的にRoadだが、初期のPLATEAUモデルによってはIntersectionもあり得るため基底クラスで持っている
        public RnRoadBase Road { get; set; }

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
            if (Road == null)
                yield break;
            foreach (var lane in Road.AllLanes
                         .Where(lane => lane.AllBorders.Any(b => b.IsSameLine(Border)))
                    )
            {
                yield return lane;
            }
        }
    }
}