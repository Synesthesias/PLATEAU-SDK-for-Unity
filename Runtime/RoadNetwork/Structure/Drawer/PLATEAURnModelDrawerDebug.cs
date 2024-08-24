using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
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

        [SerializeField] public bool showAll = true;
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


        public HashSet<RnRoad> TargetRoads { get; } = new();
        public HashSet<RnIntersection> TargetIntersections { get; } = new();
        public HashSet<RnLane> TargetLanes { get; } = new();
        public HashSet<RnWay> TargetWays { get; } = new();

        public HashSet<RnSideWalk> TargetSideWalks { get; } = new();

        [Serializable]
        public class TrackOption : DrawOption
        {
            public float splitLength = 3f;
            public TrackOption()
            {
                visible = false;
                color = Color.green;
            }
        }

        [Serializable]
        public class IntersectionOption
        {
            public bool visible = true;
            public VisibleType visibleType = VisibleType.All;
            public DrawOption showTrack = new(true, Color.yellow * 0.7f);

            public DrawOption showBorder = new(true, Color.magenta * 0.7f);

            public TrackOption showSplitTrack = new();
        }
        [SerializeField] public IntersectionOption intersectionOp = new IntersectionOption();

        [Serializable]
        public class RoadOption
        {
            public bool visible = true;
            public VisibleType visibleType = VisibleType.All;
            public bool showMedian = true;
            public bool showLaneConnection = false;
            public bool showRoadGroup = false;
            public bool showSideEdge = false;
            public DrawOption showNextConnection = new DrawOption(false, Color.red);
            public DrawOption showPrevConnection = new DrawOption(false, Color.blue);
        }

        [SerializeField]
        public RoadOption roadOp = new RoadOption();

        [Serializable]
        public class WayOption
        {
            // 法線を表示する
            public bool showNormal = true;

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
                if (self.IsBothConnectedLane)
                    return bothConnectedLaneAlpha;
                if (self.IsValidWay)
                    return validWayAlpha;
                return invalidWayAlpha;
            }
        }
        [SerializeField]
        public LaneOption laneOp = new LaneOption();

        [Serializable]
        public class SideWalkOption : DrawOption
        {

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

            public bool IsVisited(object obj)
            {
                var ret = Visited.Contains(obj);
                if (ret == false)
                    Visited.Add(obj);
                return true;
            }
        }
        DrawWork work = new DrawWork();

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
                if (wayOp.showNormal)
                {
                    DrawLine(v, v + n * 0.3f, color: Color.yellow);
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

            foreach (var p in way.Points)
            {
                DrawPoint(p);
            }
        }

        public void DrawSideWalk(RnSideWalk sideWalk, SideWalkOption p)
        {
            if (sideWalk == null)
                return;
            DrawArrows(sideWalk.Way.Select(v => v), true, color: p.color);
        }

        /// <summary>
        /// Lane描画
        /// </summary>
        /// <param name="lane"></param>
        private void DrawLane(RnLane lane, VisibleType visibleType)
        {
            if (lane == null)
                return;

            if (laneOp.visible == false)
                return;

            if (laneOp.visibleType.HasFlag(visibleType) == false)
                return;

            if (work.IsVisited(lane) == false)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Lane))
                DebugEx.DrawString($"L[{lane.DebugMyId}]", lane.GetCenter());

            if (showAll == false && (TargetLanes?.Any() == true) && TargetLanes?.Contains(lane) == false)
                return;

            var offset = Vector3.up * (lane.DebugMyId % 10);
            if (laneOp.showLeftWay.visible)
            {
                DrawWay(lane.LeftWay, color: laneOp.showLeftWay.color.PutA(laneOp.GetLaneAlpha(lane)));
                if (laneOp.showAttrText && (lane.LeftWay.IsValidOrDefault()))
                    DebugEx.DrawString($"L:{lane.DebugMyId}", lane.LeftWay[0] + offset);
            }


            if (laneOp.showRightWay.visible)
            {
                DrawWay(lane.RightWay, color: laneOp.showRightWay.color.PutA(laneOp.GetLaneAlpha(lane)));
                if (laneOp.showAttrText && (lane.RightWay.IsValidOrDefault()))
                    DebugEx.DrawString($"R:{lane.DebugMyId}", lane.RightWay[0] + offset);
            }

            if (laneOp.showCenterWay.visible)
            {
                var centerWay = lane.CreateCenterWay();
                if (centerWay != null)
                    DrawDashedLines(centerWay, color: laneOp.showCenterWay.color.PutA(laneOp.GetLaneAlpha(lane)));
            }

            if (laneOp.showPrevBorder.visible)
            {
                if (lane.PrevBorder.IsValidOrDefault())
                {
                    var type = lane.GetBorderDir(RnLaneBorderType.Prev);
                    if (laneOp.showAttrText)
                        DebugEx.DrawString($"[{lane.DebugMyId}]prev={type.ToString()}", lane.PrevBorder.Points.Last() + offset, Vector2.up * 100);
                    DrawWay(lane.PrevBorder, color: laneOp.showPrevBorder.color);
                }
            }

            if (laneOp.showNextBorder.visible)
            {
                if (lane.NextBorder.IsValidOrDefault())
                {
                    var type = lane.GetBorderDir(RnLaneBorderType.Next);
                    if (laneOp.showAttrText)
                        DebugEx.DrawString($"[{lane.DebugMyId}]next={type.ToString()}", lane.NextBorder.Points.Last() + offset, Vector2.up * 100);
                    DrawWay(lane.NextBorder, color: laneOp.showNextBorder.color);
                }
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
                visibleType |= VisibleType.SceneSelected;

            if (op.visibleType.HasFlag(visibleType) == false)
                return;

            if (work.IsVisited(road) == false)
                return;

            if (showAll == false && (TargetRoads?.Any() == true) && TargetRoads?.Contains(road) == false)
                return;

            if (targetTran && targetTran != road.TargetTran)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Road))
                DebugEx.DrawString($"R[{road.DebugMyId}]", road.GetCenter());

            if (op.showMedian)
                DrawLane(road.MedianLane, visibleType);

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
                    DrawLane(lane, visibleType);
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
        }

        /// <summary>
        /// Road描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawRoads(RnModel roadNetwork)
        {
            if (roadOp.visible == false)
                return;

            // RoadGroupで描画する場合はGroup全部で同じ色にする
            if (roadOp.showRoadGroup)
            {
                var roadGroups = new List<RnRoadGroup>();
                foreach (var road in roadNetwork.Roads)
                {
                    if (roadGroups.Any(a => a.Roads.Contains(road)) == false)
                    {
                        var group = road.CreateRoadGroupOrDefault();
                        if (group != null)
                        {
                            roadGroups.Add(group);
                        }
                    }
                }

                for (var i = 0; i < roadGroups.Count; i++)
                {
                    var group = roadGroups[i];
                    var color = DebugEx.GetDebugColor(i, roadGroups.Count);
                    foreach (var road in group.Roads)
                    {
                        foreach (var lane in road.AllLanes)
                        {
                            DrawArrows(lane.GetVertices().Select(p => p.Vertex), false, color: color);
                        }
                    }
                }

                return;
            }

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

            if (op.visibleType.HasFlag(visibleType) == false)
                return;

            if (work.IsVisited(intersection) == false)
                return;

            if (targetTran && targetTran != intersection.TargetTran)
                return;

            if (showAll == false && TargetIntersections?.Any() == true && TargetIntersections?.Contains(intersection) == false)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Intersection))
                DebugEx.DrawString($"N[{intersection.DebugMyId}]", intersection.GetCenter());

            for (var i = 0; i < intersection.Neighbors.Count; ++i)
            {
                var n = intersection.Neighbors[i];
                if (op.showBorder.visible)
                    DrawWay(n.Border, op.showBorder.color);
            }

            foreach (var track in intersection.Tracks)
            {
                if (op.showSplitTrack.visible == false)
                    continue;
                var n = 5;
                DrawArrows(Enumerable.Range(0, n)
                    .Select(i => 1f * i / (n - 1))
                    .Select(t =>
                    {
                        track.Spline.Evaluate(t, out var pos, out var tam, out var up);
                        return (Vector3)pos;
                    }), false, color: op.showSplitTrack.color);
            }

            if (op.showTrack.visible)
            {
                foreach (var l in intersection.Lanes)
                {
                    foreach (var w in l.BothWays)
                        DrawWay(w, op.showTrack.color);
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
            foreach (var sw in roadNetwork.SideWalks)
            {
                if (sideWalkRoadOp.visible == false)
                    break;
                DrawSideWalk(sw, sideWalkRoadOp);
            }
        }

        public void Draw(RnModel roadNetwork)
        {
            if (!visible)
                return;
            if (roadNetwork == null)
                return;
            work = new DrawWork();

            foreach (var r in TargetRoads)
                DrawRoad(r, VisibleType.GuiSelected);

            foreach (var i in TargetIntersections)
                DrawIntersection(i, VisibleType.GuiSelected);

            foreach (var l in TargetLanes)
                DrawLane(l, VisibleType.GuiSelected);

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