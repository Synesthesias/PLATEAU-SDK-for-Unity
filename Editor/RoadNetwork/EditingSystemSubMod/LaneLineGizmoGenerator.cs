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
    /// 道路のレーン編集（交差点編集時の線も含む）のために、
    /// シーン上に描画するための線を生成します。
    /// </summary>
    internal class LaneLineGizmoGenerator
    {
        // それぞれのwayの色
        private static readonly Color selectingWayColor = Color.red;
        private static readonly Color leftSideWayColor = new Color(0.7f, 0.1f, 0.9f);
        private static readonly Color rightSideWayColor = Color.green;
        private static readonly Color medianWayColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
        private static readonly Color sideWalkColor = new Color(0.7f, 0.6f, 0.3f);
        private static readonly Color slideDummyWayColor = Color.red + new Color(-0.2f, -0.2f, -0.2f, 0);
        private static readonly Color intersectionOutlineColor = new Color(0.2f, 0.6f, 0.5f);
        private static readonly Color intersectionBorderColor = new Color(0.2f, 1f, 0.2f);
        private static readonly Color mainLaneCenterWayColor = Color.cyan + new Color(0, -0.4f, -0.4f, 0);
        
        /// <summary>
        /// 編集用のギズモの線を生成します。
        /// </summary>
        public List<ILaneLineDrawer> Generate(
            object selectingElement,
            WayEditorData highLightWay,
            EditorDataList<EditorData<RnRoadGroup>> linkGroupEditorData,
            RnWay slideDummyWay)
        {
            List<ILaneLineDrawer> lineDrawers = new();
            
            if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<RoadGroupEditorData> eConn) == false)
            {
                Assert.IsTrue(false);
                return lineDrawers;
            }
            
            
            // RoadGroupが選択されている
            if (selectingElement is EditorData<RnRoadGroup> roadGroupEditorData)
            {

                // 歩道の線を追加
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var sideWalk in road.SideWalks)
                    {
                        lineDrawers.Add(new LaneLineDrawerSolid(sideWalk.OutsideWay.ToList(), sideWalkColor, LaneLineDrawMethod.Gizmos));
                    }
                }

                var wayEditorDataList = roadGroupEditorData.ReqSubData<WayEditorDataList>().Raw;

                // 車線のwayを追加
                foreach (var wayEditorData in wayEditorDataList)
                {

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
                        lineDrawers.Add(new LaneLineDrawerSolid(way, leftSideWayColor, LaneLineDrawMethod.Gizmos));
                    }
                    else
                    {
                        lineDrawers.Add(new LaneLineDrawerSolid(way, rightSideWayColor, LaneLineDrawMethod.Gizmos));
                    }
                }

                // 車線の向きを描画
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var lane in road.MainLanes)
                    {
                        var centerWay = lane.CreateCenterWay();
                        if (centerWay == null)
                            continue;
                        lineDrawers.Add(new LaneLineDrawerArrow(centerWay, mainLaneCenterWayColor, LaneLineDrawMethod.Gizmos));
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

                    lineDrawers.Add(new LaneLineDrawerSolid(way, sideWalkColor, LaneLineDrawMethod.Gizmos));
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
                    lineDrawers.Add(new LaneLineDrawerSolid(way, medianWayColor, LaneLineDrawMethod.Gizmos));
                }

            }
            
            // 選択中の線を追加
            if (highLightWay != null)
            {
                if (highLightWay is WayEditorData selectingWayData)
                {
                    lineDrawers.Add(new LaneLineDrawerSolid(selectingWayData.Ref.ToList(), selectingWayColor, LaneLineDrawMethod.Gizmos));
                }
            }
            
            if (slideDummyWay != null)
            {
                lineDrawers.Add(new LaneLineDrawerSolid(slideDummyWay.ToList(), slideDummyWayColor, LaneLineDrawMethod.Gizmos));
            }

            var intersectionEditorData = selectingElement as EditorData<RnIntersection>;
            if (intersectionEditorData != null)
            {
                foreach (var neighbor in intersectionEditorData.Ref.Borders)
                {
                    if (neighbor.Border != null)
                        lineDrawers.Add(new LaneLineDrawerSolid(neighbor.Border.ToList(), intersectionBorderColor, LaneLineDrawMethod.Gizmos));
                }
            }
            
            if (intersectionEditorData != null)
            {
                foreach (var edge in intersectionEditorData.Ref.Edges.Where(e => e.Road == null))
                {
                    lineDrawers.Add(new LaneLineDrawerSolid(edge.Border.ToList(), intersectionOutlineColor, LaneLineDrawMethod.Gizmos));
                }
            }

            return lineDrawers;
        }
    }
}
