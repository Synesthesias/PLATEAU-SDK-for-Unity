using JetBrains.Annotations;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure.Drawer
{
    [Flags]
    public enum RnPartsTypeMask
    {
        Point = 1 << 0,
        LineString = 1 << 1,
        Way = 1 << 2,
        Lane = 1 << 3,
        Road = 1 << 4,
        Intersection = 1 << 5,
        Neighbor = 1 << 6,
        SideWalk = 1 << 7,
    }

    [Serializable]
    public class PLATEAURnModelDrawerDebug : MonoBehaviour
    {
        // --------------------
        // start:フィールド
        // --------------------
        [SerializeField] private PLATEAURnStructureModel target;

        [SerializeField] public bool visible = true;

        // Laneの頂点の内側を向くベクトルの中央点を表示する
        [SerializeField] public bool showInsideNormalMidPoint = false;
        // 頂点インデックスを表示する
        [SerializeField] public bool showVertexIndex = false;
        // 頂点の座標を表示する
        [SerializeField] public bool showVertexPos = false;
        // 頂点表示するときのフォントサイズ
        [SerializeField] public int showVertexFontSize = 20;
        // レーン描画するときに法線方向へオフセットを入れる
        [SerializeField] public float edgeOffset = 0f;
        [SerializeField] public float yScale = 1f;
        // このオブジェクトに紐づくものだけ表示する
        [SerializeField] public RnCityObjectGroupKey targetTran;
        [SerializeField] public RnPartsTypeMask showPartsType = 0;

        [SerializeField] public bool check = false;
        // 非表示オブジェクト
        public HashSet<object> InVisibleObjects { get; } = new();

        // エディタで選択されたオブジェクト
        public HashSet<object> SelectedObjects { get; } = new();

        public class DrawerModel : RnDebugDrawerModel<RnModel>
        { }

        /// <summary>
        /// Drawの最初でリセットされるフレーム情報
        /// </summary>
        public class DrawWork : DrawerModel.DrawFrameWork
        {
            public int DrawRoadGroupCount { get; set; }

            public PLATEAURnModelDrawerDebug Self { get; set; }

            public DrawWork(PLATEAURnModelDrawerDebug self, RnModel model)
                : base(model)
            {
                Self = self;
            }

            public override bool IsGuiSelected(object obj)
            {
                return Self.SelectedObjects.Contains(obj);
            }
        }

        public class Drawer<T> : DrawerModel.Drawer<DrawWork, T>
        {
            public override bool IsShowTarget(DrawWork work, T self)
            {
                if (GetTargetGameObjects(self) != null && work.Self.targetTran && GetTargetGameObjects(self).Contains(work.Self.targetTran) == false)
                    return false;

                // 非表示設定されている
                if (work.Self.InVisibleObjects.Contains(self))
                    return false;

                return true;
            }
        }

        private static IEnumerable<LineSegment3D> GetSplineSegments(Spline a, float interval)
        {
            var length = a.GetLength();
            var num = Mathf.Max(1, Mathf.FloorToInt(length / interval));

            for (var i = 0; i < num; ++i)
            {
                var t1 = 1f * i / num;
                var t2 = 1f * (i + 1) / num;
                a.Evaluate(t1, out var p1, out var _, out var _);
                a.Evaluate(t2, out var p2, out var _, out var _);
                yield return new LineSegment3D(p1, p2);
            }
        }

        private static IEnumerable<(T a, U b)> DirectProduct<T, U>(IEnumerable<T> a, IEnumerable<U> b)
        {
            var bList = b.ToList();
            foreach (var x in a)
            {
                foreach (var y in bList)
                {
                    yield return (x, y);
                }
            }
        }

        /// <summary>
        /// Spline間の最短距離を計算する
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static float CalcSplineNearestPoint(Spline a, Spline b, out Vector3 aPos, out Vector3 bPos)
        {
            float checkIntervalMeter = 3f;
            var aSegments = GetSplineSegments(a, checkIntervalMeter).ToList();
            var bSegments = GetSplineSegments(b, checkIntervalMeter).ToList();

            aPos = bPos = Vector3.zero;
            if (aSegments.Count == 0 || bSegments.Count == 0)
            {
                return float.MaxValue;
            }

            var min = float.MaxValue;
            foreach (var (x, y) in DirectProduct(aSegments, bSegments))
            {
                var n = y.GetNearestPoint(x.Start);
                var d = (n - x.Start).magnitude;
                if (d < min)
                {
                    aPos = x.Start;
                    bPos = n;
                    min = d;
                }

                n = x.GetNearestPoint(y.Start);
                d = (n - y.Start).magnitude;
                if (d < min)
                {
                    aPos = n;
                    bPos = y.Start;
                    min = d;
                }
            }

            return min;
        }

        [Serializable]
        public class IntersectionDrawer : Drawer<RnIntersection>
        {
            public override IEnumerable<RnCityObjectGroupKey> GetTargetGameObjects(RnIntersection self)
            {
                return self.TargetGroupKeys;
            }
        }

        [Serializable]
        public class IntersectionEdgeDrawer : Drawer<RnIntersectionEdge>
        {

        }

        public class RoadDrawer : Drawer<RnRoad>
        {
            public override IEnumerable<RnCityObjectGroupKey> GetTargetGameObjects(RnRoad self)
            {
                return self.TargetGroupKeys;
            }

            public override bool IsValid(RnRoad self)
            {
                return self.IsValid;
            }
        }

        public class LaneDrawer : Drawer<RnLane>
        {
            public override bool IsValid(RnLane self)
            {
                return self.IsValidWay;
            }
        }

        public class SideWalkDrawer : Drawer<RnSideWalk>
        {
            public override IEnumerable<RnCityObjectGroupKey> GetTargetGameObjects(RnSideWalk self)
            {
                return self?.ParentRoad?.TargetGroupKeys ?? Enumerable.Empty<RnCityObjectGroupKey>();
            }

            public override bool IsValid(RnSideWalk self)
            {
                return self.IsValid;
            }
        }

        public class WayDrawer : Drawer<RnWay>
        {
            public override bool IsValid(RnWay self)
            {
                return self.IsValid;
            }
        }

        [Serializable]
        public class IntersectionEdgeOption : IntersectionEdgeDrawer
        {
            // 境界線表示オプション
            public WayOption showWay;

            protected override bool DrawImpl(DrawWork work, RnIntersectionEdge self)
            {
                if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.Neighbor))
                    DebugEx.DrawString($"{self.GetDebugLabelOrDefault()}", RnIntersection.GetEdgeCenter(self));
                return showWay.Draw(work, self.Border);
            }
        }

        [Serializable]
        public class IntersectionOption : IntersectionDrawer
        {
            [Serializable]
            private class TrackDrawCenterLine : IntersectionDrawer
            {
                public float showTrackCenterLineRefineInterval = 5f;

                public bool showWidthLine = true;

                protected override bool DrawImpl(DrawWork work, RnIntersection intersection)
                {
                    var centerLineGraph = intersection.CreateCenterLineGraph(showTrackCenterLineRefineInterval);
                    foreach (var n in centerLineGraph.CenterLines)
                    {
                        foreach (var e in n.Value)
                        {
                            work?.Self.DrawWay(e.Value, new WayOption(true, Color.blue));
                        }
                    }

                    return true;
                }
            }

            [Serializable]
            private class TrackDrawSpline : IntersectionDrawer
            {
                public Color baseColor = Color.yellow * 0.7f;


                public Color disConnectedColor = Color.red;

                // 接続先の道路タイプを表示する
                public bool showConnectedRoadType = false;

                // スプライン描画するときに何m間隔で描画するか
                public float drawSplineInterval = 3f;

                // スプラインのノットで描画する
                public bool showKnots = false;

                // fromがこれを満たすトラックのみ表示
                public VisibleType fromRoadType = VisibleType.All;

                // toがこれを満たすトラックのみ表示
                public VisibleType toRoadType = VisibleType.All;

                // 同じエッジグループから出るトラックで隣り合うトラックとの最低限の距離
                public float needSplineDistance = 0f;

                [Serializable]
                public class TurnTypeColor
                {
                    [field: SerializeField]
                    public RnTurnType Type { get; set; }
                    [field: SerializeField]
                    public Color Color { get; set; }
                    public TurnTypeColor(RnTurnType type, Color color)
                    {
                        Type = type;
                        Color = color;
                    }
                }

                public bool useTurnTypeColor = false;
                public List<TurnTypeColor> turnTypeColors = EnumEx.GetValues<RnTurnType>().Select(x => new TurnTypeColor(x, DebugEx.GetDebugColor((int)x, RnTurnTypeEx.Count))).ToList();

                protected override bool DrawImpl(DrawWork work, RnIntersection intersection)
                {
                    var color = baseColor;

                    bool IsTarget(RnTrack track)
                    {
                        if (fromRoadType != VisibleType.All)
                        {
                            var fromRoad = intersection.FindEdges(track.FromBorder).FirstOrDefault()?.Road;
                            if ((fromRoadType & work.GetVisibleType(fromRoad, fromRoad?.TargetGroupKeys)) == 0)
                                return false;
                        }

                        if (toRoadType != VisibleType.All)
                        {
                            var toRoad = intersection.FindEdges(track.ToBorder).FirstOrDefault()?.Road;
                            if ((toRoadType & work.GetVisibleType(toRoad, toRoad?.TargetGroupKeys)) == 0)
                                return false;
                        }

                        return true;
                    }

                    foreach (var track in intersection.Tracks)
                    {
                        if (IsTarget(track) == false)
                            continue;
                        if (useTurnTypeColor)
                        {
                            color = turnTypeColors.FirstOrDefault(x => x.Type == track.TurnType)?.Color ?? DebugEx.GetDebugColor((int)track.TurnType, RnTurnTypeEx.Count);
                        }

                        Color CheckRoad(RnWay trackBorderWay)
                        {
                            var edge = intersection.FindEdges(trackBorderWay).FirstOrDefault();
                            if (edge?.Road is RnRoad)
                            {
                                if (intersection.GetConnectedLanes(trackBorderWay).Any() == false)
                                    return disConnectedColor;
                            }
                            // 何もなければ変更なし
                            return color;
                        }

                        color = CheckRoad(track.FromBorder);
                        color = CheckRoad(track.ToBorder);
                        if (showKnots)
                        {
                            work.Self.DrawArrows(track.Spline.Knots.Select(k => (Vector3)k.Position), false, color: color);
                        }
                        else
                        {
                            var length = GeoGraphEx.GetEdges(track.Spline.Knots.Select(k => k.Position), false)
                                .Sum(x => ((Vector3)(x.Item1) - (Vector3)(x.Item2)).magnitude);
                            var n = Mathf.Max(3, Mathf.FloorToInt(length / Mathf.Max(0.1f, drawSplineInterval)));

                            work.Self.DrawArrows(Enumerable.Range(0, n)
                                .Select(i => 1f * i / (n - 1))
                                .Select(t =>
                                {
                                    track.Spline.Evaluate(t, out var pos, out var tam, out var up);
                                    return (Vector3)pos;
                                }), false, color: color);

                            if (showConnectedRoadType)
                            {
                                void Draw(float p, RnIntersectionEdge e)
                                {
                                    var i0 = Mathf.Floor(p);
                                    var i1 = Mathf.Ceil(p);
                                    track.Spline.Evaluate(i0 / (n - 1), out var v0, out var _, out var _);
                                    track.Spline.Evaluate(i1 / (n - 1), out var v1, out var _, out var _);
                                    var v = Vector3.Lerp(v0, v1, 1f - (p - i0));
                                    var c = e.Road is RnRoad ? Color.green : Color.red;
                                    DebugEx.DrawRegularPolygon(v, 0.5f, color: c);
                                }

                                Draw(0.5f, intersection.FindEdges(track.FromBorder).FirstOrDefault());
                                Draw(n - 1.5f, intersection.FindEdges(track.ToBorder).FirstOrDefault());
                            }
                        }
                    }

                    if (needSplineDistance > 0f)
                    {
                        foreach (var (a, b) in DirectProduct(
                                     intersection.Tracks.Where(IsTarget)
                                     , intersection.Tracks.Where(IsTarget)))
                        {
                            if (a.FromBorder.IsSameLineSequence(b.FromBorder))
                                continue;

                            var aEdge = intersection.FindEdges(a.FromBorder).FirstOrDefault();
                            var bEdge = intersection.FindEdges(b.FromBorder).FirstOrDefault();
                            if (aEdge == null || bEdge == null)
                                continue;

                            if (aEdge.Road != bEdge.Road)
                                continue;


                            var distance = CalcSplineNearestPoint(a.Spline, b.Spline, out var aPos, out var bPos);
                            if (distance < needSplineDistance)
                            {
                                work.Self.DrawLine(aPos, bPos, color: Color.red);
                                work.Self.DrawString($"{distance}", Vector3.Lerp(aPos, bPos, 0.5f));
                            }
                        }
                    }


                    return true;
                }
            }

            [Serializable]
            public class DrawTrackOption : IntersectionDrawer
            {
                [SerializeField] private TrackDrawSpline drawSpline = new() { visible = true };

                [SerializeField] private TrackDrawCenterLine drawCenterLine = new() { visible = false };

                protected override IEnumerable<RnDebugDrawerModel<RnModel>.Drawer<DrawWork, RnIntersection>> GetChildDrawers()
                {
                    yield return drawSpline;
                    yield return drawCenterLine;
                }
            }

            // トラック表示オプション
            public DrawTrackOption showTrack = new() { visible = true };

            // 非境界線の表示オプション
            public IntersectionEdgeOption showNonBorderEdge = new()
            {
                visible = true,
                showWay = new WayOption(true, Color.magenta * 0.7f)
            };

            // 境界線表示オプション
            public IntersectionEdgeOption showBorderEdge = new()
            {
                visible = true,
                showWay = new(true, Color.cyan * 0.7f)
            };

            // 中央分離帯との境界線オプション
            public IntersectionEdgeOption showMedianBorderEdge = new()
            {
                visible = true,
                showWay = new(true, Color.yellow * 0.7f)
            };

            public bool showEdgeIndex = false;

            public bool showEdgeGroup = false;

            // 接続先の無いレーンを表示する
            public bool showNoTrackBorder = false;

            // 輪郭線の法線を表示する
            public bool showEdgeNormal = false;

            // 繋がっていないエッジ場所を表示する
            public bool showDisConnectEdgePoint = false;

            protected override IEnumerable<RnDebugDrawerModel<RnModel>.Drawer<DrawWork, RnIntersection>> GetChildDrawers()
            {
                yield return showTrack;
            }

            protected override bool DrawImpl(DrawWork work, RnIntersection intersection)
            {
                if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.Intersection))
                    DebugEx.DrawString($"N[{intersection.DebugMyId}]", intersection.GetCentralVertex());

                if (showEdgeGroup)
                {
                    var root = intersection.CreateEdgeGroup();
                    var edgeGroup = root;

                    var i = 0;
                    var x = 0;
                    foreach (var eg in edgeGroup)
                    {
                        var color = DebugEx.GetDebugColor(i++, edgeGroup.Count);
                        foreach (var n in eg.Edges)
                        {
                            work.Self.DrawWay(n.Border, new WayOption(true, eg.IsBorder ? color : Color.white));
                            work.Self.DrawString($"E[{x++}]", n.Border.GetLerpPoint(0.5f));
                        }
                    }
                }

                for (var i = 0; i < intersection.Edges.Count; i++)
                {
                    var edge = intersection.Edges[i];
                    var nextEdge = intersection.Edges[(i + 1) % intersection.Edges.Count];

                    // ループしていない時にエラー表示を行う
                    if (showDisConnectEdgePoint && edge.Border.GetPoint(-1) != nextEdge.Border.GetPoint(0))
                    {
                        DebugEx.DrawArrow(edge.Border.GetPoint(-1).Vertex, nextEdge.Border.GetPoint(0).Vertex, bodyColor: Color.red);
                    }

                    void DrawEdge(IntersectionEdgeDrawer p)
                    {
                        if (p.visible == false)
                            return;
                        p.Draw(work, edge);
                        if (showEdgeIndex)
                        {
                            var pos = edge.Border.GetLerpPoint(0.5f);
                            work.Self.DrawString($"B[{i}]", pos);
                        }
                    }

                    if (edge.IsBorder)
                    {
                        DrawEdge(edge.IsMedianBorder ? showMedianBorderEdge : showBorderEdge);
                    }
                    else
                    {
                        DrawEdge(showNonBorderEdge);
                    }



                    if (showEdgeNormal)
                    {
                        var normal = RnIntersection.GetEdgeNormal(edge);
                        var center = RnIntersection.GetEdgeCenter(edge);
                        work.Self.DrawLine(center, center - 50 * normal);
                    }


                }

                if (showNoTrackBorder)
                {
                    foreach (var border in intersection.Borders)
                    {
                        if (border.IsMedianBorder)
                            continue;
                        if (intersection.Tracks.Any(t => t.ContainsBorder(border.Border)))
                            continue;

                        work.Self.DrawString(border.GetDebugLabelOrDefault(), border.CalcCenter(), color: Color.red);
                    }
                }

                return true;
            }
        }
        [Serializable]
        public class RoadOption : RoadDrawer
        {
            [Serializable]
            public class RoadGroupDrawer : RoadDrawer
            {
                public Color color = Color.green;
                public bool showSpline = false;
                public bool showSplineKnot = false;
                public float pointSkipDistance = 1e-3f;

                protected override bool DrawImpl(DrawWork work, RnRoad road)
                {
                    var group = road.CreateRoadGroupOrDefault();
                    foreach (var r in group.Roads)
                        work.Visited.Add(r);

                    if (showSpline && group.TryCreateSimpleSpline(out var spline, out var width, pointSkipDistance: pointSkipDistance))
                    {
                        var n = spline.Count;
                        if (showSplineKnot)
                        {
                            //foreach (var knot in spline.Knots.Select((v, i) => new { v, i }))
                            //{
                            //    work.DrawString(knot.i.ToString(), knot.v.Position);
                            //}
                            work.Self.DrawArrows(spline.Knots.Select(knot => (Vector3)(knot.Position)), false, color: color, arrowSize: 1f);
                        }
                        else
                        {
                            var points = Enumerable.Range(0, n)
                                .Select(i => 1f * i / (n - 1))
                                .Select(t =>
                                {
                                    spline.Evaluate(t, out var pos, out var tam, out var up);
                                    var n = new Vector3(tam.z, 0f, -tam.x).normalized;
                                    if (n.x > 0)
                                        n = -n;
                                    return new { pos = (Vector3)pos, n };
                                }).ToList();
                            work.Self.DrawArrows(points.Select(p => p.pos), false, color: color, arrowSize: 1f);
                            work.Self.DrawArrows(points.Select(p => p.pos + 0.5f * p.n * width), false, color: color, arrowSize: 1f);
                            work.Self.DrawArrows(points.Select(p => p.pos - 0.5f * p.n * width), false, color: color, arrowSize: 1f);
                        }
                    }
                    else
                    {
                        var col = DebugEx.GetDebugColor(work.DrawRoadGroupCount++, 16);
                        foreach (var r in group.Roads)
                        {
                            foreach (var lane in r.MainLanes)
                            {
                                work.Self.DrawArrows(lane.GetVertices().Select(p => p.Vertex), false, color: col);
                            }
                        }
                    }
                    return true;
                }
            }

            [Serializable]
            public class RoadNormalDrawer : RoadDrawer
            {
                public bool showLaneConnection = false;

                public bool showSideEdge = false;
                public bool showEmptyRoadLabel = false;
                public bool showAdjustBorder = false;
                public DrawOption showNextConnection = new DrawOption(false, Color.red);
                public DrawOption showPrevConnection = new DrawOption(false, Color.blue);

                // 中央分離帯描画
                public LaneOption medianLaneOp = new LaneOption();

                public float verticalNext = -1f;

                protected override bool DrawImpl(DrawWork work, RnRoad road)
                {
                    if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.Road) || (showEmptyRoadLabel && road.IsEmptyRoad))
                        DebugEx.DrawString($"R[{road.DebugMyId}]", road.GetCentralVertex());

                    void DrawRoadConnection(DrawOption op, RnRoadBase target)
                    {
                        if (op.visible == false)
                            return;
                        if (target == null)
                            return;
                        var from = road.GetCentralVertex();
                        var to = target.GetCentralVertex();
                        work.Self.DrawArrow(from, to, bodyColor: op.color);
                    }

                    DrawRoadConnection(showNextConnection, road.Next);
                    DrawRoadConnection(showPrevConnection, road.Prev);


                    if (showSideEdge)
                    {
                        work.Self.DrawWay(road.GetMergedSideWay(RnDir.Left), work.Self.laneOp.showLeftWay);
                        work.Self.DrawWay(road.GetMergedSideWay(RnDir.Right), work.Self.laneOp.showRightWay);
                        work.Self.DrawWay(road.GetMergedBorder(RnLaneBorderType.Prev), work.Self.laneOp.showPrevBorder);
                        work.Self.DrawWay(road.GetMergedBorder(RnLaneBorderType.Next), work.Self.laneOp.showNextBorder);
                    }
                    else
                    {
                        Vector3? last = null;
                        foreach (var lane in road.MainLanes)
                        {
                            work.Self.DrawLane(lane, work.Self.laneOp, work.visibleType);
                            if (showLaneConnection)
                            {
                                if (last != null)
                                {
                                    work.Self.DrawArrow(last.Value, lane.GetCentralVertex());
                                }

                                last = lane.GetCentralVertex();
                            }
                        }
                    }

                    if (showAdjustBorder)
                    {
                        foreach (var bType in new[] { RnLaneBorderType.Prev, RnLaneBorderType.Next })
                        {
                            if (road.TryGetAdjustBorderSegment(bType, out var adjustBorder))
                            {
                                DebugEx.DrawArrow(adjustBorder.Start, adjustBorder.End);
                            }
                        }
                    }

                    work.Self.DrawLane(road.MedianLane, medianLaneOp, work.visibleType);
                    if (verticalNext > 0f)
                    {
                        if (road.TryGetVerticalSliceSegment(RnLaneBorderType.Next, verticalNext, out var next))
                        {
                            work.Self.DrawArrow(next.Start, next.End, bodyColor: Color.red);
                        }
                    }

                    return true;
                }
            }

            public RoadNormalDrawer normalDrawer = new RoadNormalDrawer { visible = true };
            public RoadGroupDrawer groupDrawer = new RoadGroupDrawer { visible = false, showSpline = true, color = Color.green };

            protected override IEnumerable<RnDebugDrawerModel<RnModel>.Drawer<DrawWork, RnRoad>> GetChildDrawers()
            {
                // グループ描画しているときは通常の描画は無視する
                if (groupDrawer.visible)
                    yield return groupDrawer;
                else
                    yield return normalDrawer;
            }
        }


        [Serializable]
        public class WayOption : WayDrawer
        {
            public Color color = Color.white;

            [Serializable]
            public class Option
            {
                // 頂点の法線を表示する
                public bool showVertexNormal = false;

                // 線の法線を表示する
                public bool showEdgeNormal = false;

                public bool changeArrowColor = false;

                // 反転したWayの矢印色
                public Color normalWayArrowColor = Color.yellow;

                // 通常Wayの矢印色
                public Color reverseWayArrowColor = Color.blue;

                // 矢印サイズ
                public float arrowSize = 0.5f;

                [NonSerialized]
                public Color arrowColor;
            }

            public Option option = new();

            public WayOption() { }

            public WayOption(bool visible, Color color)
            {
                this.visible = visible;
                this.color = color;
            }

            protected override bool DrawImpl(DrawWork work, RnWay way)
            {
                if (way.Count <= 1)
                    return false;

                // 矢印色は設定されていない場合は反転しているかどうかで返る
                var arrowColor = color;
                if (option.changeArrowColor)
                    arrowColor = way.IsReversed ? option.reverseWayArrowColor : option.normalWayArrowColor;

                work.Self.DrawArrows(
                    way.Vertices.Select((v, i) => v + -work.Self.edgeOffset * way.GetVertexNormal(i))
                    , false
                    , color: color
                    , arrowColor: arrowColor
                    , arrowSize: option.arrowSize
                    );

                if (work.Self.showVertexIndex)
                {
                    foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                        work.Self.DrawString(item.i.ToString(), item.v, color: Color.red, fontSize: work.Self.showVertexFontSize);
                }

                if (work.Self.showVertexPos)
                {
                    foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                        work.Self.DrawString(item.v.ToString(), item.v, color: Color.red, fontSize: work.Self.showVertexFontSize);
                }

                foreach (var i in Enumerable.Range(0, way.Count))
                {
                    var v = way[i];
                    var n = way.GetVertexNormal(i);

                    // 法線表示
                    if (option.showVertexNormal)
                    {
                        work.Self.DrawLine(v, v + n * 0.3f, color: Color.yellow);
                    }

                    if (option.showEdgeNormal && i < way.Count - 1)
                    {
                        var p = (v + way[i + 1]) * 0.5f;
                        var nn = way.GetEdgeNormal(i);
                        work.Self.DrawArrow(p, p + nn, bodyColor: Color.blue);
                    }

                    // 中央線
                    if (work.Self.showInsideNormalMidPoint)
                    {
                        if (way.HalfLineIntersectionXz(new Ray(v - n * 0.01f, -n), out var intersection))
                        {
                            work.Self.DrawArrow(v, (v + intersection) * 0.5f);
                        }
                    }
                }

                if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.Way))
                {
                    way.GetLerpPoint(0.5f, out var p);
                    DebugEx.DrawString($"{way.GetDebugIdLabelOrDefault()}", p);
                }

                foreach (var p in way.Points)
                {
                    work.Self.DrawPoint(p);
                }

                return true;
            }
        }

        [Serializable]
        public class LaneOption : LaneDrawer
        {
            public float bothConnectedLaneAlpha = 1f;
            public float validWayAlpha = 0.75f;
            public float invalidWayAlpha = 0.3f;
            public float reverseWayAlpha = 1f;

            public WayOption showLeftWay = new WayOption(true, Color.red);
            public WayOption showRightWay = new WayOption(true, Color.blue);
            public WayOption showPrevBorder = new WayOption(true, Color.green);
            public WayOption showNextBorder = new WayOption(true, Color.green);
            public WayOption showCenterWay = new WayOption(true, Color.green * 0.5f);
            public bool showNextRoad = false;
            public bool showPrevRoad = false;

            // レイアウト対応. ほとんど使わないオプションはここにまとめる
            public class Option
            {
                public bool showAttrText = false;
                // レーンの分割線を表示する
                public bool showSplitLane = false;
                public float splitLaneRate = 0.5f;
            }

            public Option option = new();

            /// <summary>
            /// レーン描画するときのアルファを返す
            /// </summary>
            /// <param name="self"></param>
            /// <returns></returns>
            public float GetLaneAlpha(RnLane self)
            {
                if (self.IsReversed)
                    return reverseWayAlpha;
                if (self.IsBothConnectedLane)
                    return bothConnectedLaneAlpha;
                if (self.IsValidWay)
                    return validWayAlpha;
                return invalidWayAlpha;
            }

            protected override bool DrawImpl(DrawWork work, RnLane lane)
            {

                if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.Lane))
                    DebugEx.DrawString($"L[{lane.DebugMyId}]", lane.GetCentralVertex());

                var offset = Vector3.up * (lane.DebugMyId % 10);

                void DrawWayImpl(WayOption op, RnWay way)
                {
                    work.Self.DrawWay(way, op);
                    if (option.showAttrText && (way.IsValidOrDefault()))
                        DebugEx.DrawString($"L:{lane.DebugMyId}", way[0] + offset);
                }

                DrawWayImpl(showLeftWay, lane.LeftWay);
                DrawWayImpl(showRightWay, lane.RightWay);
                DrawWayImpl(showPrevBorder, lane.PrevBorder);
                DrawWayImpl(showNextBorder, lane.NextBorder);

                if (showCenterWay.visible)
                {
                    var centerWay = lane.CreateCenterWay();
                    if (centerWay != null)
                        work.Self.DrawDashedArrows(centerWay, color: showCenterWay.color.PutA(GetLaneAlpha(lane)));
                }

                if (lane.NextBorder != null && lane.PrevBorder != null && lane.NextBorder.IsSameLineReference(lane.PrevBorder))
                {
                    DebugEx.DrawString($"Invalid Border Lane ", lane.GetCentralVertex());
                }

                if (option.showSplitLane && lane.HasBothBorder)
                {
                    var vers = lane.GetInnerLerpSegments(option.splitLaneRate);
                    work.Self.DrawArrows(vers, false, color: Color.red, arrowSize: 0.1f);
                }

                static void DrawNeighborConnection(bool enable, IEnumerable<RnRoadBase> neighbors, RnWay border, Color color)
                {
                    if (enable == false)
                        return;
                    foreach (var r in neighbors)
                    {
                        var c = r.GetCentralVertex();
                        var p = Vector3.zero;
                        var c1 = border?.GetLerpPoint(0.5f, out p);
                        if (c1 != null)
                        {
                            DebugEx.DrawArrow(p, c, bodyColor: Color.red);
                        }
                    }
                }
                DrawNeighborConnection(showNextRoad, lane.GetNextRoads(), lane.NextBorder, Color.red);
                DrawNeighborConnection(showPrevRoad, lane.GetPrevRoads(), lane.PrevBorder, Color.blue);

                return true;
            }
        }


        [Serializable]
        public class SideWalkOption : SideWalkDrawer
        {
            [Serializable]
            [Flags]
            public enum SideWalkLaneTypeMask
            {
                None = 0,
                Undefined = 1 << (RnSideWalkLaneType.Undefined),
                LeftLane = 1 << (RnSideWalkLaneType.LeftLane),
                RightLane = 1 << (RnSideWalkLaneType.RightLane),
                All = ~0,
            }

            public WayOption showOutsideWay = new WayOption(true, Color.red);
            public WayOption showInsideWay = new WayOption(true, Color.blue);
            public WayOption showStartEdgeWay = new WayOption(true, Color.green);
            public WayOption showEndEdgeWay = new WayOption(true, Color.yellow);
            // Noneの時はすべて表示. それ以外はRnSideWalk.GetValidWayTypeMaskが一致したものだけ表示する(不正なSideWalk検出用)
            public RnSideWalkWayTypeMask showWayFilter = RnSideWalkWayTypeMask.None;
            public SideWalkLaneTypeMask showLaneTypeFilter = SideWalkLaneTypeMask.All;
            // 親道路との接続を表示する
            public bool showParentConnection = false;

            protected override bool DrawImpl(DrawWork work, RnSideWalk sideWalk)
            {
                if (work.Self.showPartsType.HasFlag(RnPartsTypeMask.SideWalk))
                    DebugEx.DrawString($"S[{sideWalk.DebugMyId}]", sideWalk.GetCentralVertex());

                // 一致判定
                if (showWayFilter != RnSideWalkWayTypeMask.None && sideWalk.GetValidWayTypeMask() != showWayFilter)
                    return false;

                // レーンタイプで見る
                if (((1 << (int)sideWalk.LaneType) & (int)showLaneTypeFilter) == 0)
                    return false;

                work.Self.DrawWay(sideWalk.OutsideWay, showOutsideWay);
                work.Self.DrawWay(sideWalk.InsideWay, showInsideWay);
                work.Self.DrawWay(sideWalk.StartEdgeWay, showStartEdgeWay);
                work.Self.DrawWay(sideWalk.EndEdgeWay, showEndEdgeWay);

                if (showParentConnection)
                {
                    if (sideWalk.ParentRoad != null)
                    {
                        work.Self.DrawArrow(sideWalk.GetCentralVertex(), sideWalk.ParentRoad.GetCentralVertex(), bodyColor: Color.green);
                    }
                }

                return true;
            }
        }


        [SerializeField]
        public IntersectionOption intersectionOp = new IntersectionOption();

        [SerializeField]
        public RoadOption roadOp = new RoadOption();

        [SerializeField]
        public WayOption wayOp = new WayOption();

        [SerializeField]
        public LaneOption laneOp = new LaneOption();

        [SerializeField]
        public SideWalkOption sideWalkRoadOp = new SideWalkOption();

        // --------------------
        // end:フィールド
        // --------------------


        DrawWork work = new DrawWork(null, null);

        public void DrawArrows(IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true)
        {
            if (Mathf.Abs(yScale - 1f) < 1e-3f)
                DebugEx.DrawArrows(vertices, isLoop, arrowSize, arrowUp, color, arrowColor, duration, depthTest);
            else
                DebugEx.DrawArrows(vertices.Select(v => v.PutY(v.y * yScale)), isLoop, arrowSize, arrowUp, color, arrowColor, duration, depthTest);
        }

        public void DrawString(string text, Vector3 worldPos, Vector2? screenOffset = null, Color? color = null, int? fontSize = null)
        {
            DebugEx.DrawString(text, worldPos.PutY(worldPos.y * yScale), screenOffset, color, fontSize);
        }

        public void DrawLine(Vector3 start, Vector3 end, Color? color = null)
        {
            Debug.DrawLine(start.PutY(start.y * yScale), end.PutY(end.y * yScale), color ?? Color.white);
        }

        public void DrawDashedLines(IEnumerable<Vector3> vertices, bool isLoop = false, Color? color = null,
            float lineLength = 3f, float spaceLength = 1f)
        {
            DebugEx.DrawDashedLines(vertices.Select(v => v.PutY(v.y * yScale)), isLoop, color, lineLength, spaceLength);
        }

        public void DrawDashedArrows(IEnumerable<Vector3> vertices, bool isLoop = false, Color? color = null,
            float lineLength = 3f, float spaceLength = 1f)
        {
            DebugEx.DrawDashedArrows(vertices.Select(v => v.PutY(v.y * yScale)), isLoop, color, lineLength, spaceLength);
        }
        public void DrawArrow(
            Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? bodyColor = null
            , Color? arrowColor = null
            , float duration = 0f
            , bool depthTest = true)
        {
            DebugEx.DrawArrow(start.PutY(start.y * yScale), end.PutY(end.y * yScale), arrowSize, arrowUp, bodyColor, arrowColor, duration, depthTest);
        }

        private void DrawPoint(RnPoint p)
        {
            if (showPartsType.HasFlag(RnPartsTypeMask.Point))
                DebugEx.DrawString($"P[{p.DebugMyId}]", p.Vertex);
        }

        /// <summary>
        /// Way描画
        /// </summary>
        /// <param name="way"></param>
        /// <param name="color"></param>
        /// <param name="arrowColor"></param>
        private void DrawWay(RnWay way, [CanBeNull] WayOption op = null)
        {
            op ??= wayOp;
            if (op == null)
                return;
            op.Draw(work, way);
        }

        public void DrawSideWalk(RnSideWalk sideWalk, SideWalkOption p, RnDebugDrawerBase.VisibleType visibleType)
        {
            sideWalkRoadOp?.Draw(work, sideWalk, visibleType);
        }

        /// <summary>
        /// Lane描画
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="op"></param>
        /// <param name="visibleType"></param>
        private void DrawLane(RnLane lane, LaneOption op, RnDebugDrawerBase.VisibleType visibleType)
        {
            op?.Draw(work, lane, visibleType);
        }

        private void DrawRoad(RnRoad road, RnDebugDrawerBase.VisibleType visibleType)
        {
            roadOp?.Draw(work, road, visibleType);
        }

        /// <summary>
        /// Road描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawRoads(RnModel roadNetwork)
        {
            foreach (var road in roadNetwork.Roads)
            {
                DrawRoad(road, RnDebugDrawerBase.VisibleType.NonSelected);
            }
        }

        private void DrawIntersection(RnIntersection intersection, RnDebugDrawerBase.VisibleType visibleType)
        {
            intersectionOp?.Draw(work, intersection, visibleType);
        }

        /// <summary>
        /// Intersection描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawIntersections(RnModel roadNetwork)
        {
            foreach (var intersection in roadNetwork.Intersections)
            {
                DrawIntersection(intersection, RnDebugDrawerBase.VisibleType.NonSelected);
            }
        }

        private void DrawSideWalks(RnModel roadNetwork)
        {
            if (sideWalkRoadOp.visible == false)
                return;
            foreach (var sw in roadNetwork.SideWalks)
            {
                DrawSideWalk(sw, sideWalkRoadOp, RnDebugDrawerBase.VisibleType.NonSelected);
            }
        }

        public void Draw(RnModel roadNetwork)
        {
            if (!visible)
                return;
            if (roadNetwork == null)
                return;

            if (check)
                roadNetwork.Check();

            work = new DrawWork(this, roadNetwork);
            // 先に選択中のものから描画
            foreach (var x in SelectedObjects)
            {
                if (x is RnRoad r)
                {
                    if (r.ParentModel != roadNetwork)
                        continue;
                    DrawRoad(r, RnDebugDrawerBase.VisibleType.GuiSelected);
                }
                else if (x is RnIntersection i)
                {
                    if (i.ParentModel != roadNetwork)
                        continue;
                    DrawIntersection(i, RnDebugDrawerBase.VisibleType.GuiSelected);
                }
                else if (x is RnLane l)
                {
                    DrawLane(l, laneOp, RnDebugDrawerBase.VisibleType.GuiSelected);
                }
                else if (x is RnSideWalk sw)
                {
                    DrawSideWalk(sw, sideWalkRoadOp, RnDebugDrawerBase.VisibleType.GuiSelected);
                }
                else if (x is RnLineString ls)
                {
                    DrawArrows(ls.Points.Select(p => p.Vertex), false, color: wayOp.option.normalWayArrowColor, arrowSize: wayOp.option.arrowSize);
                }
            }

            DrawRoads(roadNetwork);
            DrawIntersections(roadNetwork);
            DrawSideWalks(roadNetwork);
        }


        public void OnDrawGizmos()
        {
            if (!target)
                target = GetComponent<PLATEAURnStructureModel>();
            if (target)
                Draw(target.RoadNetwork);
        }
    }
}