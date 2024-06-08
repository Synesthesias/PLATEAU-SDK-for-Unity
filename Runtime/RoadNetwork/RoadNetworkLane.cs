using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using UnityEngine.SocialPlatforms;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork
{
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
        public bool IsBothConnectedLane => IsValidWay && PrevLanes.Any() && NextLanes.Any();

        /// <summary>
        /// 両方に境界線を持っている
        /// </summary>
        public bool HasBothBorder => IsValidWay && PrevBorder != null && NextBorder != null;

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
        /// laneに対して連結情報があれば削除する
        /// </summary>
        /// <param name="lane"></param>
        public void RemoveConnection(RoadNetworkLane lane)
        {
            PrevLanes.Remove(lane);
            NextLanes.Remove(lane);
        }

        public void ReplaceConnection(RoadNetworkLane before, RoadNetworkLane after)
        {
            Replace(PrevLanes, before, after);
            Replace(NextLanes, before, after);
        }

        private static void Replace(List<RoadNetworkLane> list, RoadNetworkLane before, RoadNetworkLane after)
        {
            var index = list.FindIndex(l => l == before);
            if (index >= 0)
                list[index] = after;
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

            // Borderの中心を開始点とする
            var startSubWays = PrevBorder.Split(splitNum);
            if (startSubWays.Any() == false)
                return null;

            var endSubWays = NextBorder.Split(splitNum);
            if (endSubWays.Any() == false)
                return null;

            var ret = new List<RoadNetworkLane>();
            var leftWay = LeftWay;
            foreach (var i in Enumerable.Range(0, splitNum))
            {
                var p2 = (i + 1f) / splitNum;
                RoadNetworkWay r = RightWay;
                if (i != splitNum - 1)
                {
                    var points = new List<Vector2>();
                    void AddPoint(Vector2 p)
                    {
                        if (points.Any() && (points.Last() - p).magnitude < 0.001f)
                            return;
                        points.Add(p);
                    }
#if false
                    points = this.GetInnerLerpSegments(p2);
#else
                    var lefts = LeftWay.Vertices.Select(x => x.Xz()).ToList();
                    var rights =
                        RightWay.Vertices.Select(x => x.Xz()).ToList();
                    AddPoint(Vector3.Lerp(lefts[0], rights[0], p2));
                    var segments = GeoGraph2D.GetInnerLerpSegments(lefts, rights, p2);
                    foreach (var s in segments)
                    {
                        AddPoint(s.Segment.Start);
                        //AddPoint(s.Segment.End);
                    }
                    AddPoint(Vector3.Lerp(lefts[^1], rights[^1], p2));
#endif
                    var centerLine = RoadNetworkLineString.Create(points.Select(p => new RoadNetworkPoint(p.Xay())));
                    r = new RoadNetworkWay(centerLine, false, true);
                }
                var l = new RoadNetworkWay(leftWay.LineString, leftWay.IsReversed, false);
                var lane = new RoadNetworkLane(l, r, startSubWays[i], endSubWays[i]);
                leftWay = r;
                ret.Add(lane);
            }

            return ret;

            //// #TODO : とりあえず２分割のみ対応
            //var startPoint = startSubWays[0].Last();
            //var endPoint = endSubWays[0].Last();

            //// このBorderから出ていくWayすべてを使って中心線を書く
            //var targetWays = new List<RoadNetworkWay> { LeftWay, RightWay };

            //var candidates = new List<Vector3>();
            //foreach (var way in targetWays)
            //{
            //    for (var i = 1; i < way.Count - 1; ++i)
            //    {
            //        var v = way[i];
            //        var n = -way.GetVertexNormal(i).normalized;
            //        var ray = new Ray(v + n * 0.01f, n);
            //        if (GeoGraph2D.PolygonHalfLineIntersectionXZ(Vertices, ray, out var inter, out var t))
            //            candidates.Add(Vector3.Lerp(v, inter, 0.5f));
            //    }
            //}

            //var centerLineVertices = new List<Vector3> { startPoint };
            //while (candidates.Count > 0)
            //{
            //    var before = centerLineVertices.Last();
            //    var found = candidates
            //        // LaneがものすごいUターンしていたりする時の対応
            //        // beforeから直接いけないものは無視
            //        .Where(c => targetWays.All(w => w.SegmentIntersectionXz(before, c, out var _) == false))
            //        .TryFindMin(x => (x - before).sqrMagnitude, out var nearPoint);

            //    if (found == false)
            //    {
            //        //Assert.IsTrue(found, "center point not found");
            //        DebugEx.DrawArrow(before, before + Vector3.up * 2, arrowSize: 1f, duration: 30f, bodyColor: Color.blue);
            //        foreach (var c in candidates)
            //            DebugEx.DrawArrow(c, c + Vector3.up * 100, arrowSize: 1f, duration: 30f, bodyColor: Color.red);
            //        break;
            //    }

            //    centerLineVertices.Add(nearPoint);
            //    candidates.RemoveAll(x => (x - nearPoint).sqrMagnitude <= RoadNetworkModel.Epsilon);
            //}

            //centerLineVertices.Add(endPoint);

            //// 自己交差があれば削除する
            //GeoGraph2D.RemoveSelfCrossing(centerLineVertices, t => t.Xz(), (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));

            //var centerLine = RoadNetworkLineString.Create(centerLineVertices);

            //var leftLane = new RoadNetworkLane(LeftWay, new RoadNetworkWay(centerLine, false, true), startSubWays[0], endSubWays[0]);
            //var rightLane = new RoadNetworkLane(new RoadNetworkWay(centerLine, false, false), RightWay, startSubWays[1], endSubWays[1]);

            //return new List<RoadNetworkLane> { leftLane, rightLane };
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
        private struct Reference
        {
            public bool IsLeftWay { get; set; }

            public int Index { get; set; }
        }

        // Baseの方向を基底とするローカル座標系において
        // left.origin -> mid.Startは放物線
        // mid.Start -> mid.Endは線分となる軌跡
        private class EventFunc
        {
            public Parabola2D? Left { get; set; }

            public LineSegment2D Segment { get; }

            public int LeftIndex { get; }

            public int RightIndex { get; }

            public Vector2 Min
            {
                get
                {
                    if (Left.HasValue)
                        return Left.Value.Origin;
                    return Segment.Start;
                }
            }

            public Vector2 Max
            {
                get
                {
                    return Segment.End;
                }
            }

            public EventFunc(LineSegment2D segment, int leftIndex, int rightIndex)
            {
                Segment = segment;
                LeftIndex = leftIndex;
                RightIndex = rightIndex;
            }
        }

        private static Tuple<Vector2, Vector2> GetAxis(LineSegment2D axis)
        {
            return new Tuple<Vector2, Vector2>(axis.Direction, new Vector2(axis.Direction.y, -axis.Direction.x));
        }

        private static Vector2 ToLocalSpace(LineSegment2D axis, Vector2 v)
        {
            var (axisX, axisY) = GetAxis(axis);
            var s = v - axis.Start;
            return new Vector2(Vector2.Dot(s, axisX), Vector2.Dot(s, axisY));
        }

        private static Vector2 ToWorldSpace(LineSegment2D axis, Vector2 v)
        {
            var (axisX, axisY) = GetAxis(axis);
            return v.x * axisX + v.y * axisY + axis.Start;
        }

        private static LineSegment2D ToLocalSpace(LineSegment2D axis, LineSegment2D v)
        {
            return new LineSegment2D(ToLocalSpace(axis, v.Start), ToLocalSpace(axis, v.End));
        }

        private static LineSegment2D ToWorldSpace(LineSegment2D axis, LineSegment2D v)
        {
            return new LineSegment2D(ToWorldSpace(axis, v.Start), ToWorldSpace(axis, v.End));
        }

        /// <summary>
        /// selfの左右の道を横幅p : (1-p)で分割した線分を返す. p=[0,1]
        /// 例) 0.5だと中央線が返る, 0だとLeftが返る, 1だとRightが返る. 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<Vector2> GetInnerLerpSegments(this RoadNetworkLane self, float p)
        {
            p = Mathf.Clamp01(p);
            if (self.IsValidWay == false)
                return new List<Vector2>();

            // Item1 : 基準がLeft or Right? (trueだとLeft)
            // Item2 : 基準のSegmentのインデックス番号
            // Item3 : 分割線
            var segments = new List<LineSegment2D>();

            var lefts = self.LeftWay.GetEdges2D().ToList();
            var rights = self.RightWay.GetEdges2D().ToList();


            var floatComparer = Comparer<float>.Default;

            Reference? baseRef = null;
            var eventPoints = new List<EventFunc>();
            var leftIndex = 0;
            var rightIndex = 0;

            //LineSegment2D Get(Reference refer)
            //{
            //    return refer.IsLeftWay ? lefts[refer.Index] : rights[refer.Index];
            //}

            void Flush(Reference refer)
            {
                var tmp = new List<LineSegment2D>();
                var debugIndex = 0;
                foreach (var s in eventPoints)
                {
                    var (l, r) = (lefts[s.LeftIndex], rights[s.RightIndex]);
                    var (b, t) = refer.IsLeftWay ? (l, r) : (r, l);

                    var m = b.GetNearestPoint(t.Start);
                    var parabola = new LineSegment2D(Vector2.Lerp(m, t.Start, p), s.Segment.Start);
                    //tmp.Add(parabola);
                    tmp.Add(s.Segment);
                    void Draw(LineSegment2D seg)
                    {
                        DebugEx.DrawString($"[{debugIndex++}] {s.LeftIndex}-{s.RightIndex}({seg.Magnitude:F2})", seg.Start.Xay(), color: Color.red, fontSize: 20);
                        DebugEx.DrawLineSegment2D(seg, color: DebugEx.GetDebugColor(debugIndex, 8));
                    }
                    Draw(s.Segment);
                    //Draw(parabola);
                }

                //float Y(LineSegment2D s, float x)
                //{
                //    if (Mathf.Abs(s.End.x - s.Start.x) < GeoGraph2D.Epsilon)
                //        return s.Start.y;
                //    var t = (x - s.Start.x) / (s.End.x - s.Start.x);
                //    return s.Start.y + (s.End.y - s.Start.y) * t;
                //}
                //tmp.Sort((a, b) =>
                //{
                //    var ret = floatComparer.Compare(a.Start.x, b.Start.x);
                //    if (ret != 0) return ret;
                //    return floatComparer.Compare(a.Start.y, b.Start.y);
                //});
                segments.AddRange(tmp);
                eventPoints.Clear();
            }

            while (leftIndex < lefts.Count && rightIndex < rights.Count)
            {
                var l = lefts[leftIndex];
                var r = rights[rightIndex];

                var ray = GeoGraph2D.LerpRay(l.Ray, r.Ray, p);
                var points = new List<Tuple<Vector2, float, bool>>();

                void AddInter(Ray2D ray2, bool isLeft)
                {
                    var hit = LineUtil.LineIntersection(ray, ray2, out var inter, out var t1, out var t2);
                    if (!hit)
                        return;
                    points.Add(new(inter, t1, isLeft));
                }

                var dirL = new Vector2(l.Direction.y, -l.Direction.x);
                var dirR = new Vector2(r.Direction.y, -r.Direction.x);
                AddInter(new Ray2D(l.Start, dirL), true);
                AddInter(new Ray2D(l.End, dirL), true);
                AddInter(new Ray2D(r.Start, dirR), false);
                AddInter(new Ray2D(r.End, dirR), false);

                points.Sort((a, b) => floatComparer.Compare(a.Item2, b.Item2));

                // ベースラインの切り替えが走ったかどうか
                var isSwitch = false;
                if (points.Any())
                {
                    // 終了店がRightWay
                    var endLeft = points.Last().Item3;
                    isSwitch = baseRef.HasValue && baseRef.Value.IsLeftWay != endLeft;
                    baseRef = new Reference { IsLeftWay = endLeft, Index = leftIndex };
                }

                // left-rightが重ならない時は無視
                if (baseRef.HasValue && points.Count == 4 && points[0].Item3 != points[1].Item3 && Vector2.Angle(l.Direction, r.Direction) < 80)
                {
                    var (b, t) = baseRef.Value.IsLeftWay ? (l, r) : (r, l);
                    var segment = new LineSegment2D(points[1].Item1, points[2].Item1);
                    var ev = new EventFunc(segment, leftIndex, rightIndex);
                    eventPoints.Add(ev);
                    //DebugEx.DrawString($"[{segments.Count}] {leftIndex}-{rightIndex}({segment.Magnitude})", segment.Start.Xay(), color: Color.red, fontSize: 20);
                    //DebugEx.DrawLineSegment2D(segment, color: DebugEx.GetDebugColor(segments.Count, 8));
                }

                if (isSwitch)
                {
                    Flush(baseRef.Value);
                }

                if (baseRef is { IsLeftWay: true })
                    rightIndex++;
                else
                    leftIndex++;
            }

            if (baseRef.HasValue)
                Flush(baseRef.Value);
            var ret = new List<Vector2>();
            void Add(Vector2 point)
            {
                if (ret.Any() && (ret.Last() - point).sqrMagnitude < GeoGraph2D.Epsilon)
                    return;
                ret.Add(point);
            }

            //if (self.PrevBorder.GetLerpPoint(p, out var prevCp) >= 0)
            //    Add(prevCp);
            foreach (var x in segments)
            {
                Add(x.Start);
                Add(x.End);
            }

            //if (self.NextBorder.GetLerpPoint(p, out var nextCp) >= 0)
            //    Add(nextCp);

            // 自己交差があれば削除する
            //GeoGraph2D.RemoveSelfCrossing(ret, t => t, (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));

            return ret;
        }
    }
}