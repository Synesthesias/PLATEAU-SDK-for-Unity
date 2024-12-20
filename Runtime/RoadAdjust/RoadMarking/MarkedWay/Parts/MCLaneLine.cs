using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークから、LaneLineを収集します。
    /// LaneLineとは車線間の線のうち、同じ方向を通行する線を区切るもの（センターラインでないもの）を指します。
    /// MCはMarkedWayComposerの略です。
    /// </summary>
    internal class MCLaneLine : IMarkedWayListComposer
    {
        public MarkedWayList ComposeFrom(IRrTarget target)
        {
            var ret = new MarkedWayList();
            foreach (var road in target.Roads())
            {
                var carLanes = road.MainLanes;
                // 車道のうち、端でない（路側帯線でない）もののLeftWayは車線境界線です。
                for (int i = 1; i < carLanes.Count - 1; i++)
                {
                    ret.Add(new MarkedWay(new MWLine(carLanes[i].LeftWay), MarkedWayType.LaneLine, carLanes[i].IsReverse));
                }
            }
            return ret;
        }
    }
}