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
                    var nextBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Next, RnDir.Left));
                    AddStopLine(wayList, nextBorder);
                }

                if (road.Prev is RnIntersection)
                {
                    var prevBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Prev, RnDir.Right));
                    AddStopLine(wayList, prevBorder);
                }
            }

            return wayList;
        }


        private void AddStopLine(MarkedWayList wayList, MWLine border)
        {
            wayList.Add(new MarkedWay(border, MarkedWayType.StopLine, false));
        }
    }
}