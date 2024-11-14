using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// <see cref="RnWay"/>を拡張し、道路に車線の線を描くための情報を追加したクラスです。
    /// </summary>
    public class MarkedWay
    {
        public RnWay Way { get; private set; }
        public MarkedWayType Type { get; private set; }
        public bool IsReversed { get; private set; } // RnLane.IsReverseに相当します

        public MarkedWay(RnWay way, MarkedWayType type, bool isReversed)
        {
            Way = way;
            Type = type;
            IsReversed = isReversed;
        }
    }

    /// <summary> 道路の線を描くにあたって見た目が異なるタイプのenumです。 </summary>
    public enum MarkedWayType
    {
        /// <summary> センターライン、すなわち、車の進行方向が違う車線を区切る線。6メートル以上か以下かで線のタイプが異なる。 </summary>
        CenterLineOver6MWidth,
        CenterLineUnder6MWidth,
        CenterLineNearIntersection,
        /// <summary> 車の進行方向が同じ車線を区切る線。すなわち、車線同士を区切る線のうち、センターラインでない線。 車線境界線。</summary>
        LaneLine,
        /// <summary> 車道と歩道の間の線。路側帯線。 </summary>
        ShoulderLine,
        None
    }

    internal static class MarkedWayTypeExtension
    {
        public static ILineMeshGenerator ToLineMeshGenerator(this MarkedWayType type, bool direction)
        {
            switch (type)
            {
                case MarkedWayType.CenterLineOver6MWidth:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.White);
                case MarkedWayType.CenterLineUnder6MWidth:
                    return new DashedLineMeshGenerator(RoadMarkingMaterial.White, direction);
                case MarkedWayType.CenterLineNearIntersection:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.Yellow);
                case MarkedWayType.LaneLine:
                    return new DashedLineMeshGenerator(RoadMarkingMaterial.White, direction);
                case MarkedWayType.ShoulderLine:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.White);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
    
    /// <summary>
    /// <see cref="MarkedWay"/>のリストです。
    /// </summary>
    public class MarkedWayList
    {
        private List<MarkedWay> ways;
        public IReadOnlyList<MarkedWay> MarkedWays => ways;

        public MarkedWayList(List<MarkedWay> ways)
        {
            this.ways = ways;
        }

        public MarkedWayList()
        {
            this.ways = new List<MarkedWay>();
        }

        public void Add(MarkedWay way)
        {
            if (way == null || way.Way == null || way.Way.Points == null)
            {
                Debug.LogWarning("way is null.");
                return;
            }
            ways.Add(way);
        }

        public void AddRange(MarkedWayList wayList)
        {
            if (wayList == null)
            {
                Debug.LogWarning("wayList is null.");
                return;
            }

            foreach (var way in wayList.ways)
            {
                Add(way);
            }
        }
    }
}