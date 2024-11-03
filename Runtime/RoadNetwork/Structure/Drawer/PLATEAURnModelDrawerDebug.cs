using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.RoadNetwork.Voronoi;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        [SerializeField] public bool showSplitLane = false;
        [SerializeField] public float splitLaneRate = 0.5f;
        [SerializeField] public float yScale = 1f;
        [SerializeField] public PLATEAUCityObjectGroup targetTran = null;
        [SerializeField] public RnPartsTypeMask showPartsType = 0;

        // 非表示オブジェクト
        public HashSet<object> InVisibleObjects { get; } = new();

        // エディタで選択されたオブジェクト
        public HashSet<object> SelectedObjects { get; } = new();

        [Serializable]
        public class IntersectionOption
        {
            [Serializable]
            public class DrawTrackOption : DrawOption
            {
                public bool useTurnTypeColor = false;

                public Color disConnectedColor = Color.red;

                // 接続先の道路タイプを表示する
                public bool showConnectedRoadType = false;

                // スプライン描画するときに何m間隔で描画するか
                public float drawSplineInterval = 3f;

                // スプラインのノットで描画する
                public bool showKnots = false;
                public DrawTrackOption()
                {
                    visible = true;
                    color = Color.yellow * 0.7f;
                }
            }

            public bool visible = true;
            public VisibleType visibleType = VisibleType.All;
            public DrawTrackOption showTrack = new();

            // 非境界線の表示オプション
            public DrawOption showNonBorderEdge = new(true, Color.magenta * 0.7f);

            // 境界線表示オプション
            public DrawOption showBorderEdge = new(true, Color.cyan * 0.7f);

            // 中央分離帯との境界線オプション
            public DrawOption showMedianBorderEdge = new(true, Color.yellow * 0.7f);

            public bool showEdgeIndex = false;

            public bool showEdgeGroup = false;

            public bool showRecLine = false;
            public bool showRecLine2 = false;
            public float showRecLineRefineInterval = 5f;
            public float showRecLineHalfLineLength = 10f;

            public int showRecLineNest = 3;
        }
        [SerializeField] public IntersectionOption intersectionOp = new IntersectionOption();

        [Serializable]
        public class RoadGroupOption : DrawOption
        {
            public bool showSpline = false;
            public bool showSplineKnot = false;
            public float pointSkipDistance = 1e-3f;
        }

        [Serializable]
        public class RoadOption
        {
            public bool visible = true;
            public VisibleType visibleType = VisibleType.All;
            public bool showMedian = true;
            public bool showLaneConnection = false;
            public RoadGroupOption showRoadGroup = new RoadGroupOption { visible = false, showSpline = true, color = Color.green };
            public bool showSideEdge = false;
            public bool showEmptyRoadLabel = false;
            public DrawOption showNextConnection = new DrawOption(false, Color.red);
            public DrawOption showPrevConnection = new DrawOption(false, Color.blue);
        }

        [SerializeField]
        public RoadOption roadOp = new RoadOption();

        [Serializable]
        public class WayOption
        {
            // 頂点の法線を表示する
            public bool showVertexNormal = true;

            // 線の法線を表示する
            public bool showEdgeNormal = false;

            // 反転したWayの矢印色
            public Color normalWayArrowColor = Color.yellow;
            // 通常Wayの矢印色
            public Color reverseWayArrowColor = Color.blue;

            public float arrowSize = 0.5f;
        }
        [SerializeField] public WayOption wayOp = new WayOption();

        [Serializable]
        public class LaneOption
        {
            public bool visible = true;
            public VisibleType visibleType = VisibleType.All;
            public float bothConnectedLaneAlpha = 1f;
            public float validWayAlpha = 0.75f;
            public float invalidWayAlpha = 0.3f;
            public bool showAttrText = false;
            public float reverseWayAlpha = 1f;
            public DrawOption showLeftWay = new DrawOption(true, Color.red);
            public DrawOption showRightWay = new DrawOption(true, Color.blue);
            // 境界線を表示する
            public DrawOption showPrevBorder = new DrawOption(true, Color.green);
            public DrawOption showNextBorder = new DrawOption(true, Color.green);
            public DrawOption showCenterWay = new DrawOption(true, Color.green * 0.5f);
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
        }
        [SerializeField]
        public LaneOption laneOp = new LaneOption();

        [SerializeField]
        public LaneOption medianLaneOp = new LaneOption();

        [Serializable]
        public class SideWalkOption
        {
            public bool visible = true;
            public DrawOption showOutsideWay = new DrawOption(true, Color.red);
            public DrawOption showInsideWay = new DrawOption(true, Color.blue);
            public DrawOption showStartEdgeWay = new DrawOption(true, Color.green);
            public DrawOption showEndEdgeWay = new DrawOption(true, Color.yellow);
            // Noneの時はすべて表示. それ以外はRnSideWalk.GetValidWayTypeMaskが一致したものだけ表示する(不正なSideWalk検出用)
            public RnSideWalkWayTypeMask showWayFilter = RnSideWalkWayTypeMask.None;
        }
        [SerializeField] public SideWalkOption sideWalkRoadOp = new SideWalkOption();

        // --------------------
        // end:フィールド
        // --------------------

        /// <summary>
        /// Drawの最初でリセットされるフレーム情報
        /// </summary>
        class DrawWork
        {
            public HashSet<object> Visited { get; } = new();

            public RnModel Model { get; set; }

            public int DrawRoadGroupCount { get; set; }

            public DrawWork(RnModel model)
            {
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
        DrawWork work = new DrawWork(null);

        private void DrawArrows(IEnumerable<Vector3> vertices
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
        private void DrawWay(RnWay way, Color color, Color? arrowColor = null)
        {
            if (way == null)
                return;

            // 非表示設定
            if (InVisibleObjects.Contains(way))
                return;

            if (way.Count <= 1)
                return;
            // 矢印色は設定されていない場合は反転しているかどうかで返る
            if (arrowColor.HasValue)
                arrowColor = way.IsReversed ? wayOp.reverseWayArrowColor : wayOp.normalWayArrowColor;

            DrawArrows(way.Vertices.Select((v, i) => v + -edgeOffset * way.GetVertexNormal(i)), false, color: color, arrowColor: arrowColor, arrowSize: wayOp.arrowSize);

            if (showVertexIndex)
            {
                foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                    DrawString(item.i.ToString(), item.v, color: Color.red, fontSize: showVertexFontSize);
            }

            if (showVertexPos)
            {
                foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                    DrawString(item.v.ToString(), item.v, color: Color.red, fontSize: showVertexFontSize);
            }

            foreach (var i in Enumerable.Range(0, way.Count))
            {
                var v = way[i];
                var n = way.GetVertexNormal(i);

                // 法線表示
                if (wayOp.showVertexNormal)
                {
                    DrawLine(v, v + n * 0.3f, color: Color.yellow);
                }

                if (wayOp.showEdgeNormal && i < way.Count - 1)
                {
                    var p = (v + way[i + 1]) * 0.5f;
                    var nn = way.GetEdgeNormal(i);
                    DrawArrow(p, p + nn, bodyColor: Color.blue);
                }

                // 中央線
                if (showInsideNormalMidPoint)
                {
                    if (way.HalfLineIntersectionXz(new Ray(v - n * 0.01f, -n), out var intersection))
                    {
                        DrawArrow(v, (v + intersection) * 0.5f);
                    }
                }
            }

            if (showPartsType.HasFlag(RnPartsTypeMask.Way))
            {
                way.GetLerpPoint(0.5f, out var p);
                DebugEx.DrawString($"{way.GetDebugIdLabelOrDefault()}", p);
            }

            foreach (var p in way.Points)
            {
                DrawPoint(p);
            }
        }

        public void DrawSideWalk(RnSideWalk sideWalk, SideWalkOption p)
        {
            if (sideWalk == null)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.SideWalk))
                DebugEx.DrawString($"S[{sideWalk.DebugMyId}]", sideWalk.GetCenter());

            // 一致判定
            if (p.showWayFilter != RnSideWalkWayTypeMask.None && sideWalk.GetValidWayTypeMask() != p.showWayFilter)
                return;

            // 非表示設定
            if (InVisibleObjects.Contains(sideWalk))
                return;

            void DrawSideWalkWay(RnWay way, DrawOption op)
            {
                if (op.visible == false)
                    return;
                DrawWay(way, op.color);
            }
            DrawSideWalkWay(sideWalk.OutsideWay, p.showOutsideWay);
            DrawSideWalkWay(sideWalk.InsideWay, p.showInsideWay);
            DrawSideWalkWay(sideWalk.StartEdgeWay, p.showStartEdgeWay);
            DrawSideWalkWay(sideWalk.EndEdgeWay, p.showEndEdgeWay);
        }

        /// <summary>
        /// Lane描画
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="op"></param>
        /// <param name="visibleType"></param>
        private void DrawLane(RnLane lane, LaneOption op, VisibleType visibleType)
        {
            if (lane == null)
                return;

            if (op.visible == false)
                return;

            if ((visibleType & op.visibleType) == 0)
                return;

            if (work.IsVisited(lane) == false)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Lane))
                DebugEx.DrawString($"L[{lane.DebugMyId}]", lane.GetCenter());

            // 非表示設定
            if (InVisibleObjects.Contains(lane))
                return;

            var offset = Vector3.up * (lane.DebugMyId % 10);
            if (op.showLeftWay.visible)
            {
                DrawWay(lane.LeftWay, color: op.showLeftWay.color.PutA(op.GetLaneAlpha(lane)));
                if (op.showAttrText && (lane.LeftWay.IsValidOrDefault()))
                    DebugEx.DrawString($"L:{lane.DebugMyId}", lane.LeftWay[0] + offset);
            }

            if (op.showRightWay.visible)
            {
                DrawWay(lane.RightWay, color: op.showRightWay.color.PutA(op.GetLaneAlpha(lane)));
                if (op.showAttrText && (lane.RightWay.IsValidOrDefault()))
                    DebugEx.DrawString($"R:{lane.DebugMyId}", lane.RightWay[0] + offset);
            }

            if (op.showCenterWay.visible)
            {
                var centerWay = lane.CreateCenterWay();
                if (centerWay != null)
                    DrawDashedArrows(centerWay, color: op.showCenterWay.color.PutA(op.GetLaneAlpha(lane)));
            }

            if (op.showPrevBorder.visible)
            {
                if (lane.PrevBorder.IsValidOrDefault())
                {
                    var type = lane.GetBorderDir(RnLaneBorderType.Prev);
                    if (op.showAttrText)
                        DebugEx.DrawString($"[{lane.DebugMyId}]prev={type.ToString()}", lane.PrevBorder.Points.Last() + offset, Vector2.up * 100);
                    DrawWay(lane.PrevBorder, color: op.showPrevBorder.color);
                }
            }

            if (op.showNextBorder.visible)
            {
                if (lane.NextBorder.IsValidOrDefault())
                {
                    var type = lane.GetBorderDir(RnLaneBorderType.Next);
                    if (op.showAttrText)
                        DebugEx.DrawString($"[{lane.DebugMyId}]next={type.ToString()}", lane.NextBorder.Points.Last() + offset, Vector2.up * 100);
                    DrawWay(lane.NextBorder, color: op.showNextBorder.color);
                }
            }

            if (lane.NextBorder != null && lane.PrevBorder != null && lane.NextBorder.IsSameLine(lane.PrevBorder))
            {
                DebugEx.DrawString($"Invalid Border Lane ", lane.GetCenter());
            }

            if (showSplitLane && lane.HasBothBorder)
            {
                var vers = lane.GetInnerLerpSegments(splitLaneRate);
                DrawArrows(vers, false, color: Color.red, arrowSize: 0.1f);
            }
        }

        private void DrawRoad(RnRoad road, VisibleType visibleType)
        {
            var op = roadOp;

            if (op.visible == false)
                return;

            if (RnEx.IsEditorSceneSelected(road.CityObjectGroup))
            {
                visibleType |= VisibleType.SceneSelected;
                visibleType &= ~VisibleType.NonSelected;
            }

            if ((visibleType & op.visibleType) == 0)
                return;

            if (work.IsVisited(road) == false)
                return;

            if (targetTran && targetTran != road.TargetTran)
                return;

            // 非表示設定
            if (InVisibleObjects.Contains(road))
                return;

            // RoadGroupで描画する場合はGroup全部で同じ色にする
            if (roadOp.showRoadGroup.visible)
            {
                var group = road.CreateRoadGroupOrDefault();
                foreach (var r in group.Roads)
                    work.Visited.Add(r);

                if (roadOp.showRoadGroup.showSpline && group.TryCreateSpline(out var spline, out var width, pointSkipDistance: roadOp.showRoadGroup.pointSkipDistance))
                {
                    var n = spline.Count;
                    if (roadOp.showRoadGroup.showSplineKnot)
                    {
                        //foreach (var knot in spline.Knots.Select((v, i) => new { v, i }))
                        //{
                        //    DrawString(knot.i.ToString(), knot.v.Position);
                        //}
                        DrawArrows(spline.Knots.Select(knot => (Vector3)(knot.Position)), false, color: roadOp.showRoadGroup.color, arrowSize: 1f);
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
                        DrawArrows(points.Select(p => p.pos), false, color: roadOp.showRoadGroup.color, arrowSize: 1f);
                        DrawArrows(points.Select(p => p.pos + 0.5f * p.n * width), false, color: roadOp.showRoadGroup.color, arrowSize: 1f);
                        DrawArrows(points.Select(p => p.pos - 0.5f * p.n * width), false, color: roadOp.showRoadGroup.color, arrowSize: 1f);
                    }
                }
                else
                {
                    var color = DebugEx.GetDebugColor(work.DrawRoadGroupCount++, 16);
                    foreach (var r in group.Roads)
                    {
                        foreach (var lane in r.AllLanes)
                        {
                            DrawArrows(lane.GetVertices().Select(p => p.Vertex), false, color: color);
                        }
                    }
                }

                return;
            }

            if (showPartsType.HasFlag(RnPartsTypeMask.Road) || (op.showEmptyRoadLabel && road.IsEmptyRoad))
                DebugEx.DrawString($"R[{road.DebugMyId}]", road.GetCenter());

            void DrawRoadConnection(DrawOption op, RnRoadBase target)
            {
                if (op.visible == false)
                    return;
                if (target == null)
                    return;
                var from = road.GetCenter();
                var to = target.GetCenter();
                DrawArrow(from, to, bodyColor: op.color);
            }

            DrawRoadConnection(op.showNextConnection, road.Next);
            DrawRoadConnection(op.showPrevConnection, road.Prev);


            if (op.showSideEdge)
            {
                DrawWay(road.GetMergedSideWay(RnDir.Left), laneOp.showLeftWay.color);
                DrawWay(road.GetMergedSideWay(RnDir.Right), laneOp.showRightWay.color);
                DrawWay(road.GetMergedBorder(RnLaneBorderType.Prev), laneOp.showPrevBorder.color);
                DrawWay(road.GetMergedBorder(RnLaneBorderType.Next), laneOp.showNextBorder.color);
            }
            else
            {
                Vector3? last = null;
                foreach (var lane in road.AllLanes)
                {
                    DrawLane(lane, laneOp, visibleType);
                    if (op.showLaneConnection)
                    {
                        if (last != null)
                        {
                            DrawArrow(last.Value, lane.GetCenter());
                        }

                        last = lane.GetCenter();
                    }
                }
            }

            DrawLane(road.MedianLane, medianLaneOp, visibleType);
        }

        /// <summary>
        /// Road描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawRoads(RnModel roadNetwork)
        {
            if (roadOp.visible == false)
                return;

            foreach (var road in roadNetwork.Roads)
            {
                DrawRoad(road, VisibleType.NonSelected);
            }
        }

        private void DrawIntersection(RnIntersection intersection, VisibleType visibleType)
        {
            var op = intersectionOp;
            if (op.visible == false)
                return;

            if (RnEx.IsEditorSceneSelected(intersection.CityObjectGroup))
                visibleType |= VisibleType.SceneSelected;

            if ((visibleType & op.visibleType) == 0)
                return;

            if (work.IsVisited(intersection) == false)
                return;

            if (targetTran && targetTran != intersection.TargetTran)
                return;

            // 非表示設定
            if (InVisibleObjects.Contains(intersection))
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Intersection))
                DebugEx.DrawString($"N[{intersection.DebugMyId}]", intersection.GetCenter());

            if (op.showEdgeGroup)
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
                        DrawWay(n.Border, eg.IsBorder ? color : Color.white);
                        DrawString($"E[{x++}]", n.Border.GetLerpPoint(0.5f));
                    }
                }

                return;
            }

            if (op.showRecLine2)
            {
                var edges = intersection.Edges.Where(e => e.IsBorder == false);
                var vs =
                    edges.SelectMany(e => e.Border.LineString.Refined(op.showRecLineRefineInterval).Points.Select(p => new { e = e, p = p, v = p.Vertex.Xz() })).ToList();
                var voronoiData = Voronoi.RnVoronoiEx.CalcVoronoiData(vs, v => new Vector2d(v.v));

                //for (var i = 0; i < vertices.Count; ++i)
                //{
                //    var v = vertices[i];
                //    DebugEx.DrawSphere(v.v, param.sphereSize);
                //    DebugEx.DrawString($"{i}", v.v, color: Color.red, fontSize: 20);
                //}

                Dictionary<Vector3, HashSet<int>> drawn = new();

                void DrawPoint(Vector3 x, int a)
                {
                    if (drawn.TryGetValue(x, out var p) == false)
                    {
                        drawn[x] = new HashSet<int> { a };
                        // DebugEx.DrawSphere(x, param.sphereSize, color: Color.green);
                    }
                    drawn[x].Add(a);
                }
                var colors = new List<int>();
                Dictionary<Vector3, int> edgeCount = new();
                var childIndex = 0;
                var plane = AxisPlane.Xz;

                Dictionary<RnNeighbor, Dictionary<RnNeighbor, List<int>>> pass = new();

                var edgeStartIndices = voronoiData.Edges.Take(0)
                    .ToDictionary(x => (RnRoadBase)null, x => x);
                foreach (var from in intersection.CreateEdgeGroup())
                {
                    if (from.Key == null)
                        continue;

                    var centroid = Vector2Ex.Centroid(from.Edges.Select(e => e.Border.GetLerpPoint(0.5f).Xz()));


                    var f = voronoiData.Edges.Where(e =>
                    {
                        if (e.LeftSitePoint.e != from.LeftSide.Edges[0] && e.RightSitePoint.e != from.LeftSide.Edges[0])
                        {
                            return false;
                        }

                        if (e.LeftSitePoint.e != from.RightSide.Edges[0] && e.RightSitePoint.e != from.RightSide.Edges[0])
                        {
                            return false;
                        }

                        return e.Start.HasValue;
                    }).TryFindMin(v => (v.Start.Value.ToVector2() - centroid).sqrMagnitude, out var x);
                    edgeStartIndices[from.Key] = x;
                    if (x != null)
                        DebugEx.DrawSphere(x.Start.Value.ToVector2().ToVector3(plane), 2f, color: Color.cyan);
                }

                foreach (var e in voronoiData.Edges)
                {
                    var color = DebugEx.GetDebugColor(childIndex++, 16);

                    if (/*param.showAllEdge == false &&*/ e.LeftSitePoint.e == e.RightSitePoint.e)
                        continue;


                    var c = e.LeftSitePoint.e.GetHashCode() ^ e.RightSitePoint.e.GetHashCode();
                    var index = colors.IndexOf(c);
                    if (index < 0)
                    {
                        index = colors.Count;
                        colors.Add(c);
                    }
                    color = DebugEx.GetDebugColor(index, 16);

                    // 完全な直線の場合サイトポイントの中間点(2等分線だから)
                    var mid = new Vector2d(((e.LeftSitePoint.v + e.RightSitePoint.v) * 0.5f));
                    var st = Vector3.zero;
                    var en = Vector3.zero;
                    var d = e.Direction * op.showRecLineHalfLineLength;
                    if (e.Start == null && e.End == null)
                    {
                        st = (mid - d).ToVector2().ToVector3(plane);
                        en = (mid + d).ToVector2().ToVector3(plane);
                    }
                    else
                    {
                        st = (e.Start ?? (e.End.Value - d)).ToVector2().ToVector3(plane);
                        en = (e.End ?? (e.Start.Value + d)).ToVector2().ToVector3(plane);

                    }

                    DebugEx.DrawLine(st, en, color);
                    var p = (st + en) * 0.5f;
                    var n = (en - st).ToVector2(plane).normalized.Rotate(90).ToVector3(plane) * 0.1f;

                    var x = edgeCount.TryGetValue(p, out var l) ? l + 1 : 1;
                    edgeCount[p] = x;
                    //DebugEx.DrawString($"{e.LeftSiteIndex}", p + n * x, color: Color.blue, fontSize: 20);
                    //DebugEx.DrawString($"{e.RightSiteIndex}", p - n * x, color: Color.blue, fontSize: 20);

                    DrawPoint(st, 0);
                    DrawPoint(en, 0);
                    //DebugEx.DrawString($"E", en);
                    //DebugEx.DrawString($"S", st);
                }
                return;
            }

            if (op.showRecLine)
            {
                var rec = intersection.CreateRecLine();

                for (var i = 0; i < rec.Count; ++i)
                {
                    var color = DebugEx.GetDebugColor(i, rec.Count);
                    void Draw(RnIntersectionEx.RecLine r, int no)
                    {
                        if (r.LeftSide != null)
                        {
                            var left = r.LeftSide.ToWork();
                            DrawArrows(left.Select(l => l.Vertex), color: color + Color.red);
                        }

                        if (r.RightSide != null)
                        {
                            var right = r.RightSide.ToWork();
                            DrawArrows(right.Select(l => l.Vertex), color: color + Color.blue);
                        }

                        foreach (var x in r.Lines)
                        {
                            DrawWay(x, color: color);
                            DrawString($"{no}", x.GetLerpPoint(0.5f));
                        }
                    }
                    var r = rec[i];


                    if (r.LeftSide != null && r.RightSide != null)
                    {
                        var left = r.LeftSide.Way.ToList();
                        var right = r.RightSide.Way.ToList();
                        var x = GeoGraphEx.GetInnerLerpSegments(left, right, AxisPlane.Xz, 0.5f);
                        // DrawArrows(x, color: color + Color.green);
                    }
                    Draw(r, 0);
                    for (var j = 0; j < op.showRecLineNest; ++j)
                    {
                        try
                        {
                            r = r.CreateChild();
                            Draw(r, j + 1);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            break;
                        }
                    }
                }

                return;
            }

            //foreach (var n in intersection.Edges)
            for (var i = 0; i < intersection.Edges.Count; i++)
            {
                var n = intersection.Edges[i];

                void Draw(DrawOption p)
                {
                    if (p.visible == false)
                        return;
                    DrawWay(n.Border, p.color);
                    var pos = n.Border.GetLerpPoint(0.5f);
                    if (op.showEdgeIndex)
                        DrawString($"B[{i}]", pos);
                }

                if (n.IsBorder)
                {
                    Draw(n.IsMedianBorder ? op.showMedianBorderEdge : op.showBorderEdge);
                }
                else
                {
                    Draw(op.showNonBorderEdge);
                }
            }

            if (op.showTrack.visible)
            {
                foreach (var track in intersection.Tracks)
                {

                    var color = op.showTrack.color;
                    if (op.showTrack.useTurnTypeColor)
                    {
                        color = DebugEx.GetDebugColor((int)track.TurnType, RnTurnTypeEx.Count);
                    }

                    Color CheckRoad(RnWay trackBorderWay)
                    {
                        var edge = intersection.FindEdges(trackBorderWay).FirstOrDefault();
                        if (edge?.Road is RnRoad)
                        {
                            if (intersection.GetConnectedLanes(trackBorderWay).Any() == false)
                                return op.showTrack.disConnectedColor;
                        }
                        // 何もなければ変更なし
                        return color;
                    }

                    color = CheckRoad(track.FromBorder);
                    color = CheckRoad(track.ToBorder);
                    if (op.showTrack.showKnots)
                    {
                        DrawArrows(track.Spline.Knots.Select(k => (Vector3)k.Position), false, color: color);
                    }
                    else
                    {
                        var length = GeoGraphEx.GetEdges(track.Spline.Knots.Select(k => k.Position), false)
                            .Sum(x => ((Vector3)(x.Item1) - (Vector3)(x.Item2)).magnitude);
                        var n = Mathf.Max(3, Mathf.FloorToInt(length / Mathf.Max(0.1f, op.showTrack.drawSplineInterval)));

                        DrawArrows(Enumerable.Range(0, n)
                            .Select(i => 1f * i / (n - 1))
                            .Select(t =>
                            {
                                track.Spline.Evaluate(t, out var pos, out var tam, out var up);
                                return (Vector3)pos;
                            }), false, color: color);

                        if (op.showTrack.showConnectedRoadType)
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
            }
        }

        /// <summary>
        /// Intersection描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawIntersections(RnModel roadNetwork)
        {
            if (intersectionOp.visible == false)
                return;

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
                DrawSideWalk(sw, sideWalkRoadOp);
            }
        }

        public void Draw(RnModel roadNetwork)
        {
            if (!visible)
                return;
            if (roadNetwork == null)
                return;
            work = new DrawWork(roadNetwork);

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
                    DrawSideWalk(sw, sideWalkRoadOp);
                }
                else if (x is RnLineString ls)
                {
                    DrawArrows(ls.Points.Select(p => p.Vertex), false, color: wayOp.normalWayArrowColor, arrowSize: wayOp.arrowSize);

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