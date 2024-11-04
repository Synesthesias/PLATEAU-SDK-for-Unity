using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary> 道路ネットワークから、交差点の線を収集します。 </summary>
    internal class MCIntersection : IMarkedWayListComposer
    {
        /// <summary> 長すぎる交差点の線を無視するしきい値 </summary>
        private const float IntersectionLineIgnoreLength = 100f;
        
        public MarkedWayList ComposeFrom(RnModel model)
        {
            var found = new List<MarkedWay>();
            foreach (var inter in model.Intersections)
            {
                // 交差点の境界のうち、他の道路を横切らない箇所に歩道の線を引きます。
                foreach (var edge in inter.Edges.Where(e => !e.IsBorder))
                {
                    var border = edge.Border;
                    if (border.CalcLength() > IntersectionLineIgnoreLength) continue; // 経験上、大きすぎる交差点は誤判定の可能性が高いので除外します
                    found.Add(new MarkedWay(border, MarkedWayType.ShoulderLine, true /*方向は関係ない*/));
                }
            }
            return new MarkedWayList(found);
        }
    }
}