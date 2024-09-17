using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Structure
{
    public class RnModelDebugEditorWindow : EditorWindow
    {
        public interface IInstanceHelper
        {
            RnModel GetModel();

            HashSet<RnRoad> TargetRoads { get; }

            HashSet<RnIntersection> TargetIntersections { get; }

            HashSet<RnLane> TargetLanes { get; }

            HashSet<RnWay> TargetWays { get; }

            HashSet<RnSideWalk> TargetSideWalks { get; }

            public bool IsTarget(RnRoadBase roadBase);
        }

        private const string WindowName = "Debug RnModel Editor";

        private IInstanceHelper InstanceHelper { get; set; }

        private long addTargetId = -1;

        private LaneEdit laneEdit = new LaneEdit();
        private RoadEdit roadEdit = new RoadEdit();
        private IntersectionEdit intersectionEdit = new IntersectionEdit();
        private SideWalkEdit sideWalkEdit = new SideWalkEdit();

        // FoldOutの状態を保持する
        private HashSet<object> FoldOuts { get; } = new();


        private class LaneEdit
        {
            [Serializable]
            public class LaneSplitEdit
            {
                public int splitNum = 2;
            }
            public LaneSplitEdit splitEdit = new LaneSplitEdit();

            [Serializable]
            public class LaneWidthEdit
            {
                public LaneWayMoveOption moveOption = LaneWayMoveOption.MoveBothWay;
                public float width = 0f;
                public float moveWidth = 0f;
            }

            public LaneWidthEdit widthEdit = new LaneWidthEdit();

            public ulong laneNormalId = ulong.MaxValue;
            public float rightWayPos = 0f;
            public float leftWayPos = 0f;
        }

        /// <summary>
        /// レーンの編集
        /// </summary>
        /// <param name="lane"></param>
        public void EditLane(RnLane lane)
        {
            if (lane == null)
                return;
            var p = laneEdit;
            RnEditorUtil.TargetToggle($"Id '{lane.DebugMyId.ToString()}'", InstanceHelper.TargetLanes, lane);
            using (new EditorGUI.DisabledScope(false))
            {
                void Draw(RnLaneBorderType type)
                {
                    RnEditorUtil.Separator();
                    EditorGUILayout.LabelField($"Border {type}");
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var border = lane.GetBorder(type);
                        EditorGUILayout.LabelField($"BorderWay {border?.GetDebugMyIdOrDefault()}[{border?.LineString?.GetDebugMyIdOrDefault()}]");
                        EditorGUILayout.LabelField($"Connect Lanes [{lane.GetConnectedLanes(type).Select(l => l.DebugMyId).Join2String()}]");
                        EditorGUILayout.LabelField($"Connect Roads [{lane.GetConnectedRoads(type).Select(l => l.DebugMyId).Join2String()}]");
                    }
                }
                lane.Attributes = (RnLaneAttribute)EditorGUILayout.EnumFlagsField("Attribute", lane.Attributes);
                Draw(RnLaneBorderType.Prev);
                Draw(RnLaneBorderType.Next);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LongField($"Left Way", lane.LeftWay.GetDebugMyIdOrDefault());
                    EditorGUILayout.LongField($"Right Way", lane.RightWay.GetDebugMyIdOrDefault());
                }
            }

            // 情報表示
            if (p.rightWayPos != 0f && lane.RightWay != null)
            {
                p.rightWayPos = 0f;
            }

            if (p.leftWayPos != 0f && lane.LeftWay != null)
            {
                p.leftWayPos = 0f;
            }

            using (var _ = new EditorGUILayout.HorizontalScope())
            {
                p.splitEdit.splitNum = EditorGUILayout.IntField("SplitNum", p.splitEdit.splitNum);
                if (GUILayout.Button("Split"))
                {
                    if (lane.Parent is RnRoad road)
                    {
                        var lanes = lane.SplitLane(p.splitEdit.splitNum, true);
                        foreach (var item in lanes)
                        {
                            var l = item.Key;
                            var parent = l.Parent as RnRoad;
                            parent?.ReplaceLane(l, item.Value);
                        }
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(false))
                {
                    EditorGUILayout.FloatField("Width", lane.CalcWidth());
                }

                p.widthEdit.width = EditorGUILayout.FloatField("->", p.widthEdit.width);
                p.widthEdit.moveOption = (LaneWayMoveOption)EditorGUILayout.EnumPopup("MoveOption", p.widthEdit.moveOption);
                if (GUILayout.Button("SetWidth"))
                {
                    lane.TrySetWidth(p.widthEdit.width, p.widthEdit.moveOption);
                }
            }

            if (p.widthEdit.moveWidth != 0f)
            {
                switch (p.widthEdit.moveOption)
                {
                    case LaneWayMoveOption.MoveBothWay:
                        lane.LeftWay?.MoveAlongNormal(p.widthEdit.moveWidth * 0.5f);
                        lane.RightWay?.MoveAlongNormal(p.widthEdit.moveWidth * 0.5f);
                        break;
                    case LaneWayMoveOption.MoveLeftWay:
                        lane.LeftWay?.MoveAlongNormal(p.widthEdit.moveWidth);
                        break;
                    case LaneWayMoveOption.MoveRightWay:
                        lane.RightWay?.MoveAlongNormal(p.widthEdit.moveWidth);
                        break;
                }
                p.widthEdit.moveWidth = 0f;
            }
        }



        private class RoadEdit
        {
            // 左側レーン数
            public int leftLaneCount = -1;
            // 右側レーン数
            public int rightLaneCount = -1;

            // 中央分離帯幅
            public float medianWidth = 0;
            public LaneWayMoveOption medianWidthOption = LaneWayMoveOption.MoveBothWay;

            public HashSet<RnLane> FoldoutLanes { get; } = new HashSet<RnLane>();
        }

        /// <summary>
        /// 道路の編集
        /// </summary>
        /// <param name="road"></param>
        private void EditRoad(RnRoad road)
        {
            var p = roadEdit;
            if (road == null)
                return;
            var roadGroup = road.CreateRoadGroupOrDefault();
            if (roadGroup == null)
                return;

            RnEditorUtil.TargetToggle($"Id '{road.DebugMyId.ToString()}'", InstanceHelper.TargetRoads, road);
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.LongField("Prev", (long)(road.Prev?.DebugMyId ?? ulong.MaxValue));
                EditorGUILayout.LongField("Next", (long)(road.Next?.DebugMyId ?? ulong.MaxValue));
            }

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("LaneCount");
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"L({roadGroup.GetLeftLaneCount()})->", GUILayout.Width(80));
                        p.leftLaneCount = EditorGUILayout.IntField("", p.leftLaneCount, GUILayout.Width(80));
                        if (GUILayout.Button("Change"))
                        {
                            roadGroup.SetLeftLaneCount(p.leftLaneCount);
                        }
                        EditorGUILayout.LabelField($"R({roadGroup.GetRightLaneCount()})->", GUILayout.Width(80));
                        p.rightLaneCount = EditorGUILayout.IntField(p.rightLaneCount, GUILayout.Width(80));
                        if (GUILayout.Button("Change"))
                        {
                            roadGroup.SetRightLaneCount(p.rightLaneCount);
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                    }
                    if (GUILayout.Button("Change Both"))
                    {
                        roadGroup.SetLaneCount(p.leftLaneCount, p.rightLaneCount);
                    }
                }
            }

            if (GUILayout.Button("Align"))
            {
                roadGroup.Align();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                p.medianWidth = EditorGUILayout.FloatField("MedianWidth", p.medianWidth);
                p.medianWidthOption = (LaneWayMoveOption)EditorGUILayout.EnumPopup("MoveOption", p.medianWidthOption);
                if (GUILayout.Button("SetMedianWidth"))
                {
                    roadGroup.SetMedianWidth(p.medianWidth, p.medianWidthOption);
                }

                if (GUILayout.Button("RemoveMedian"))
                {
                    roadGroup.RemoveMedian(p.medianWidthOption);
                }
            }

            if (GUILayout.Button("DisConnect"))
            {
                road.DisConnect(false);
            }

            if (GUILayout.Button("Convert2Intersection"))
            {
                road.ParentModel.Convert2Intersection(road);
            }

            foreach (var lane in road.MainLanes)
            {
                var foldout = EditorGUILayout.Foldout(p.FoldoutLanes.Contains(lane), $"Lane {lane.GetDebugMyIdOrDefault()}");
                if (foldout)
                {
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        p.FoldoutLanes.Add(lane);
                        EditLane(lane);
                    }
                }
                else
                {
                    p.FoldoutLanes.Remove(lane);
                }
            }
        }
        private class IntersectionEdit
        {
            public long convertPrevRoadId = -1;
            public long convertNextRoadId = -1;
        }

        public void EditIntersection(RnIntersection intersection)
        {
            if (intersection == null)
                return;
            var p = intersectionEdit;

            RnEditorUtil.TargetToggle($"Id '{intersection.DebugMyId.ToString()}'", InstanceHelper.TargetIntersections, intersection);
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.LabelField("Intersection ID", intersection.DebugMyId.ToString());
                EditorGUILayout.LabelField("Border");
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var b in intersection.Neighbors)
                    {
                        EditorGUILayout.LabelField($"Neighbor:{b.Road.GetDebugMyIdOrDefault()}, Border:{b.Border.GetDebugMyIdOrDefault()}");
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                p.convertPrevRoadId = EditorGUILayout.LongField("PrevRoadId", p.convertPrevRoadId);
                p.convertNextRoadId = EditorGUILayout.LongField("NextRoadId", p.convertNextRoadId);

                var prev = intersection.Neighbors.Select(n => n.Road)
                    .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)p.convertPrevRoadId);
                var next = intersection.Neighbors.Select(n => n.Road)
                    .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)p.convertNextRoadId);

                if (GUILayout.Button("Convert2Road"))
                {
                    intersection.ParentModel.Convert2Road(intersection, prev, next);
                }
            }

            if (GUILayout.Button("DisConnect"))
            {
                intersection.DisConnect(false);
            }

            if (GUILayout.Button("Build Track"))
            {
                intersection.BuildTracks();
            }
        }

        public class WayEdit
        {

        }

        public class SideWalkEdit
        {
        }

        public void EditSideWalk(RnSideWalk sideWalk)
        {
            var p = sideWalkEdit;

            RnEditorUtil.TargetToggle($"Id '{sideWalk.DebugMyId.ToString()}'", InstanceHelper.TargetSideWalks,
                sideWalk);
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.LabelField($"ParentRoad:{sideWalk.ParentRoad.GetDebugMyIdOrDefault()}");
            }
        }

        public void Reinitialize()
        {
        }

        private void Initialize()
        {
        }

        private void OnEnable()
        {
            Initialize();
        }

        /// <Summary>
        /// ウィンドウのパーツを表示します。
        /// </Summary>
        private void OnGUI()
        {
            var model = InstanceHelper?.GetModel();
            if (model == null)
                return;
            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("Lane Edit", GUILayout.Height(20));

            //addTargetId = RnEditorUtil.CheckAddTarget(InstanceHelper.TargetLanes, this.addTargetId, out var isAddLane);
            // 内部でTargetLanesを更新するため、ToListでコピーを取得
            foreach (var l in InstanceHelper.TargetLanes.ToList())
            {
                RnEditorUtil.Separator();
                EditLane(l);
            }

            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("Road Edit", GUILayout.Height(20));
            addTargetId = RnEditorUtil.CheckAddTarget<RnRoad, RnRoadBase>(InstanceHelper.TargetRoads, this.addTargetId, out var isAddRoad);
            foreach (var r in model.Roads)
            {
                if (isAddRoad && r.DebugMyId == (ulong)addTargetId)
                    InstanceHelper.TargetRoads.Add(r);
                if (InstanceHelper.IsTarget(r) == false && InstanceHelper.TargetRoads.Contains(r) == false)
                    continue;

                var foldout = EditorGUILayout.Foldout(FoldOuts.Contains(r), $"Road {r.GetDebugMyIdOrDefault()}");
                if (foldout)
                {
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        FoldOuts.Add(r);
                        EditRoad(r);
                    }
                }
                else
                {
                    FoldOuts.Remove(r);
                }

            }

            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("Intersection Edit", GUILayout.Height(20));

            addTargetId = RnEditorUtil.CheckAddTarget<RnIntersection, RnRoadBase>(InstanceHelper.TargetIntersections, this.addTargetId, out var isAddInter);
            foreach (var i in model.Intersections)
            {
                if (isAddInter && i.DebugMyId == (ulong)addTargetId)
                    InstanceHelper.TargetIntersections.Add(i);
                if (InstanceHelper.IsTarget(i) == false && InstanceHelper.TargetIntersections.Contains(i) == false)
                    continue;
                var foldout = EditorGUILayout.Foldout(FoldOuts.Contains(i), $"InterSection {i.GetDebugMyIdOrDefault()}");
                if (foldout)
                {
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        FoldOuts.Add(i);
                        EditIntersection(i);
                    }
                }
                else
                {
                    FoldOuts.Remove(i);
                }
            }

            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("Side Walk Edit", GUILayout.Height(20));
            addTargetId = RnEditorUtil.CheckAddTarget<RnIntersection, RnRoadBase>(InstanceHelper.TargetIntersections, this.addTargetId, out var isAddSideWalk);
            foreach (var sw in model.SideWalks)
            {
                if (isAddSideWalk && sw.DebugMyId == (ulong)addTargetId)
                    InstanceHelper.TargetSideWalks.Add(sw);
                if (InstanceHelper.TargetSideWalks.Contains(sw) == false)
                    continue;
                var foldout = EditorGUILayout.Foldout(FoldOuts.Contains(sw), $"SideWalk {sw.GetDebugMyIdOrDefault()}");
                if (foldout)
                {
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        FoldOuts.Add(sw);
                        EditSideWalk(sw);
                    }
                }
                else
                {
                    FoldOuts.Remove(sw);
                }
            }
        }

        /// <summary>
        /// ウィンドウを取得する、存在しない場合に生成する
        /// </summary>
        /// <param name="instanceHelper"></param>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static RnModelDebugEditorWindow OpenWindow(IInstanceHelper instanceHelper, bool focus)
        {
            var ret = GetWindow<RnModelDebugEditorWindow>(WindowName, focus);
            ret.InstanceHelper = instanceHelper;
            return ret;
        }

        /// <summary>
        /// ウィンドウのインスタンスを確認する
        /// ラップ関数
        /// </summary>
        /// <returns></returns>
        public static bool HasOpenInstances()
        {
            return HasOpenInstances<RnModelDebugEditorWindow>();
        }

    }
}