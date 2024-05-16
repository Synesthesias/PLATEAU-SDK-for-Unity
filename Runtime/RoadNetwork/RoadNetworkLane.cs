using PlasticGui;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLane
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RoadNetworkDataLane> MyId { get; set; }

        // 親リンク
        public RoadNetworkLink ParentLink { get; set; }

        // 連結しているレーン(上流)
        public List<RoadNetworkLane> NextLanes { get; private set; } = new List<RoadNetworkLane>();

        // 連結しているレーン(下流)
        public List<RoadNetworkLane> PrevLanes { get; private set; } = new List<RoadNetworkLane>();

        // 境界線(下流)
        public RoadNetworkWay PrevBorder { get; set; }

        // 境界線(上流)
        public RoadNetworkWay NextBorder { get; set; }

        // 車線(左)
        public RoadNetworkWay LeftWay { get; private set; }

        // 車線(右)
        public RoadNetworkWay RightWay { get; private set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 左右両方のWayを返す
        public IEnumerable<RoadNetworkWay> BothWays
        {
            get
            {
                return Enumerable.Repeat(LeftWay, 1).Concat(Enumerable.Repeat(RightWay, 1)).Where(w => w != null);
            }
        }

        public IEnumerable<RoadNetworkWay> AllBorders
        {
            get
            {
                return Enumerable.Repeat(PrevBorder, 1).Concat(Enumerable.Repeat(NextBorder, 1)).Where(w => w != null);
            }
        }

        public bool IsValidWay => LeftWay != null && RightWay != null;

        /// <summary>
        /// 道の両方に接続先があるかどうか
        /// </summary>
        public bool IsBothConnectedLane => IsValidWay && PrevBorder != null && NextBorder != null;

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                foreach (var v in PrevBorder?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in LeftWay?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in NextBorder?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;

                foreach (var v in RightWay?.LineString?.Points?.Select(x => x.Vertex) ?? new List<Vector3>())
                    yield return v;
            }
        }

        public RoadNetworkLane(RoadNetworkWay leftWay, RoadNetworkWay rightWay, RoadNetworkWay startBorder, RoadNetworkWay endBorder)
        {
            LeftWay = leftWay;
            RightWay = rightWay;
            PrevBorder = startBorder;
            NextBorder = endBorder;
        }

        //デシリアライズの為に必要
        public RoadNetworkLane() { }

        // 向き反転させる
        public void Reverse()
        {
            (PrevBorder, NextBorder) = (NextBorder?.ReversedWay(), PrevBorder?.ReversedWay());
            (LeftWay, RightWay) = (RightWay?.ReversedWay(), LeftWay?.ReversedWay());

            (NextLanes, PrevLanes) = (PrevLanes, NextLanes);

        }

        /// <summary>
        /// LaneをsplitNumで分割する
        /// </summary>
        /// <param name="splitNum"></param>
        /// <returns></returns>
        public List<RoadNetworkLane> SplitLane(int splitNum)
        {
            if (IsValidWay == false)
                return null;

            if (splitNum <= 1)
                return new List<RoadNetworkLane> { this };

            // #TODO : とりあえず２分割のみ対応
            // Borderの中心を開始点とする
            var startSubWays = PrevBorder.Split(2);
            if (startSubWays.Any() == false)
                return null;

            var endSubWays = NextBorder.Split(2);
            if (endSubWays.Any() == false)
                return null;
            // #TODO : とりあえず２分割のみ対応
            var startPoint = startSubWays[0].Last();
            var endPoint = endSubWays[0].Last();

            // このBorderから出ていくWayすべてを使って中心線を書く
            var targetWays = new List<RoadNetworkWay> { LeftWay, RightWay };

            var candidates = new List<Vector3>();
            foreach (var way in targetWays)
            {
                for (var i = 1; i < way.Count - 1; ++i)
                {
                    var v = way[i];
                    var n = -way.GetVertexNormal(i).normalized;
                    var ray = new Ray(v + n * 0.01f, n);
                    if (GeoGraph2D.PolygonHalfLineIntersectionXZ(Vertices, ray, out var inter, out var t))
                        candidates.Add(Vector3.Lerp(v, inter, 0.5f));
                }
            }

            var centerLineVertices = new List<Vector3> { startPoint };
            while (candidates.Count > 0)
            {
                var before = centerLineVertices.Last();
                var found = candidates
                    // LaneがものすごいUターンしていたりする時の対応
                    // beforeから直接いけないものは無視
                    .Where(c => targetWays.All(w => w.SegmentIntersectionXz(before, c, out var _) == false))
                    .TryFindMin(x => (x - before).sqrMagnitude, out var nearPoint);

                if (found == false)
                {
                    //Assert.IsTrue(found, "center point not found");
                    DebugUtil.DrawArrow(before, before + Vector3.up * 2, arrowSize: 1f, duration: 30f, bodyColor: Color.blue);
                    foreach (var c in candidates)
                        DebugUtil.DrawArrow(c, c + Vector3.up * 100, arrowSize: 1f, duration: 30f, bodyColor: Color.red);
                    break;
                }

                centerLineVertices.Add(nearPoint);
                candidates.RemoveAll(x => (x - nearPoint).sqrMagnitude <= RoadNetworkModel.Epsilon);
            }

            centerLineVertices.Add(endPoint);

            // 自己交差があれば削除する
            GeoGraph2D.RemoveSelfCrossing(centerLineVertices, t => t.Xz(), (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));

            var centerLine = RoadNetworkLineString.Create(centerLineVertices);

            var leftLane = new RoadNetworkLane(LeftWay, new RoadNetworkWay(centerLine, false, true), startSubWays[0], endSubWays[0]);
            var rightLane = new RoadNetworkLane(new RoadNetworkWay(centerLine, false, false), RightWay, startSubWays[1], endSubWays[1]);

            return new List<RoadNetworkLane> { leftLane, rightLane };
        }

        /// <summary>
        /// Xz平面だけで見たときの, 線分(st, en)との最も近い交点を返す
        /// </summary>
        /// <param name="st"></param>
        /// <param name="en"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool SegmentIntersectionXz(Vector3 st, Vector3 en, out Vector3 intersection)
        {
            return GeoGraph2D.PolygonSegmentIntersectionXZ(Vertices, st, en, out intersection, out var t);
        }

        public static RoadNetworkLane CreateOneWayLane(RoadNetworkWay way)
        {
            return new RoadNetworkLane(way, null, null, null);
        }
    }

    /// <summary>
    /// ROadNetworkLaneの拡張関数
    /// </summary>
    public static class RoadNetworkLaneEx
    {
        public static List<Vector2> GetSplitEdges(this RoadNetworkLane self, float p)
        {
            if (self.IsValidWay == false)
                return new List<Vector2>();


            var segments = new List<LineSegment2D>();

            var lefts = self.LeftWay.GetEdges2D().ToList();
            var rights = self.RightWay.GetEdges2D().ToList();
            var rightIndex = 0;
            for (var i = 0; i < lefts.Count; ++i)
            {
                while (rightIndex < rights.Count)
                {
                    var l = lefts[i];
                    var r = rights[rightIndex];
                    var dot = Vector2.Dot(l.Direction, r.Direction);
                    var ray = GeoGraph2D.LerpRay(l.Ray, r.Ray, p);

                    var v0 = ray.GetNearestPoint(l.Start, out var t0);
                    var v1 = ray.GetNearestPoint(l.End, out var t1);
                    var v2 = ray.GetNearestPoint(r.Start, out var t2);
                    var v3 = ray.GetNearestPoint(r.End, out var t3);

                    var points = new List<Tuple<Vector2, float, int>>
                    {
                        // left
                        new(v0, t0, 0),
                        new(v1, t1, 1),
                        // right
                        new(v2, t2, 2),
                        new(v3, t3, 3),
                    };
                    points.Sort((a, b) => Comparer<float>.Default.Compare(a.Item2, b.Item2));
                    if ((points[0].Item3 / 2) != (points[1].Item3 / 2))
                    {
                        var segment = new LineSegment2D(points[1].Item1, points[2].Item1);
                        segments.Add(segment);
                        //DebugUtil.DrawString($"[{segments.Count}] {i}-{rightIndex}({segment.Magnitude})", segment.Start.Xay(), color: Color.red);
                        DebugUtil.DrawLineSegment2D(segment);
                    }

                    if (points[3].Item3 == 1)
                    {
                        rightIndex++;
                        continue;
                    }

                    break;
                }

            }

            var ret = new List<Vector2>();

            void Add(Vector2 point)
            {
                if (ret.Any() && (ret.Last() - point).sqrMagnitude < 0.01f)
                    return;
                ret.Add(point);
            }
            foreach (var x in segments)
            {
                Add(x.Start);
                //Add(x.End);
            }
            // 自己交差があれば削除する
            GeoGraph2D.RemoveSelfCrossing(ret, t => t, (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));

            return ret;
        }
    }
}