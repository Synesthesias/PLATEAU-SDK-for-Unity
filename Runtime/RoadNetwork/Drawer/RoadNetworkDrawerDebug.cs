using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Drawer
{
    [Serializable]
    public class RoadNetworkDrawerDebug
    {
        [SerializeField] private bool showNormal = true;

        [SerializeField] private bool showBorder = true;

        // Laneの頂点の内側を向くベクトルの中央点を表示する
        [SerializeField] private bool showInsideNormalMidPoint = false;

        //[SerializeField] private bool showVertexIndex = false;
        // 頂点の座標を表示する
        [SerializeField] private bool showVertexPos = false;
        [SerializeField] private int showVertexFontSize = 20;

        [SerializeField] private float edgeOffset = 10f;

        [SerializeField] private bool showSplitLane = false;
        [SerializeField] private bool showSplitLane2 = false;
        [SerializeField] private float splitLaneRate = 0.5f;

        private static Color GetEdgeColor(RoadNetworkLane self)
        {
            if (self.IsBothConnectedLane)
                return Color.green;
            if (self.IsValidWay)
                return new Color(0.5f, 1f, 0f);
            return new Color(0f, 1f, 0.5f);
        }

        public void Draw(RoadNetworkModel roadNetwork)
        {
            if (roadNetwork == null)
                return;

            // 道描画

            void DrawWay(RoadNetworkWay way, Color color, Color arrowColor)
            {
                if (way == null)
                    return;

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
            }

            foreach (var node in roadNetwork.Nodes)
            {
                var center = node.GetCenterPoint();
                Debug.DrawLine(center, center + Vector3.up);

                foreach (var n in node.Neighbors)
                {
                    DrawWay(n.Border, Color.magenta, Color.magenta);
                    n.Border.GetLerpPoint(0.5f, out var c);
                    Debug.DrawLine(center, c, color: Color.red);
                }

                foreach (var l in node.Tracks)
                {
                    foreach (var w in l.BothWays)
                        DrawWay(w, Color.cyan, Color.cyan);
                }
            }

            foreach (var link in roadNetwork.Links)
            {
                foreach (var lane in link.AllLanes)
                {
                    foreach (var way in lane.BothWays)
                    {
                        DrawWay(way, color: GetEdgeColor(lane), arrowColor: way.IsReversed ? Color.cyan : Color.blue);

                        foreach (var i in Enumerable.Range(0, way.Count))
                        {
                            var v = way[i];
                            var n = way.GetVertexNormal(i);
                            // 法線表示
                            if (showNormal)
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