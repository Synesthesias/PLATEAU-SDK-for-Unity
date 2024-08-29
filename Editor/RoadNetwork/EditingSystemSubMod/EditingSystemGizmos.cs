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

        private List<Vector3> selectingWay = new List<Vector3>();

        private List<List<Vector3>> selectableWayList = new List<List<Vector3>>();

        private List<List<Vector3>> guideWayList = new List<List<Vector3>>();
        //public List<Vector3> intersections = new List<Vector3>();
        //public Color intersectionColor = Color.green;
        //public float intersectionRadius = 4.0f;

        //public float btnSize = 2.0f;

        private List<List<Vector3>> sideWalks = new List<List<Vector3>>();

        private List<List<Vector3>> slideDummyWayList = new List<List<Vector3>>();

        private List<List<Vector3>> intersectionOutline = new List<List<Vector3>>();
        private List<List<Vector3>> intersectionBorder = new List<List<Vector3>>();

        private readonly Color selectingColorOffset = new Color(0.2f, 0.2f, 0.2f, 0);
        private readonly Color dummyColorOffset = new Color(-0.2f, -0.2f, -0.2f, 0);

        private Color connectionColor = Color.blue;
        private Color selectingWayColor = Color.red;
        private Color selectableWayColor = Color.yellow;
        private Color guideWayColor = Color.black;
        private Color sideWalkColor = Color.grey;
        private Color slideDummyWayColor = Color.black + new Color(-0.2f, -0.2f, -0.2f, 0);

        private Color intersectionOutlineColor = Color.gray;
        private Color intersectionBorderColor = Color.gray + new Color(-0.2f, -0.2f, -0.2f, 0);

        public EditingSystemGizmos()
        {

        }


        public void Update(
            object selectingElement,
            WayEditorData highLightWay,
            EditorDataList<EditorData<RnRoadGroup>> linkGroupEditorData,
            RnWay slideDummyWay)
        {
            if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<LinkGroupEditorData> eConn) == false)
            {
                Assert.IsTrue(false);
                return;
            }
            List<LinkGroupEditorData> connections = eConn.ToList();
            connections.Remove(null);

            // 仮　専用ノード間の繋がりを描画
            intersectionConnectionLinePairs.Clear();
            var pts = intersectionConnectionLinePairs;
            pts.Capacity = (connections.Count + 3) * 2;
            foreach (var item in connections)
            {
                var roadGroup = selectingElement as EditorData<RnRoadGroup>;
                if (item.LinkGroup == roadGroup)
                {
                    continue;
                }
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

            // RoadGroupが選択されている
            selectableWayList.Clear();
            guideWayList.Clear();
            sideWalks.Clear();
            if (selectingElement is EditorData<RnRoadGroup> roadGroupEditorData)
            {
                //// 歩道の描画
                //foreach (var road in roadGroupEditorData.Ref.Roads)
                //{
                //    foreach (var sideWalk in road.SideWalks)
                //    {
                //        sideWalks.Add(sideWalk.Way.ToList());
                //    }
                //}

                var laneGroup = new LaneGroupEditorData(roadGroupEditorData.Ref);


                var wayEditorDataList = roadGroupEditorData.GetSubData<List<WayEditorData>>();

                //// way用の編集データがない場合は作成
                //if (wayEditorDataList == null || true)
                //{
                //    // wayを重複無しでコレクションする
                //    HashSet<RnWay> ways = new HashSet<RnWay>();
                //    foreach (var road in roadGroupEditorData.Ref.Roads)
                //    {
                //        foreach (var lane in road.AllLanes)
                //        {
                //            ways.Add(lane.LeftWay);
                //            ways.Add(lane.RightWay);
                //        }
                //    }

                //    // way用の編集データの作成
                //    wayEditorDataList = new List<WayEditorData>(ways.Count);
                //    foreach (var editingTarget in ways)
                //    {
                //        if (editingTarget == null)
                //        {
                //            continue;
                //        }
                //        wayEditorDataList.Add(new WayEditorData(editingTarget));
                //    }
                //    roadGroupEditorData.TryAdd(wayEditorDataList);

                //    //// 道路端のwayを編集不可能にする
                //    //wayEditorDataList.First().IsSelectable = false;
                //    //wayEditorDataList.Last().IsSelectable = false;

                //    // 下　もしかしたらwayを結合して扱う必要があるかも
                //    // 道路端のwayを編集不可能にする
                //    //var firstRoad = roadGroupEditorData.Ref.Roads.First();
                //    //var leftEdgeLane = firstRoad.MainLanes.First();
                //    //wayEditorDataList.Find(x => x.Ref == leftEdgeLane.LeftWay).IsSelectable = false;
                //    //var rightEdgeLane = firstRoad.MainLanes.Last();
                //    //if (leftEdgeLane == rightEdgeLane)  // レーンが一つしかない時は反対側のレーンを参照する
                //    //{
                //    //    wayEditorDataList.Find(x => x.Ref == rightEdgeLane.RightWay).IsSelectable = false;
                //    //}
                //    //else
                //    //{
                //    //    if (rightEdgeLane.LeftWay != null)
                //    //    {
                //    //        rightEdgeLane.GetBorderDir(RnLaneBorderType.)
                //    //        wayEditorDataList.Find(x => x.Ref == rightEdgeLane.LeftWay).IsSelectable = false;

                //    //    }
                //    //    wayEditorDataList.Find(x => x.Ref == rightEdgeLane.LeftWay).IsSelectable = false;
                //    //}
                //}

                // 選択可能なwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    selectableWayList.Add(way);
                    if (wayEditorData.IsSelectable)
                    {
                        var points = wayEditorData.Ref.Points;
                        foreach (var p in points)
                        {
                            way.Add(p);
                        }
                    }
                }

                // ガイド用のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    guideWayList.Add(way);
                    if (wayEditorData.IsSelectable == false)
                    {
                        var points = wayEditorData.Ref.Points;
                        foreach (var p in points)
                        {
                            way.Add(p);
                        }
                    }
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

            selectingWay.Clear();
            if (highLightWay != null)
            {
                //if (selectingElement is WayEditorData selectingWayData)
                if (highLightWay is WayEditorData selectingWayData)
                {
                    selectingWay.AddRange(selectingWayData.Ref.ToList());
                }
            }

            slideDummyWayList.Clear();
            if (slideDummyWay != null)
            {
                slideDummyWayList.Add(slideDummyWay.ToList());
            }

            intersectionBorder.Clear();
            var intersectionEditorData = selectingElement as EditorData<RnIntersection>;
            if (intersectionEditorData != null)
            {
                var borders = intersectionEditorData.Ref.GetBorders();
                foreach (var border in borders)
                {
                    intersectionBorder.Add(border.EdgeWay.ToList());                    
                }

                var lanes = intersectionEditorData.Ref.Lanes;
                foreach (var lane in lanes)
                {
                    foreach (var way in lane.BothWays)
                    {
                        intersectionOutline.Add(way.ToList());
                    }
                }
            }

        }


        public List<Action> BuildDrawCommands()
        {
            drawFuncs.Clear();

            foreach (var sideWalk in sideWalks)
            {
                if (sideWalk == null)
                {
                    continue;
                }
                var sideWalkList = sideWalk.ToArray();
                drawFuncs.Add(() =>
                {
                    Gizmos.color = sideWalkColor;
                    Gizmos.DrawLineStrip(sideWalkList, false);
                });
            }

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

            AddDrawFunc(ref drawFuncs, selectableWayList, selectableWayColor);

            AddDrawFunc(ref drawFuncs, guideWayList, guideWayColor);

            // 選択中のwayを描画
            if (selectingWay.Count >= 2)
            {
                drawFuncs.Add(() =>
                {
                    Gizmos.color = selectingWayColor;
                    {
                        Gizmos.DrawLineStrip(selectingWay.ToArray(), false);
                    }
                });
            }

            AddDrawFunc(ref drawFuncs, slideDummyWayList, slideDummyWayColor);

            AddDrawFunc(ref drawFuncs, intersectionOutline, intersectionOutlineColor);

            AddDrawFunc(ref drawFuncs, intersectionBorder, intersectionBorderColor);

            return drawFuncs;
        }

        private static void AddDrawFunc(ref List<Action> drawFuncs, List<List<Vector3>> lines, Color color)
        {
            if (lines.Count == 0)
            {
                return;
            }
            drawFuncs.Add(() =>
            {
                Gizmos.color = color;
                foreach (var line in lines)
                {
                    Gizmos.DrawLineStrip(line.ToArray(), false);
                }
            });

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
