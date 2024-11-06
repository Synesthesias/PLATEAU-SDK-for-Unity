using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// ギズモの描画システム
    /// ギズモ描画を行う処理を生成する。
    /// 実際の描画はMonoBehaviourを継承したギズモ描画クラスを用意したのでそこで行う。
    /// </summary>
    public class EditingSystemGizmos
    {
        private List<System.Action> drawFuncs = new List<Action>();

        private List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();
        private List<List<Vector3>> intersectionConnectionLinePairs2 = new List<List<Vector3>>();

        private List<Vector3> selectingWay = new List<Vector3>();

        private List<List<Vector3>> selectableWayList = new List<List<Vector3>>();

        private List<List<Vector3>> guideWayList = new List<List<Vector3>>();

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
        private Color slideDummyWayColor = Color.red + new Color(-0.2f, -0.2f, -0.2f, 0);

        private Color intersectionOutlineColor = Color.gray;
        private Color intersectionBorderColor = Color.gray + new Color(-0.2f, -0.2f, -0.2f, 0);

        public EditingSystemGizmos()
        {

        }


        /// <summary>
        /// 描画コマンド生成に必要なデータを更新する
        /// </summary>
        /// <param name="selectingElement"></param>
        /// <param name="highLightWay"></param>
        /// <param name="linkGroupEditorData"></param>
        /// <param name="slideDummyWay"></param>
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

                // 歩道の描画
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var sideWalk in road.SideWalks)
                    {
                        sideWalks.Add(sideWalk.OutsideWay.ToList());
                    }
                }

                var laneGroup = new LaneGroupEditorData(roadGroupEditorData.Ref);


                var wayEditorDataList = roadGroupEditorData.GetSubData<List<WayEditorData>>();

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

            intersectionConnectionLinePairs = pts;

            selectingWay.Clear();
            if (highLightWay != null)
            {
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
                foreach (var neighbor in intersectionEditorData.Ref.Neighbors)
                {
                    if (neighbor.Border != null)
                        intersectionOutline.Add(neighbor.Border.ToList());
                }
            }

        }


        /// <summary>
        /// 描画コマンドの生成
        /// </summary>
        /// <returns></returns>
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
                });
            }

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
