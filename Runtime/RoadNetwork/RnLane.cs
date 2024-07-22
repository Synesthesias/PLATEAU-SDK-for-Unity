using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using UnityEngine.SocialPlatforms;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork
{
    public enum RnLaneBorderType
    {
        Prev,
        Next
    }

    public static class RnLaneBorderTypeEx
    {
        /// <summary>
        /// 反対側を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RnLaneBorderType GetOpposite(this RnLaneBorderType self)
        {
            return self == RnLaneBorderType.Prev ? RnLaneBorderType.Next : RnLaneBorderType.Prev;
        }
    }

    public enum RnLaneBorderDir
    {
        // LeftWay -> RightWay
        Left2Right,
        // RightWay -> LeftWay
        Right2Left
    }

    public static class RnLaneBorderDirEx
    {
        /// <summary>
        /// 反対値を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RnLaneBorderDir GetOpposite(this RnLaneBorderDir self)
        {
            return self == RnLaneBorderDir.Left2Right ? RnLaneBorderDir.Right2Left : RnLaneBorderDir.Left2Right;
        }
    }

    [Flags]
    public enum RnLaneAttribute
    {
        // 左折専用
        LeftOnly = 1 << 0,
        // 右折専用
        RightOnly = 1 << 1,
    }

    public class RnLane : ARnParts<RnLane>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 親リンク
        public RnRoadBase Parent { get; set; }

        // 境界線(下流)
        public RnWay PrevBorder { get; set; }

        // 境界線(上流)
        public RnWay NextBorder { get; set; }

        // 車線(左)
        public RnWay LeftWay { get; private set; }

        // 車線(右)
        public RnWay RightWay { get; private set; }

        // 属性
        public RnLaneAttribute Attributes { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 左右両方のWayを返す
        public IEnumerable<RnWay> BothWays
        {
            get
            {
                return Enumerable.Repeat(LeftWay, 1).Concat(Enumerable.Repeat(RightWay, 1)).Where(w => w != null);
            }
        }

        public IEnumerable<RnWay> AllBorders
        {
            get
            {
                return Enumerable.Repeat(PrevBorder, 1).Concat(Enumerable.Repeat(NextBorder, 1)).Where(w => w != null);
            }
        }

        /// <summary>
        /// 有効なレーンかどうか
        /// Left/Rightどっちも有効ならtrue
        /// </summary>
        public bool IsValidWay => LeftWay.IsValidOrDefault() && RightWay.IsValidOrDefault();

        /// <summary>
        /// 道の両方に接続先があるかどうか
        /// </summary>
        public bool IsBothConnectedLane => IsValidWay && GetNextRoad() != null && GetPrevRoad() != null;

        /// <summary>
        /// 両方に境界線を持っている
        /// </summary>
        public bool HasBothBorder => IsValidWay && PrevBorder.IsValidOrDefault() && NextBorder.IsValidOrDefault();

        public RnLane(RnWay leftWay, RnWay rightWay, RnWay startBorder, RnWay endBorder)
        {
            LeftWay = leftWay;
            RightWay = rightWay;
            PrevBorder = startBorder;
            NextBorder = endBorder;
        }

        //デシリアライズの為に必要
        public RnLane() { }

        /// <summary>
        /// borderと接続しているレーンを全て取得
        /// </summary>
        /// <param name="border"></param>
        /// <returns></returns>
        private IEnumerable<RnLane> GetConnectedLanes(RnWay border)
        {
            if (Parent == null || border == null)
                yield break;

            foreach (var n in Parent.GetNeighborRoads())
            {
                foreach (var lane in n.AllLanes)
                {
                    if (lane.AllBorders.Any(b => b.IsSameLine(border)))
                        yield return lane;
                }
            }
        }

        /// <summary>
        /// このレーン接続先のRnRoadBaseを取得. ParentのNext/Prevとは逆になる可能性がある
        /// </summary>
        /// <returns></returns>
        public RnRoadBase GetNextRoad()
        {
            return GetNextLanes().FirstOrDefault(w => w.Parent != null)?.Parent;
        }

        /// <summary>
        /// このレーン接続元のRnRoadBaseを取得. ParentのNext/Prevとは逆になる可能性がある
        /// </summary>
        /// <returns></returns>
        public RnRoadBase GetPrevRoad()
        {
            return GetPrevLanes().FirstOrDefault(w => w.Parent != null)?.Parent;
        }


        /// <summary>
        /// 接続先レーンをすべて取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetNextLanes()
        {
            return GetConnectedLanes(NextBorder);
        }

        /// <summary>
        /// 接続元レーンをすべて取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetPrevLanes()
        {
            return GetConnectedLanes(PrevBorder);
        }

        // 向き反転させる
        public void Reverse()
        {
            (PrevBorder, NextBorder) = (NextBorder?.ReversedWay(), PrevBorder?.ReversedWay());
            (LeftWay, RightWay) = (RightWay?.ReversedWay(), LeftWay?.ReversedWay());
        }

        /// <summary>
        /// Borderの向きをborderDirになるようにそろえる
        /// </summary>
        public void AlignBorder(RnLaneBorderDir borderDir = RnLaneBorderDir.Left2Right)
        {
            AlignBorder(RnLaneBorderType.Prev, borderDir);
            AlignBorder(RnLaneBorderType.Next, borderDir);
        }

        /// <summary>
        /// typeの境界線をborderDirにそろえる
        /// </summary>
        /// <param name="type"></param>
        /// <param name="borderDir"></param>
        private void AlignBorder(RnLaneBorderType type, RnLaneBorderDir borderDir)
        {
            var border = GetBorder(type);
            if (border == null)
                return;
            var dir = GetBorderDir(type);
            if (dir != borderDir)
            {
                border.Reverse(true);
            }
        }

        private static void Replace(List<RnLane> list, RnLane before, RnLane after)
        {
            var index = list.FindIndex(l => l == before);
            if (index >= 0)
                list[index] = after;
        }

        /// <summary>
        /// 境界線を取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public RnWay GetBorder(RnLaneBorderType type)
        {
            return type switch
            {
                RnLaneBorderType.Prev => PrevBorder,
                RnLaneBorderType.Next => NextBorder,
                _ => null
            };
        }

        /// <summary>
        /// 境界線の方向を取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public RnLaneBorderDir? GetBorderDir(RnLaneBorderType type)
        {
            var border = GetBorder(type);
            if (border == null)
                return null;
            if (IsValidWay == false)
                return null;
            // borderの0番目の点がLeftWayの0番目の点と同じならLeft2Right
            if (border.GetPoint(0) == LeftWay.GetPoint(0))
                return RnLaneBorderDir.Left2Right;

            if (border.GetPoint(0) == RightWay.GetPoint(0))
                return RnLaneBorderDir.Right2Left;

            var d = border.GetPoint(1).Vertex - border.GetPoint(0).Vertex;
            // #NOTE : Laneが複雑な形状をしているときのためPrevはPrev側, NextBorderだとNext側を見る
            var index = type == RnLaneBorderType.Prev ? 0 : -1;
            var d2 = RightWay.GetPoint(index).Vertex - LeftWay.GetPoint(index).Vertex;

            if (Vector2.Dot(d.Xz(), d2.Xz()) > 0)
                return RnLaneBorderDir.Left2Right;
            return RnLaneBorderDir.Right2Left;
        }


        public static RnLane CreateOneWayLane(RnWay way)
        {
            return new RnLane(way, null, null, null);
        }
    }

    /// <summary>
    /// ROadNetworkLaneの拡張関数
    /// </summary>
    public static class RnLaneEx
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
        public static List<Vector3> GetInnerLerpSegments(this RnLane self, float p2)
        {
            var points = new List<Vector3>();
            void AddPoint(Vector3 p)
            {
                if (points.Any() && (points.Last() - p).magnitude < 0.001f)
                    return;
                points.Add(p);
            }

            var lefts = self.LeftWay.Vertices.ToList();
            var rights =
                self.RightWay.Vertices.ToList();
            AddPoint(Vector3.Lerp(lefts[0], rights[0], p2));
            var segments = GeoGraphEx.GetInnerLerpSegments(lefts, rights, AxisPlane.Xz, p2);
            foreach (var s in segments)
            {
                AddPoint(s.Segment.Start);
            }
            AddPoint(Vector3.Lerp(lefts[^1], rights[^1], p2));
            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="borderWay"></param>
        /// <param name="leftPos">Laneの外側から見て左側の境界点</param>
        /// <param name="leftNormal">Laneの外側から見て左側の境界点の進行方向(Node側の方向)</param>
        /// <param name="rightPos">Laneの外側から見て右側の境界点</param>
        /// <param name="rightNormal">Laneの外側から見て右側の境界点の進行方向(Node側の方向)</param>
        /// <returns></returns>
        public static bool TryGetBorderNormal(this RnLane self, RnWay borderWay, out Vector3 leftPos, out Vector3 leftNormal, out Vector3 rightPos, out Vector3 rightNormal)
        {
            leftNormal = rightNormal = Vector3.zero;
            leftPos = rightPos = Vector3.zero;
            if (self.IsValidWay == false)
                return false;

            if (self.PrevBorder?.LineString == borderWay.LineString)
            {
                leftPos = self.LeftWay[0];
                rightPos = self.RightWay[0];
                leftNormal = (self.LeftWay[0] - self.LeftWay[1]).normalized;
                rightNormal = (self.RightWay[0] - self.RightWay[1]).normalized;
                return true;
            }

            if (self.NextBorder?.LineString == borderWay.LineString)
            {
                leftPos = self.RightWay[^1];
                rightPos = self.LeftWay[^1];
                leftNormal = (self.RightWay[^1] - self.RightWay[^2]).normalized;
                rightNormal = (self.LeftWay[^1] - self.LeftWay[^2]).normalized;
                return true;
            }

            return false;
        }

        /// <summary>
        /// PrevBorderの長さを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcPrevBorderWidth(this RnLane self)
        {
            return self.PrevBorder?.LineString?.CalcLength() ?? 0f;
        }

        /// <summary>
        /// NextBorderの長さを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcNextBorderWidth(this RnLane self)
        {
            return self.NextBorder?.LineString?.CalcLength() ?? 0f;
        }

        /// <summary>
        /// このレーンの幅を返す(Next/PrevBorderの短い方)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcWidth(this RnLane self)
        {
            return Mathf.Min(self.CalcNextBorderWidth(), self.CalcPrevBorderWidth());
        }

        /// <summary>
        /// LeftWay -> NextBorder -> RightWay -> PrevBorderの順に頂点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<RnPoint> GetVertices(this RnLane self)
        {
            IEnumerable<RnPoint> GetWayPoints(RnWay way, bool isReverse)
            {
                if (way == null)
                    yield break;

                for (var i = 0; i < way.Count; ++i)
                {
                    var index = isReverse ? way.Count - 1 - i : i;
                    yield return way.GetPoint(index);
                }
            }

            IEnumerable<RnPoint> GetBorderPoints(RnLaneBorderType type, RnLaneBorderDir dir)
            {
                var border = self.GetBorder(type);
                if (border == null)
                    yield break;

                if (dir == self.GetBorderDir(type))
                {
                    for (var i = 0; i < border.Count; ++i)
                        yield return border.GetPoint(i);
                }
                else
                {
                    for (var i = border.Count - 1; i >= 0; --i)
                        yield return border.GetPoint(i);
                }
            }

            foreach (var p in GetWayPoints(self.LeftWay, false))
                yield return p;

            foreach (var p in GetBorderPoints(RnLaneBorderType.Next, RnLaneBorderDir.Left2Right))
                yield return p;

            foreach (var p in GetWayPoints(self.RightWay, true))
                yield return p;

            foreach (var p in GetBorderPoints(RnLaneBorderType.Prev, RnLaneBorderDir.Right2Left))
                yield return p;
        }

        /// <summary>
        /// selfの全頂点の重心を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(this RnLane self)
        {
            var a = self.GetVertices().Aggregate(new { sum = Vector3.zero, i = 0 }, (a, p) => new { sum = a.sum + p.Vertex, i = a.i + 1 });
            if (a.i == 0)
                return Vector3.zero;
            return a.sum / a.i;
        }

        public static RnLaneBorderType? GetBorderType(this RnLane self, RnWay border)
        {
            if (self.PrevBorder == border)
                return RnLaneBorderType.Prev;
            if (self.NextBorder == border)
                return RnLaneBorderType.Next;
            return null;
        }

        /// <summary>
        /// LaneをsplitNumで分割したLaneリストを返す. 隣接するLinkのLaneは分割しない
        /// </summary>
        /// <param name="splitNum"></param>
        /// <returns></returns>
        public static List<RnLane> SplitLaneSelf(this RnLane self, int splitNum)
        {
            var ret = self.SplitLane(splitNum, false);
            return ret.GetValueOrDefault(self) ?? new List<RnLane>();
        }

        /// <summary>
        /// レーンの分割を行う. withConnectedLinkLaneがtrueの場合は隣接するLinkのLaneも分割する
        /// 戻り値 : Key:分割前のレーン, Value:分割後のレーン
        /// </summary>
        /// <param name="self"></param>
        /// <param name="splitNum"></param>
        /// <param name="withConnectedLinkLane"></param>
        /// <param name="rateSelector">i番目のWayの場所を返す. nullの場合は全て等間隔</param>
        /// <returns></returns>
        public static Dictionary<RnLane, List<RnLane>> SplitLane(this RnLane self, int splitNum, bool withConnectedLinkLane, Func<int, float> rateSelector = null)
        {
            if (splitNum <= 1)
                return new Dictionary<RnLane, List<RnLane>>();

            if ((self?.HasBothBorder ?? false) == false)
                return new Dictionary<RnLane, List<RnLane>>();

            if (rateSelector == null)
                rateSelector = i => (i + 1f) / splitNum;

            var visited = new HashSet<RnLane>();
            var queue = new Queue<RnLane>();
            queue.Enqueue(self);

            // 境界線
            var wayEqualComp = new RnWayEqualityComparer(true);
            var border2SubBorders = new Dictionary<RnWay, List<RnWay>>(wayEqualComp);

            List<RnWay> GetSplitBorder(RnLane l, RnLaneBorderType borderType, out bool isLeft2Right)
            {
                isLeft2Right = true;
                var border = l.GetBorder(borderType);
                // ContainsKeyやTriGetValueだとEqualityComparerが反応しないのでAnyで無理やり
                if (border2SubBorders.TryGetValue(border, out var ret) == false)
                {
                    ret = border.Split(splitNum, true);
                    border2SubBorders[border] = ret;
                }
                if (ret.Any() == false || ret[0].IsValid == false)
                    return ret;
                var s = ret[0].GetPoint(0);
                if (s == border.GetPoint(0))
                {
                    isLeft2Right = l.GetBorderDir(borderType) == RnLaneBorderDir.Left2Right;
                }
                else if (s == border.GetPoint(-1))
                {
                    isLeft2Right = l.GetBorderDir(borderType) == RnLaneBorderDir.Right2Left;
                }
                else
                {
                    Assert.IsTrue(false, "SplitBorder Error");
                }
                return ret;
            }

            var ret = new Dictionary<RnLane, List<RnLane>>();
            while (queue.Count > 0)
            {
                var targetLane = queue.Dequeue();
                if (visited.Contains(targetLane))
                    continue;
                visited.Add(targetLane);

                ret[targetLane] = new List<RnLane>();

                // 隣接するレーンがLinkのLaneの場合それをキューに積む
                if (withConnectedLinkLane)
                {
                    var nextLanes = targetLane.GetNextLanes().ToList();
                    var prevLanes = targetLane.GetPrevLanes().ToList();
                    var neighbors = nextLanes
                        .Concat(prevLanes)
                        .Where(l => l.Parent is RnLink && visited.Contains(l) == false)
                        .ToList();
                    foreach (var l in neighbors)
                        queue.Enqueue(l);
                }

                var prevSubBorders = GetSplitBorder(targetLane, RnLaneBorderType.Prev, out var isPrevLeft2Right);
                var nextSubBorders = GetSplitBorder(targetLane, RnLaneBorderType.Next, out var isNextLeft2Right);

                var leftWay = targetLane.LeftWay;
                foreach (var i in Enumerable.Range(0, splitNum))
                {
                    var prevBorder = prevSubBorders[isPrevLeft2Right ? i : prevSubBorders.Count - 1 - i];
                    var nextBorder = nextSubBorders[isNextLeft2Right ? i : nextSubBorders.Count - 1 - i];
                    var rate = rateSelector(i);
                    RnWay r = new RnWay(targetLane.RightWay.LineString, targetLane.RightWay.IsReversed, true);
                    if (i != splitNum - 1)
                    {
                        var points = new List<RnPoint>();
                        void AddPoint(RnPoint p)
                        {
                            if (points.Any() && (points.Last().Vertex - p.Vertex).sqrMagnitude < 0.001f)
                                return;
                            points.Add(p);
                        }

                        if (isPrevLeft2Right)
                            AddPoint(prevBorder.Points.Last());
                        else
                            AddPoint(prevBorder.Points.First());

                        var segments = GeoGraphEx.GetInnerLerpSegments(targetLane.LeftWay.Vertices.ToList(), targetLane.RightWay.Vertices.ToList(), AxisPlane.Xz, rate);
                        foreach (var s in segments)
                        {
                            AddPoint(new RnPoint(s.Segment.Start));
                        }

                        if (isNextLeft2Right)
                            AddPoint(nextBorder.Points.Last());
                        else
                            AddPoint(nextBorder.Points.First());

                        GeoGraph2D.RemoveSelfCrossing(points, t => t.Vertex.Xz(), (p1, p2, p3, p4, inter, f1, f2) => new RnPoint(Vector3.Lerp(p1, p2, f1)));
                        var rightLine = RnLineString.Create(points);
                        r = new RnWay(rightLine, false, targetLane.RightWay.IsReverseNormal);
                    }
                    var l = new RnWay(leftWay.LineString, leftWay.IsReversed, false);
                    var newLane = new RnLane(l, r, prevBorder, nextBorder);
                    ret[targetLane].Add(newLane);
                    leftWay = r;
                }
            }

            return ret;
        }
#if false
        public static void SetLaneWidth(this RnLane self, float width, bool moveLeft)
        {
            if (self == null)
                return;

            if (self.IsValidWay == false)
                return;

            void Move(RnWay move, RnWay keep)
            {
                for (var i = 0; i < move.Count; ++i)
                {
                    var m = move.GetPoint(i);

                    var n = move.GetVertexNormal(i).normalized;
                    if (keep.FindNearestPoint(m.Vertex, out var pos))
                    {
                        var d = m.Vertex - pos;

                        var del = width - d.magnitude; ;
                        m.Vertex += del * d;
                    }

                }
            }

            if (moveLeft)
            {
                Move(self.LeftWay, self.RightWay);
            }
            else
            {
                Move(self.RightWay, self.LeftWay);
            }
        }
#endif
#if false
        public static List<Vector2> GetInnerLerpSegments2(this RoadNetworkLane self, float p)
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
#endif
    }
}