using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.Editor.RoadNetwork.RoadNetworkEditingSystem;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    public class EditingSystemGizmos
    {
        private List<System.Action> drawFuncs = new List<Action>();

        private List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();
        private List<List<Vector3>> intersectionConnectionLinePairs2 = new List<List<Vector3>>();
        private Color connectionColor = Color.blue;

        //public List<Vector3> intersections = new List<Vector3>();
        //public Color intersectionColor = Color.green;
        //public float intersectionRadius = 4.0f;

        //public float btnSize = 2.0f;

        public void Update(EditorDataList<EditorData<RnRoadGroup>> linkGroupEditorData)
        {
            // 仮　専用ノード間の繋がりを描画
            if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<LinkGroupEditorData> eConn) == false)
            {
                Assert.IsTrue(false);
                return;
            }
            List<LinkGroupEditorData> connections = eConn.ToList();

            intersectionConnectionLinePairs.Clear();
            var pts = intersectionConnectionLinePairs;
            pts.Capacity = (connections.Count + 3) * 2;
            foreach (var item in connections)
            {
                if (item.CacheRoadPosList == null)
                {
                    item.CacheRoadPosList = new List<Vector3>(item.ConnectionLinks.Count * 2);

                    foreach (var link in item.ConnectionLinks)
                    {
                        var allLanes = link.AllLanes.GetEnumerator();
                        Vector3 prevBorderPos = Vector3.zero;
                        Vector3 nextBorderPos = Vector3.zero;

                        if (allLanes.MoveNext())
                        {
                            var lane = allLanes.Current;
                            var prevpoints = lane.PrevBorder.Points;
                            var nextpoints = lane.NextBorder.Points;
                            if (CalcCenterPos(prevpoints, out prevBorderPos) &&
                                CalcCenterPos(nextpoints, out nextBorderPos))
                            {
                                item.CacheRoadPosList.Add(prevBorderPos);
                                item.CacheRoadPosList.Add(nextBorderPos);
                            }
                        }
                    }
                }
                foreach (var p in item.CacheRoadPosList)
                {
                    pts.Add(p);
                }

            }

            //gizmos.intersectionConnectionLinePairs2.Clear();
            //var pts = gizmos.intersectionConnectionLinePairs2;
            //pts.Capacity = (connections.Count + 3) * 2;
            //foreach (var item in connections)
            //{
            //    foreach (var link in item.ConnectionLinks)
            //    {
            //        foreach (var lane in link.AllLanes)
            //        {
            //            lane.NextBorder.LineString.Points
            //        }
            //        pts.Add();

            //    }
            //    //Debug.Log(item.A.RefGameObject.name + "." + item.B.RefGameObject.name);
            //}

            intersectionConnectionLinePairs = pts;
        }


        public List<Action> BuildDrawCommands()
        {
            drawFuncs.Clear();

            var nParis = intersectionConnectionLinePairs.Count;
            if (nParis >= 2 && nParis % 2 == 0)
            {
                drawFuncs.Add(() =>
                {
                    Gizmos.color = connectionColor;
                    //Handles.DrawLines(linePairs);
                    Gizmos.DrawLineList(intersectionConnectionLinePairs.ToArray());
                });
            }

            var nParis2 = intersectionConnectionLinePairs2.Count;
            if (nParis2 >= 2)
            {
                drawFuncs.Add(() =>
                {
                    Gizmos.color = connectionColor;
                    foreach (var item in intersectionConnectionLinePairs2)
                    {
                        Gizmos.DrawLineList(item.ToArray());
                    }
                    //Handles.DrawLines(linePairs);
                });
            }

            //    var ee = intersectionConnectionLinePairs.GetEnumerator();
            //    while (ee.MoveNext())
            //    {
            //        var p1 = ee.Current;
            //        if (ee.MoveNext())
            //        {
            //            var p2 = ee.Current;
            //            var btnP = (p1 + p2) / 2.0f;
            //            if (Handles.Button(btnP, Quaternion.identity, btnSize, btnSize, Handles.SphereHandleCap)){
            //                Debug.Log("Button Clicked");
            //            }
            //        }
            //    }

            //    if (intersections.Count > 0)
            //    {
            //        Gizmos.color = intersectionColor;
            //        foreach (var i in intersections)
            //        {
            //            Gizmos.DrawSphere(i, intersectionRadius);
            //        }
            //    }

            return drawFuncs;
        }

        private static bool CalcCenterPos(IEnumerable<RnPoint> points, out UnityEngine.Vector3 v)
        {
            var nP = points.Count();
            if (nP <= 0)
            {
                v = Vector3.zero;
                return false;
            }
            var sum = Vector3.zero;
            foreach (var p in points)
            {
                sum += p.Vertex;
            }
            var borderPos = sum / nP;
            v = borderPos;
            return true;
        }


    }
}
