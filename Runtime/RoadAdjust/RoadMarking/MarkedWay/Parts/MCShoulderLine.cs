using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークから、路側帯線(ShoulderLine)を収集します。
    /// 路側帯線とは車道と歩道の間の線を指します。
    /// MCはMarkedWayComposerの略です。
    /// </summary>
    internal class MCShoulderLine : IMarkedWayListComposer
    {
        public MarkedWayList ComposeFrom(RnModel model)
        {
            var ret = new MarkedWayList();
            foreach (var road in model.Roads)
            {
                var carLanes = road.MainLanes;
                // 端の車線について、そのLeftLaneは歩道と車道の間です。
                var firstLane = carLanes[0];
                var lastLane = carLanes[^1];
                ret.Add(new MarkedWay(firstLane.LeftWay, MarkedWayType.ShoulderLine, firstLane.IsReverse));
                ret.Add(new MarkedWay(lastLane.LeftWay, MarkedWayType.ShoulderLine, lastLane.IsReverse));
            }
            return ret;
        }
    }
}