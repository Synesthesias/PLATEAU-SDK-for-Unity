using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// <see cref="RnWay"/>を拡張し、道路に車線の線を描くための情報を追加したクラスです。
    /// </summary>
    public class MarkedWay
    {
        public RnWay Way { get; private set; }
        public MarkedWayType Type { get; private set; }
        public bool Direction { get; private set; }

        public MarkedWay(RnWay way, MarkedWayType type, bool direction)
        {
            Way = way;
            Type = type;
            Direction = direction;
        }
    }

    /// <summary> 道路の線を描くにあたって見た目が異なるタイプのenumです。 </summary>
    public enum MarkedWayType
    {
        /// <summary> センターライン、すなわち、車の進行方向が違う車線を区切る線。 </summary>
        CenterLine,
        /// <summary> 車の進行方向が同じ車線を区切る線。すなわち、車線同士を区切る線のうち、センターラインでない線。 車線境界線。</summary>
        LaneLine,
        /// <summary> 車道と歩道の間の線。車道境界線。 </summary>
        RoadwayBorderLine
    }

    public static class MarkedWayTypeExtension
    {
        public static ILineMeshGenerator ToLineMeshGenerator(this MarkedWayType type, bool direction)
        {
            switch (type)
            {
                case MarkedWayType.CenterLine:
                    return new SolidLineMeshGenerator();
                case MarkedWayType.LaneLine:
                    return new DashedLineMeshGenerator(direction);
                case MarkedWayType.RoadwayBorderLine:
                    return new SolidLineMeshGenerator();
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// <see cref="MarkedWay"/>のリストです。
    /// </summary>
    public class MarkedWayList
    {
        private List<MarkedWay> ways;
        public IReadOnlyList<MarkedWay> Get => ways;

        public MarkedWayList(List<MarkedWay> ways)
        {
            this.ways = ways;
        }

        /// <summary> 道路ネットワークから、車線を引く対象となる<see cref="MarkedWay"/>を収集します。 </summary>
        public static MarkedWayList ComposeFrom(RnModel model)
        {
            var found = new List<MarkedWay>();
            foreach (var road in model.Roads)
            {
                var carLanes = road.MainLanes;
                for (int i = 0; i < carLanes.Count; i++)
                {
                    var lane = carLanes[i];
                    
                    // 次のレーンと進行方向が異なる場合、RightLaneはセンターラインです。
                    if (i < carLanes.Count - 1 && lane.IsReverse != carLanes[i + 1].IsReverse)
                    {
                        found.Add(new MarkedWay(lane.RightWay, MarkedWayType.CenterLine, lane.IsReverse));
                    }
                    // RightLaneを見る必要があるのはセンターラインだけで、それ以外はLeftLaneだけ見れば重複なく網羅できます。

                    // 端の車線の場合、そのLeftLaneは歩道と車道の間です
                    if (i == 0 || i == carLanes.Count - 1)
                    {
                        found.Add(new MarkedWay(lane.LeftWay, MarkedWayType.RoadwayBorderLine, lane.IsReverse));
                    }
                    else
                    {
                        // そうでない場合、センターラインではない車線同士の境界線です。
                        found.Add(new MarkedWay(lane.LeftWay, MarkedWayType.LaneLine, lane.IsReverse));
                    }

                    
                }
            }
            return new MarkedWayList(found);
        }
        
    }
}