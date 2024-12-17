using JetBrains.Annotations;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using static PLATEAU.RoadNetwork.Structure.Drawer.PLATEAURnModelDrawerDebug;
using static PLATEAU.RoadNetwork.Util.LineCrossPointResult;

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
        [Flags]
        public enum VisibleType
        {
            Empty = 0,
            // 選択されていないもの
            NonSelected = 1 << 0,
            // シーンで選択されたGameObjectに紐づくもの
            SceneSelected = 1 << 1,
            // EditorWindowで選択されたもの
            GuiSelected = 1 << 2,
            // 全て
            All = ~0
        }
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
        [SerializeField] public PLATEAUCityObjectGroup targetTran = null;
        [SerializeField] public RnPartsTypeMask showPartsType = 0;

        // 非表示オブジェクト
        public HashSet<object> InVisibleObjects { get; } = new();

        // エディタで選択されたオブジェクト
        public HashSet<object> SelectedObjects { get; } = new();

        public class Drawer<T> where T : ARnPartsBase
        {
            // 表示非表示設定
            public bool visible = false;

            // 表示対象設定
            public VisibleType showVisibleType = VisibleType.All;

            protected virtual bool DrawImpl(DrawWork work, T self) { return true; }

            // 子Drawer
            protected virtual IEnumerable<Drawer<T>> GetChildDrawers() => Enumerable.Empty<Drawer<T>>();

            public bool Draw(DrawWork work, T self, VisibleType visibleType)
            {
                if (visible == false)
                    return false;

                if (self == null)
                    return false;

                var lastVisibleType = visibleType;
                try
                {
                    if (GetTargetGameObjects(self)?.Any(RnEx.IsEditorSceneSelected) ?? false)
                    {
                        visibleType |= VisibleType.SceneSelected;
                        visibleType &= ~VisibleType.NonSelected;
                    }

                    if (work.IsVisited(self) == false)
                        return false;

                    if (GetTargetGameObjects(self) != null && work.Self.targetTran && GetTargetGameObjects(self).Contains(work.Self.targetTran) == false)
                        return false;

                    // 非表示設定されている
                    if (work.Self.InVisibleObjects.Contains(self))
                        return false;

                    static bool Exec(Drawer<T> drawer, DrawWork work, T obj)
                    {
                        if (drawer.visible == false)
                            return false;

                        if ((work.visibleType & drawer.showVisibleType) == 0)
                            return false;

                        if (drawer.DrawImpl(work, obj) == false)
                            return false;

                        foreach (var child in drawer.GetChildDrawers() ?? new List<Drawer<T>>())
                        {
                            Exec(child, work, obj);
                        }

                        return true;
                    }
                    work.visibleType = visibleType;
                    return Exec(this, work, self);
                }
                finally
                {
                    work.visibleType = lastVisibleType;
                }
            }

            public virtual IEnumerable<PLATEAUCityObjectGroup> GetTargetGameObjects(T self) => null;
        }

        [Serializable]
        public class IntersectionDrawer : Drawer<RnIntersection>
        {
            public override IEnumerable<PLATEAUCityObjectGroup> GetTargetGameObjects(RnIntersection self)
            {
                return self.TargetTrans;
            }
        }

        public class RoadDrawer : Drawer<RnRoad>
        {
            public override IEnumerable<PLATEAUCityObjectGroup> GetTargetGameObjects(RnRoad self)
            {
                return self.TargetTrans;
            }
        }

        public class LaneDrawer : Drawer<RnLane> { }

        public class SideWalkDrawer : Drawer<RnSideWalk>
        {
            public override IEnumerable<PLATEAUCityObjectGroup> GetTargetGameObjects(RnSideWalk self)
            {
                return self?.ParentRoad?.TargetTrans ?? Enumerable.Empty<PLATEAUCityObjectGroup>();
            }
        }

        public class WayDrawer : Drawer<RnWay> { }



        [Serializable]
        public class IntersectionOption : IntersectionDrawer
        {
            [Serializable]
            private class TrackDrawCenterLine : IntersectionDrawer
            {
                public float showTrackCenterLineRefineInterval = 5f;

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

                public bool useTurnTypeColor = false;

                public Color disConnectedColor = Color.red;

                // 接続先の道路タイプを表示する
                public bool showConnectedRoadType = false;

                // スプライン描画するときに何m間隔で描画するか
                public float drawSplineInterval = 3f;

                // スプラインのノットで描画する
                public bool showKnots = false;

                protected override bool DrawImpl(DrawWork work, RnIntersection intersection)
                {
                    var color = baseColor;

                    foreach (var track in intersection.Tracks)
                    {
                        if (useTurnTypeColor)
                        {
                            color = DebugEx.GetDebugColor((int)track.TurnType, RnTurnTypeEx.Count);
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
                                void Draw(float p, RnNeighbor e)
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

                    return true;
                }
            }

            [Serializable]
            public class DrawTrackOption : IntersectionDrawer
            {
                [SerializeField] private TrackDrawSpline drawSpline = new() { visible = true };

                [SerializeField] private TrackDrawCenterLine drawCenterLine = new() { visible = false };

                protected override IEnumerable<Drawer<RnIntersection>> GetChildDrawers()
                {
                    yield return drawSpline;
                    yield return drawCenterLine;
                }
            }

            // トラック表示オプション
            public DrawTrackOption showTrack = new() { visible = true };

            // 非境界線の表示オプション
            public WayOption showNonBorderEdge = new(true, Color.magenta * 0.7f);

            // 境界線表示オプション
            public WayOption showBorderEdge = new(true, Color.cyan * 0.7f);

            // 中央分離帯との境界線オプション
            public WayOption showMedianBorderEdge = new(true, Color.yellow * 0.7f);

            public bool showEdgeIndex = false;

            public bool showEdgeGroup = false;

            // 接続先の無いレーンを表示する
            public bool showNoTrackBorder = false;

            protected override IEnumerable<Drawer<RnIntersection>> GetChildDrawers()
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
                    var n = intersection.Edges[i];

                    void DrawEdge(WayOption p)
                    {
                        if (p.visible == false)
                            return;
                        work.Self.DrawWay(n.Border, p);
                        var pos = n.Border.GetLerpPoint(0.5f);
                        if (showEdgeIndex)
                            work.Self.DrawString($"B[{i}]", pos);
                    }

                    if (n.IsBorder)
                    {
                        DrawEdge(n.IsMedianBorder ? showMedianBorderEdge : showBorderEdge);
                    }
                    else
                    {
                        DrawEdge(showNonBorderEdge);
                    }
                }

                if (showNoTrackBorder)
                {
                    foreach (var border in intersection.Neighbors)
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
                            foreach (var lane in r.AllLanes)
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
                        foreach (var lane in road.AllLanes)
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

            protected override IEnumerable<Drawer<RnRoad>> GetChildDrawers()
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
                if (self.IsReverse)
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

        /// <summary>
        /// Drawの最初でリセットされるフレーム情報
        /// </summary>
        public class DrawWork
        {
            public HashSet<object> Visited { get; } = new();

            public RnModel Model { get; set; }

            public int DrawRoadGroupCount { get; set; }

            public VisibleType visibleType = VisibleType.Empty;

            public PLATEAURnModelDrawerDebug Self { get; set; }

            public DrawWork(PLATEAURnModelDrawerDebug self, RnModel model)
            {
                Self = self;
                Model = model;
            }

            public bool IsVisited(object obj)
            {
                var ret = Visited.Contains(obj);
                if (ret == false)
                    Visited.Add(obj);
                return true;
            }
        }

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
            op.Draw(work, way, work.visibleType);
        }

        public void DrawSideWalk(RnSideWalk sideWalk, SideWalkOption p, VisibleType visibleType)
        {
            sideWalkRoadOp?.Draw(work, sideWalk, visibleType);
        }

        /// <summary>
        /// Lane描画
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="op"></param>
        /// <param name="visibleType"></param>
        private void DrawLane(RnLane lane, LaneOption op, VisibleType visibleType)
        {
            laneOp?.Draw(work, lane, visibleType);
        }

        private void DrawRoad(RnRoad road, VisibleType visibleType)
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
                DrawRoad(road, VisibleType.NonSelected);
            }
        }

        private void DrawIntersection(RnIntersection intersection, VisibleType visibleType)
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
                DrawIntersection(intersection, VisibleType.NonSelected);
            }
        }

        private void DrawSideWalks(RnModel roadNetwork)
        {
            if (sideWalkRoadOp.visible == false)
                return;
            foreach (var sw in roadNetwork.SideWalks)
            {
                DrawSideWalk(sw, sideWalkRoadOp, VisibleType.NonSelected);
            }
        }

        public void Draw(RnModel roadNetwork)
        {
            if (!visible)
                return;
            if (roadNetwork == null)
                return;

            work = new DrawWork(this, roadNetwork);
            // 先に選択中のものから描画
            foreach (var x in SelectedObjects)
            {
                if (x is RnRoad r)
                {
                    if (r.ParentModel != roadNetwork)
                        continue;
                    DrawRoad(r, VisibleType.GuiSelected);
                }
                else if (x is RnIntersection i)
                {
                    if (i.ParentModel != roadNetwork)
                        continue;
                    DrawIntersection(i, VisibleType.GuiSelected);
                }
                else if (x is RnLane l)
                {
                    DrawLane(l, laneOp, VisibleType.GuiSelected);
                }
                else if (x is RnSideWalk sw)
                {
                    DrawSideWalk(sw, sideWalkRoadOp, VisibleType.GuiSelected);
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