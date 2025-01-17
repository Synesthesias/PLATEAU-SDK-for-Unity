using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    public class StopLineComposer
    {
        private const float HeightOffset = 0.07f; // 経験的にこのくらいの高さなら道路にめりこまないという値
        
        public MarkedWayList ComposeFrom(IRrTarget target)
        {
            var wayList = new MarkedWayList();
            foreach (var road in target.Roads())
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

            wayList.Translate(Vector3.up * HeightOffset);
            return wayList;
        }


        private void AddStopLine(MarkedWayList wayList, MWLine border)
        {
            wayList.Add(new MarkedWay(border, MarkedWayType.StopLine, false));
        }
    }
}