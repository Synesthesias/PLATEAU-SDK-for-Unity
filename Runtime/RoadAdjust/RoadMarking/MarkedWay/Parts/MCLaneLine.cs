using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークから、LaneLineを収集します。
    /// LaneLineとは車線間の線のうち、センターラインでないものを指します。
    /// MCはMarkedWayComposerの略です。
    /// </summary>
    internal class MCLaneLine : IMarkedWayListComposer
    {
        public MarkedWayList ComposeFrom(RnModel model)
        {
            var ret = new MarkedWayList();
            foreach (var road in model.Roads)
            {
                var carLanes = road.MainLanes;
                // 車道のうち、端でない（路側帯線でない）もののLeftWayは車線境界線です。
                for (int i = 1; i < carLanes.Count - 1; i++)
                {
                    ret.Add(new MarkedWay(carLanes[i].LeftWay, MarkedWayType.LaneLine, carLanes[i].IsReverse));
                }
            }
            return ret;
        }
    }
}