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
                if (!road.TryGetMergedSideWay(null, out var leftWay, out var rightWay)) continue;
                ret.Add(new MarkedWay(leftWay, MarkedWayType.ShoulderLine, true));
                ret.Add(new MarkedWay(rightWay, MarkedWayType.ShoulderLine, false));
            }
            return ret;
        }
    }
}