using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Flags]
    public enum RnPartsTypeMask
    {
        Empty = 0,
        Point = 1 << 0,
        LineString = 1 << 1,
        Way = 1 << 2,
        Lane = 1 << 3,
        Link = 1 << 4,
        Node = 1 << 5,
        Neighbor = 1 << 6,
    }

    [Serializable]
    public class RoadNetworkDrawerDebug
    {
        // --------------------
        // start:フィールド
        // --------------------
        [SerializeField] private bool visible = true;
        // Laneの頂点の内側を向くベクトルの中央点を表示する
        [SerializeField] private bool showInsideNormalMidPoint = false;
        // 頂点インデックスを表示する
        [SerializeField] private bool showVertexIndex = false;
        // 頂点の座標を表示する
        [SerializeField] private bool showVertexPos = false;
        // 頂点表示するときのフォントサイズ
        [SerializeField] private int showVertexFontSize = 20;
        // レーン描画するときに法線方向へオフセットを入れる
        [SerializeField] private float edgeOffset = 10f;
        [SerializeField] private bool showSplitLane = false;
        [SerializeField] private float splitLaneRate = 0.5f;
        [SerializeField] private float yScale = 1f;
        [SerializeField] private PLATEAUCityObjectGroup targetTran = null;
        [SerializeField] private RnPartsTypeMask showPartsType = RnPartsTypeMask.Empty;
        [Serializable]
        private class NodeOption
        {
            public bool visible = true;

            public DrawOption showTrack = new DrawOption();

            public DrawOption showBorder = new DrawOption();

            public DrawOption showSplitTrack = new DrawOption();
        }
        [SerializeField] private NodeOption nodeOp = new NodeOption();

        [Serializable]
        private class LinkOption
        {
            public bool visible = true;
            public int showLinkId = -1;
            public bool showMedian = true;
            public bool showLaneConnection = false;
            public bool showLinkGroup = false;
            public bool showSideEdge = false;
            public DrawOption showNextConnection = new DrawOption(false, Color.red);
            public DrawOption showPrevConnection = new DrawOption(false, Color.blue);
            [Serializable]
            public class ShowInfo
            {
                public int prevId = -1;

                public int nextId = -1;

                // 左側レーン数
                public int leftLaneCount = -1;
                // 右側レーン数
                public int rightLaneCount = -1;

                public bool changeLaneCount = false;

                // 中央分離帯幅
                public float medianWidth = 0;

            }
            public ShowInfo targetInfo = new ShowInfo();
        }

        [SerializeField] private LinkOption linkOp = new LinkOption();
        [Serializable]
        private class WayOption
        {
            // 法線を表示する
            public bool showNormal = true;

            // 反転したWayの矢印色
            public Color normalWayArrowColor = Color.yellow;
            // 通常Wayの矢印色
            public Color reverseWayArrowColor = Color.blue;

            public float arrowSize = 0.5f;
        }
        [SerializeField] private WayOption wayOp = new WayOption();

        [Serializable]
        private class LaneOption
        {
            public bool visible = true;
            public int showLaneId = -1;
            public float bothConnectedLaneAlpha = 1f;
            public float validWayAlpha = 0.75f;
            public float invalidWayAlpha = 0.3f;
            public bool showAttrText = false;
            public DrawOption showLeftWay = new DrawOption();
            public DrawOption showRightWay = new DrawOption();
            // 境界線を表示する
            public DrawOption showPrevBorder = new DrawOption();
            public DrawOption showNextBorder = new DrawOption();
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
        [SerializeField] private LaneOption laneOp = new LaneOption();

        [Serializable]
        private class SideWalkOption : DrawOption
        {

        }
        [SerializeField] private SideWalkOption sideWalkRoadOp = new SideWalkOption();

        // --------------------
        // end:フィールド
        // --------------------

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

        /// <summary>
        /// Lane描画
        /// </summary>
        /// <param name="lane"></param>
        private void DrawLane(RnLane lane)
        {
            if (lane == null)
                return;
            if (laneOp.visible == false)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Lane))
                DebugEx.DrawString($"L[{lane.DebugMyId}]", lane.GetCenter());

            if (laneOp.showLaneId >= 0 && lane.DebugMyId != (ulong)laneOp.showLaneId)
                return;

            var offset = Vector3.up * (lane.DebugMyId % 10);
            if (laneOp.showLeftWay.visible)
            {
                DrawWay(lane.LeftWay, color: laneOp.showLeftWay.color.PutA(laneOp.GetLaneAlpha(lane)));
                if (laneOp.showAttrText)
                    DebugEx.DrawString($"L:{lane.DebugMyId}", lane.LeftWay[0] + offset);
            }

            if (laneOp.showRightWay.visible)
            {
                DrawWay(lane.RightWay, color: laneOp.showRightWay.color.PutA(laneOp.GetLaneAlpha(lane)));
                if (laneOp.showAttrText)
                    DebugEx.DrawString($"R:{lane.DebugMyId}", lane.RightWay[0] + offset);
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

        private void DrawLink(RnLink link)
        {
            if ((ulong)linkOp.showLinkId == link.DebugMyId)
            {
                linkOp.targetInfo.prevId = (int)(link.Prev?.DebugMyId ?? ulong.MaxValue);
                linkOp.targetInfo.nextId = (int)(link.Next?.DebugMyId ?? ulong.MaxValue);
                linkOp.targetInfo.leftLaneCount = link.GetLeftLaneCount();
                linkOp.targetInfo.rightLaneCount = link.GetRightLaneCount();
            }

            if (linkOp.visible == false)
                return;
            if (linkOp.showLinkId >= 0 && link.DebugMyId != (ulong)linkOp.showLinkId)
                return;

            if (targetTran && targetTran != link.TargetTran)
                return;

            if (showPartsType.HasFlag(RnPartsTypeMask.Link))
                DebugEx.DrawString($"L[{link.DebugMyId}]", link.GetCenter());

            if (linkOp.showMedian)
                DrawLane(link.MedianLane);

            void DrawLinkConnection(DrawOption op, RnRoadBase target)
            {
                if (op.visible == false)
                    return;
                if (target == null)
                    return;
                var from = link.GetCenter();
                var to = target.GetCenter();
                DrawArrow(from, to, bodyColor: op.color);
            }

            DrawLinkConnection(linkOp.showNextConnection, link.Next);
            DrawLinkConnection(linkOp.showPrevConnection, link.Prev);

            Vector3? last = null;
            foreach (var lane in link.AllLanes)
            {
                DrawLane(lane);
                if (linkOp.showLaneConnection)
                {
                    if (last != null)
                    {
                        DrawArrow(last.Value, lane.GetCenter());
                    }

                    last = lane.GetCenter();
                }
            }

            if (linkOp.showSideEdge)
            {
                DrawWay(link.GetMergedSideWay(RnDir.Left), Color.red);
                DrawWay(link.GetMergedSideWay(RnDir.Right), Color.blue);
            }
        }

        /// <summary>
        /// Link描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawLinks(RnModel roadNetwork)
        {
            if (linkOp.visible == false)
                return;

            // LinkGroupで描画する場合はGroup全部で同じ色にする
            if (linkOp.showLinkGroup)
            {
                var linkGroups = new List<RnLinkGroup>();
                foreach (var link in roadNetwork.Links)
                {
                    if (linkGroups.Any(a => a.Links.Contains(link)) == false)
                    {
                        var group = link.CreateLinkGroupOrDefault();
                        if (group != null)
                        {
                            linkGroups.Add(group);
                        }
                    }
                }

                for (var i = 0; i < linkGroups.Count; i++)
                {
                    var group = linkGroups[i];
                    var color = DebugEx.GetDebugColor(i, linkGroups.Count);
                    foreach (var link in group.Links)
                    {
                        foreach (var lane in link.AllLanes)
                        {
                            DrawArrows(lane.GetVertices().Select(p => p.Vertex), false, color: color);
                        }
                    }
                }

                return;
            }

            foreach (var link in roadNetwork.Links)
            {
                DrawLink(link);
            }
        }

        private void DrawNode(RnNode node)
        {
            if (nodeOp.visible == false)
                return;

            if (targetTran && targetTran != node.TargetTran)
                return;

            for (var i = 0; i < node.Neighbors.Count; ++i)
            {
                var n = node.Neighbors[i];
                if (nodeOp.showBorder.visible)
                    DrawWay(n.Border, nodeOp.showBorder.color);

                if (nodeOp.showSplitTrack.visible)
                {
                    for (var j = i + 1; j < node.Neighbors.Count; ++j)
                    {
                        var n2 = node.Neighbors[j];
                        if (n == n2)
                            continue;
                        var way = node.CalcTrackWay(n.Link, n2.Link);
                        if (way != null)
                        {
                            foreach (var w in way.BothWays)
                                DrawWay(w, nodeOp.showSplitTrack.color);
                        }
                    }
                }
            }

            if (nodeOp.showTrack.visible)
            {
                foreach (var l in node.Lanes)
                {
                    foreach (var w in l.BothWays)
                        DrawWay(w, nodeOp.showTrack.color);
                }
            }
        }

        /// <summary>
        /// Node描画
        /// </summary>
        /// <param name="roadNetwork"></param>
        private void DrawNodes(RnModel roadNetwork)
        {
            if (nodeOp.visible == false)
                return;

            foreach (var node in roadNetwork.Nodes)
            {
                DrawNode(node);
            }
        }

        private void DrawSideWalks(RnModel roadNetwork)
        {
            foreach (var sw in roadNetwork.SideWalks)
            {
                if (sideWalkRoadOp.visible == false)
                    break;
                DrawWay(new RnWay(sw), color: sideWalkRoadOp.color);
            }
        }

        public void Draw(RnModel roadNetwork)
        {
            if (!visible)
                return;
            if (roadNetwork == null)
                return;

            DrawLinks(roadNetwork);

            DrawNodes(roadNetwork);
            DrawSideWalks(roadNetwork);
        }
    }
}