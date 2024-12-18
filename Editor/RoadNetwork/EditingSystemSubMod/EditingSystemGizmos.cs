using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// ギズモの描画システム
    /// ギズモ描画を行う処理を生成する。
    /// 実際の描画はMonoBehaviourを継承したギズモ描画クラスを用意したのでそこで行う。
    /// </summary>
    internal class EditingSystemGizmos
    {
        /// <summary>
        /// 描画コマンド群
        /// </summary>
        private List<System.Action> drawFuncs = new List<Action>();

        // 選択中のwayを表現する頂点リスト
        private LaneLineDrawerSolid selectingWay = LaneLineDrawerSolid.CreateWithEmptyLine(selectingWayColor);

        // 左側、右側車線のwayを表現する頂点リスト
        private List<LaneLineDrawerSolid> leftLaneWayList = new();
        private List<LaneLineDrawerSolid> rightLaneWayList = new();

        // 中央分離帯
        private List<LaneLineDrawerSolid> medianWayList = new ();

        // 歩道
        private List<LaneLineDrawerSolid> sideWalks = new();

        private List<LaneLineDrawerArrow> mainLaneCenterWay = new();

        // 選択中のwayをスライドした時の結果表示
        private List<LaneLineDrawerSolid> slideDummyWayList = new();

        // 交差点の外周
        private List<List<Vector3>> intersectionOutline = new List<List<Vector3>>();
        // 交差点と道路の境界
        private List<List<Vector3>> intersectionBorder = new List<List<Vector3>>();

        // それぞれのwayの色


        private static readonly Color connectionColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
        private static readonly Color selectingWayColor = Color.red;
                
        private static readonly Color leftSideWayColor = Color.yellow;
        private static readonly Color rightSideWayColor = Color.green;
        private static readonly Color medianWayColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
        
        private static readonly Color sideWalkColor = Color.magenta;
        private static readonly Color slideDummyWayColor = Color.red + new Color(-0.2f, -0.2f, -0.2f, 0);
                
        private static readonly Color intersectionOutlineColor = Color.gray;
        private static readonly Color intersectionBorderColor = Color.gray + new Color(-0.2f, -0.2f, -0.2f, 0);

        private static readonly Color mainLaneCenterWayColor = Color.cyan + new Color(0, -0.4f, -0.4f, 0);

        public EditingSystemGizmos()
        {

        }

        public void Clear()
        {
            drawFuncs.Clear();
            selectingWay.ClearLine();
            leftLaneWayList.Clear();
            rightLaneWayList.Clear();
            medianWayList.Clear();
            sideWalks.Clear();
            slideDummyWayList.Clear();
            intersectionOutline.Clear();
            intersectionBorder.Clear();
            mainLaneCenterWay.Clear();
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
            if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<RoadGroupEditorData> eConn) == false)
            {
                Assert.IsTrue(false);
                return;
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
                        sideWalks.Add(new LaneLineDrawerSolid(sideWalk.OutsideWay.ToList(), sideWalkColor));
                    }
                }

                var laneGroup = new LaneGroupEditorData(roadGroupEditorData.Ref);


                var wayEditorDataList = roadGroupEditorData.ReqSubData<WayEditorDataList>().Raw;

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
                        
                    var parent = wayEditorData.ParentLane;
                    Debug.Assert(parent != null);
                    if (parent.IsReverse == false)
                    {
                        leftLaneWayList.Add(new LaneLineDrawerSolid(way, leftSideWayColor));
                    }
                    else
                    {
                        rightLaneWayList.Add(new LaneLineDrawerSolid(way, rightSideWayColor));
                    }
                }

                // 車線の向きを描画
                mainLaneCenterWay.Clear();
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var lane in road.MainLanes)
                    {
                        var centerWay = lane.CreateCenterWay();
                        if (centerWay == null)
                            continue;
                        mainLaneCenterWay.Add(new LaneLineDrawerArrow(centerWay, mainLaneCenterWayColor));
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

                    sideWalks.Add(new LaneLineDrawerSolid(way, sideWalkColor));
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

                    medianWayList.Add(new LaneLineDrawerSolid(way, medianWayColor));
                }

            }


            selectingWay.ClearLine();
            if (highLightWay != null)
            {
                if (highLightWay is WayEditorData selectingWayData)
                {
                    selectingWay.SetLine(selectingWayData.Ref.ToList());
                }
            }

            slideDummyWayList.Clear();
            if (slideDummyWay != null)
            {
                slideDummyWayList.Add(new LaneLineDrawerSolid(slideDummyWay.ToList(), slideDummyWayColor));
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


            drawFuncs.Add(() => {foreach(var s in sideWalks) s.Draw();});
            drawFuncs.Add(() => {foreach(var l in leftLaneWayList) l.Draw();});
            drawFuncs.Add(() => {foreach (var r in rightLaneWayList) r.Draw(); });
            drawFuncs.Add(() => selectingWay.Draw());
            drawFuncs.Add(() => {foreach (var s in slideDummyWayList) s.Draw(); });

            AddDrawFunc(ref drawFuncs, intersectionOutline, intersectionOutlineColor);

            AddDrawFunc(ref drawFuncs, intersectionBorder, intersectionBorderColor);

            drawFuncs.Add(() => {foreach(var m in medianWayList) m.Draw();});


            drawFuncs.Add(() => {foreach(var m in mainLaneCenterWay) m.Draw();});

            

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
