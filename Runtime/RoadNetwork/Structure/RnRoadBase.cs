using System;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnRoadBase : ARnParts<RnIntersection>
    {
        // 境界線情報を取得
        public virtual IEnumerable<RnBorder> GetBorders() { yield break; }

        // 隣接するRoadを取得
        public virtual IEnumerable<RnRoadBase> GetNeighborRoads() { yield break; }

        // 所持全レーンを取得
        public virtual IEnumerable<RnLane> AllLanes { get { yield break; } }
    }
}