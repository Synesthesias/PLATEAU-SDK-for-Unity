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
        /// <summary>
        /// 描画コマンド群
        /// </summary>
        private List<System.Action> drawFuncs = new List<Action>();

        // 交差間の繋がり(RnRoadGroup)を表現する頂点リスト
        private List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();

        // 選択中のwayを表現する頂点リスト
        private List<Vector3> selectingWay = new List<Vector3>();

        // 左側、右側車線のwayを表現する頂点リスト
        private List<List<Vector3>> leftLaneWayList = new List<List<Vector3>>();
        private List<List<Vector3>> rightLaneWayList = new List<List<Vector3>>();

        // 中央分離帯
        private List<List<Vector3>> medianWayList = new List<List<Vector3>>();

        // 歩道
        private List<List<Vector3>> sideWalks = new List<List<Vector3>>();

        // 選択中のwayをスライドした時の結果表示
        private List<List<Vector3>> slideDummyWayList = new List<List<Vector3>>();

        // 交差点の外周
        private List<List<Vector3>> intersectionOutline = new List<List<Vector3>>();
        // 交差点と道路の境界
        private List<List<Vector3>> intersectionBorder = new List<List<Vector3>>();

        // それぞれのwayの色

        private readonly Color selectingColorOffset = new Color(0.2f, 0.2f, 0.2f, 0);
        private readonly Color dummyColorOffset = new Color(-0.2f, -0.2f, -0.2f, 0);

        private readonly Color connectionColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
        private readonly Color selectingWayColor = Color.red;
                
        private readonly Color leftSideWayColor = Color.yellow;
        private readonly Color rightSideWayColor = Color.green;
        private readonly Color medianWayColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
                
        private readonly Color guideWayColor = Color.black;
        private readonly Color sideWalkColor = Color.magenta;
        private readonly Color slideDummyWayColor = Color.red + new Color(-0.2f, -0.2f, -0.2f, 0);
                
        private readonly Color intersectionOutlineColor = Color.gray;
        private readonly Color intersectionBorderColor = Color.gray + new Color(-0.2f, -0.2f, -0.2f, 0);

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
                if (item == null)
                    continue;

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
            rightLaneWayList.Clear();
            leftLaneWayList.Clear();
            sideWalks.Clear();
            medianWayList.Clear();
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

                // 車線のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.Main)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }
                        
                    var parent = wayEditorData.Parent;
                    Debug.Assert(parent != null);
                    if (parent.IsReverse == false)
                    {
                        leftLaneWayList.Add(way);
                    }
                    else
                    {
                        rightLaneWayList.Add(way);
                    }
                }

                // 歩道のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.SideWalk)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }

                    sideWalks.Add(way);
                }

                // 中央分離帯のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.Median)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }

                    medianWayList.Add(way);
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

            AddDrawFunc(ref drawFuncs, leftLaneWayList, leftSideWayColor);
            AddDrawFunc(ref drawFuncs, rightLaneWayList, rightSideWayColor);

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

            AddDrawFunc(ref drawFuncs, medianWayList, medianWayColor);

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
