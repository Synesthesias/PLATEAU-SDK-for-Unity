using System.Collections.Generic;

namespace PLATEAU.RoadNetwork
{
    public class RnRoadBase : ARnParts<RnNode>
    {
        public virtual IEnumerable<RnBorder> GetNeighbors() { yield break; }

        public virtual IEnumerable<RnWay> GetBorderWays() { yield break; }

        // 
        public virtual IEnumerable<RnLane> AllLanes { get { yield break; } }
    }
}