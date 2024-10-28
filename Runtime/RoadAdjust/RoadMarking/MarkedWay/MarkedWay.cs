using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 路面標示の線1つを表現するクラスです。
    /// </summary>
    public class MarkedWay
    {
        public MWLine Line { get; private set; }
        public MarkedWayType Type { get; private set; }
        public bool IsReversed { get; private set; } // RnLane.IsReverseに相当します

        public MarkedWay(MWLine line, MarkedWayType type, bool isReversed)
        {
            Line = line;
            Type = type;
            IsReversed = isReversed;
        }


    }

    /// <summary>
    /// 路面標示の線の座標列です。
    /// MWはMarkedWayの略です。
    /// </summary>
    public class MWLine : IEnumerable<Vector3>
    {
        private Vector3[] points;

        public MWLine(IEnumerable<Vector3> points)
        {
            this.points = points.ToArray();
        }

        public IEnumerable<Vector3> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value.ToArray();
            }
        }

        public Vector3 this[int index]
        {
            get => points[index];
        }

        public int Count => points.Length;

        public float SumDistance()
        {
            if (points.Length == 0) return 0;
            var p = points[0];
            float len = 0;
            for (int i = 1; i < points.Length; i++)
            {
                len += Vector3.Distance(points[i], p);
            }

            return len;
        }
        
        public IEnumerator<Vector3> GetEnumerator()
        {
            foreach (var p in points) yield return p;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        /// <summary> 停止線 </summary>
        StopLine,
        None
    }

    internal static class MarkedWayTypeExtension
    {
        /// <summary> 法令で定められた、車線標示の線の太さです。 </summary>
        private const float CarLaneLineWidth = 0.15f;

        /// <summary> 法令で定められた、停止線の線の太さです。 </summary>
        private const float StopLineWidth = 0.45f;
        
        public static ILineMeshGenerator ToLineMeshGenerator(this MarkedWayType type, bool direction)
        {
            switch (type)
            {
                case MarkedWayType.CenterLineOver6MWidth:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.White, CarLaneLineWidth);
                case MarkedWayType.CenterLineUnder6MWidth:
                    return new DashedLineMeshGenerator(RoadMarkingMaterial.White, direction, CarLaneLineWidth);
                case MarkedWayType.CenterLineNearIntersection:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.Yellow, CarLaneLineWidth);
                case MarkedWayType.LaneLine:
                    return new DashedLineMeshGenerator(RoadMarkingMaterial.White, direction, CarLaneLineWidth);
                case MarkedWayType.ShoulderLine:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.White, CarLaneLineWidth);
                case MarkedWayType.StopLine:
                    return new SolidLineMeshGenerator(RoadMarkingMaterial.White, StopLineWidth);
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
            if (way == null || way.Line == null)
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