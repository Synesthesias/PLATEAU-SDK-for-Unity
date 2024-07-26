using PLATEAU.Util;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Serializable]
    public class RoadNetworkDrawerDebug
    {
        // --------------------
        // start:フィールド
        // --------------------
        // 境界線を表示する
        [SerializeField] private bool showBorder = true;
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
        [SerializeField] private bool showSplitLane2 = false;
        [SerializeField] private float splitLaneRate = 0.5f;

        [SerializeField] private bool showNodeNeighbor = false;
        [SerializeField] private bool showNodeTracks = false;

        [Serializable]
        private class WayOption
        {
            // 法線を表示する
            public bool showNormal = true;

            // 反転したWayの矢印色
            public Color normalWayArrowColor = Color.yellow;
            // 通常Wayの矢印色
            public Color reverseWayArrowColor = Color.blue;
        }
        [SerializeField] private WayOption wayOp = new WayOption();

        [Serializable]
        private class LaneOption
        {
            // 左
            public Color leftWayColor = Color.green;
            public Color rightWayColor = Color.green;
            public float bothConnectedLaneAlpha = 1f;
            public float validWayAlpha = 0.75f;
            public float invalidWayAlpha = 0.3f;

            /// <summary>
            /// レーン描画するときのアルファを返す
            /// </summary>
            /// <param name="self"></param>
            /// <returns></returns>
            public float GetLaneAlpha(RoadNetworkLane self)
            {
                if (self.IsBothConnectedLane)
                    return bothConnectedLaneAlpha;
                if (self.IsValidWay)
                    return validWayAlpha;
                return invalidWayAlpha;
            }
        }
        [SerializeField] private LaneOption laneOp = new LaneOption();

        // --------------------
        // end:フィールド
        // --------------------


        public void Draw(RoadNetworkModel roadNetwork)
        {
            if (roadNetwork == null)
                return;

            // 道描画

            void DrawWay(RoadNetworkWay way, Color color, Color? arrowColor = null)
            {
                if (way == null)
                    return;
                if (way.Count <= 1)
                    return;
                // 矢印色は設定されていない場合は反転しているかどうかで返る
                if (arrowColor.HasValue)
                    arrowColor = way.IsReversed ? wayOp.reverseWayArrowColor : wayOp.normalWayArrowColor;

                DebugEx.DrawArrows(way.Vertices.Select((v, i) => v + -edgeOffset * way.GetVertexNormal(i)), false, color: color, arrowColor: arrowColor);

                if (showVertexIndex)
                {
                    foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                        DebugEx.DrawString(item.i.ToString(), item.v, color: Color.red, fontSize: showVertexFontSize);
                }

                if (showVertexPos)
                {
                    foreach (var item in way.Vertices.Select((v, i) => new { v, i }))
                        DebugEx.DrawString(item.v.ToString(), item.v, color: Color.red, fontSize: showVertexFontSize);
                }


                foreach (var i in Enumerable.Range(0, way.Count))
                {
                    var v = way[i];
                    var n = way.GetVertexNormal(i);

                    // 法線表示
                    if (wayOp.showNormal)
                    {
                        Debug.DrawLine(v, v + n * 0.3f, color: Color.yellow);
                    }
                    // 中央線
                    if (showInsideNormalMidPoint)
                    {
                        if (way.HalfLineIntersectionXz(new Ray(v - n * 0.01f, -n), out var intersection))
                        {
                            DebugEx.DrawArrow(v, (v + intersection) * 0.5f);
                        }
                    }
                }
            }

            foreach (var node in roadNetwork.Nodes)
            {
                var center = node.GetCenterPoint();
                Debug.DrawLine(center, center + Vector3.up);

                if (showNodeNeighbor)
                {
                    foreach (var n in node.Neighbors)
                    {
                        DrawWay(n.Border, Color.magenta, Color.magenta);
                        n.Border.GetLerpPoint(0.5f, out var c);
                        Debug.DrawLine(center, c, color: Color.red);
                    }
                }

                if (showNodeTracks)
                {
                    foreach (var l in node.Tracks)
                    {
                        foreach (var w in l.BothWays)
                            DrawWay(w, Color.cyan, Color.cyan);
                    }
                }
            }

            foreach (var link in roadNetwork.Links)
            {
                foreach (var lane in link.AllLanes)
                {
                    DrawWay(lane.LeftWay, color: laneOp.leftWayColor.PutA(laneOp.GetLaneAlpha(lane)));
                    DrawWay(lane.RightWay, color: laneOp.rightWayColor.PutA(laneOp.GetLaneAlpha(lane)));
                    if (showBorder)
                    {
                        DrawWay(lane.PrevBorder, color: Color.blue, arrowColor: Color.blue);
                        DrawWay(lane.NextBorder, color: Color.red, arrowColor: Color.red);

                    }

                    if (showSplitLane && lane.IsBothConnectedLane)
                    {
                        var vers = lane.GetInnerLerpSegments(splitLaneRate);
                        if (showSplitLane2)
                            DebugEx.DrawArrows(vers.Select(v => v.Xay()), false, color: Color.red, arrowSize: 0.1f);
                    }
                }


                //foreach (var i in Enumerable.Range(0, l.vertices.Count))
                //{
                //    var v = l.vertices[i];
                //    var n = l.GetVertexNormal(i).normalized;
                //    if (showNormal)
                //    {
                //        Debug.DrawLine(v, v + n * 0.3f, color: Color.yellow);
                //    }

                //    if (showInsideNormalMidPoint)
                //    {
                //        if (l.HalfLineIntersectionXz(new Ray(v - n * 0.01f, -n), out var intersection))
                //        {
                //            DebugUtil.DrawArrow(v, (v + intersection) * 0.5f);
                //        }
                //    }
                //}


            }
        }
    }
}