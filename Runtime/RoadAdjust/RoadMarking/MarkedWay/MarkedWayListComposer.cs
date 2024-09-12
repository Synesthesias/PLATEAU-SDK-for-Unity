using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークをもとに、車線を引く対象となる<see cref="MarkedWay"/>のリスト<see cref="MarkedWayList"/>を生成します。
    /// </summary>
    public class MarkedWayListComposer : IMarkedWayListComposer
    {
        


        /// <summary> 道路ネットワークから、車線を引く対象となる<see cref="MarkedWay"/>を収集します。 </summary>
        public MarkedWayList ComposeFrom(RnModel model)
        {
            // ここに、どの線を追加したいか記述します。
            var composers = new IMarkedWayListComposer[]
            {
                new MCLaneLine(), // 車線の間の線のうち、センターラインでないもの。
                new MCShoulderLine(), // 路側帯線、すなわち歩道と車道の間の線。
                new MCCenterLine(), // センターライン
                new MCIntersection() // 交差点の線
            };
            
            var ret = new MarkedWayList();
            foreach (var composer in composers)
            {
                ret.AddRange(composer.ComposeFrom(model));
            }
            return ret;
        }
    }

    internal interface IMarkedWayListComposer
    {
        public MarkedWayList ComposeFrom(RnModel model);
    }

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
                // 車道のうち、端でない（路側帯線でない）もののLeftLaneは車線境界線です。
                for (int i = 1; i < carLanes.Count - 1; i++)
                {
                    ret.Add(new MarkedWay(carLanes[i].LeftWay, MarkedWayType.LaneLine, carLanes[i].IsReverse));
                }
            }
            return ret;
        }
    }

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
                var lastLane = carLanes[carLanes.Count - 1];
                ret.Add(new MarkedWay(firstLane.LeftWay, MarkedWayType.ShoulderLine, firstLane.IsReverse));
                ret.Add(new MarkedWay(lastLane.LeftWay, MarkedWayType.ShoulderLine, lastLane.IsReverse));
            }
            return ret;
        }
    }

    /// <summary>
    /// 道路ネットワークから、センターラインを収集します。
    /// センターラインとは、車の進行方向が違う車線を区切る線です。
    /// MCはMarkedWayComposerの略です。
    /// </summary>
    internal class MCCenterLine : IMarkedWayListComposer
    {
        private const float WidthThreshold = 6f; // センターラインのタイプが変わるしきい値、道路の片側の幅
        public MarkedWayList ComposeFrom(RnModel model)
        {
            var ret = new MarkedWayList();
            foreach (var road in model.Roads)
            {
                var carLanes = road.MainLanes;
                
                // 片側の道路幅からタイプを判定します
                bool isOver6M =
                    carLanes.Where(l => l.IsReverse).Sum(l => l.CalcWidth()) > WidthThreshold ||
                    carLanes.Where(l => !l.IsReverse).Sum(l => l.CalcWidth()) > WidthThreshold;
                var type = isOver6M ? MarkedWayType.CenterLineOver6M : MarkedWayType.CenterLineUnder6M;
                
                for (int i = 0; i < carLanes.Count; i++)
                {
                    var lane = carLanes[i];
                    
                    // 次のレーンと進行方向が異なる場合、RightLaneはセンターラインです。
                    if (i < carLanes.Count - 1 && lane.IsReverse != carLanes[i + 1].IsReverse)
                    {
                        ret.Add(new MarkedWay(lane.RightWay, type, lane.IsReverse));
                    }
                }
            }
            return ret;
        }
    }

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