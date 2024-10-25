using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    public class StopLineComposer
    {
        public MarkedWayList ComposeFrom(RnModel network)
        {
            var wayList = new MarkedWayList();
            foreach (var road in network.Roads)
            {
                if (road.Next is RnIntersection)
                {
                    var nextBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Next));
                    wayList.Add(new MarkedWay(nextBorder, MarkedWayType.StopLine, false));
                }

                if (road.Prev is RnIntersection)
                {
                    var prevBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Prev));
                    wayList.Add(new MarkedWay(prevBorder, MarkedWayType.StopLine, false));
                }
            }

            return wayList;
        }
        

    }
}