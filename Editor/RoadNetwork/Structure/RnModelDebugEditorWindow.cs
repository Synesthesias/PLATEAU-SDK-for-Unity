using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{

    public class RnModelDebugEditorWindow : EditorWindow
    {
        public interface IInstanceHelper
        {
            RnModel GetModel();

            long TargetLaneId { get; set; }

            long TargetRoadId { get; set; }

            long TargetIntersectionId { get; set; }

            public bool IsTarget(RnRoadBase roadBase);
        }

        private const string WindowName = "Debug RnModel Editor";

        private IInstanceHelper InstanceHelper { get; set; }

        private class LaneEdit
        {
            private class LaneSplitEdit
            {
                public int splitNum = 2;
            }
            private readonly LaneSplitEdit splitEdit = new LaneSplitEdit();

            [Serializable]
            public class LaneWidthEdit
            {
                public LaneWayMoveOption moveOption = LaneWayMoveOption.MoveBothWay;
                public float width = 0f;
                public float moveWidth = 0f;
            }

            private readonly LaneWidthEdit widthEdit = new LaneWidthEdit();

            ulong laneNormalId = ulong.MaxValue;
            public float rightWayPos = 0f;
            public float leftWayPos = 0f;


            public void Update(RnLane lane)
            {
                if (lane == null)
                    return;

                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Lane ID", lane.DebugMyId.ToString());
                    EditorGUILayout.LongField("PrevBorder", (long)(lane.PrevBorder?.DebugMyId ?? ulong.MaxValue));
                    EditorGUILayout.LongField("NextBorder", (long)(lane.NextBorder?.DebugMyId ?? ulong.MaxValue));
                }
                // 情報表示

                if (rightWayPos != 0f && lane.RightWay != null)
                {
                    rightWayPos = 0f;
                }

                if (leftWayPos != 0f && lane.LeftWay != null)
                {
                    leftWayPos = 0f;
                }

                using (var _ = new EditorGUILayout.HorizontalScope())
                {
                    splitEdit.splitNum = EditorGUILayout.IntField("SplitNum", splitEdit.splitNum);
                    if (GUILayout.Button("Split"))
                    {
                        if (lane.Parent is RnRoad road)
                        {
                            var lanes = lane.SplitLane(splitEdit.splitNum, true);
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

                    widthEdit.width = EditorGUILayout.FloatField("->", widthEdit.width);
                    widthEdit.moveOption = (LaneWayMoveOption)EditorGUILayout.EnumPopup("MoveOption", widthEdit.moveOption);
                    if (GUILayout.Button("SetWidth"))
                    {
                        lane.TrySetWidth(widthEdit.width, widthEdit.moveOption);
                    }
                }

                if (widthEdit.moveWidth != 0f)
                {
                    switch (widthEdit.moveOption)
                    {
                        case LaneWayMoveOption.MoveBothWay:
                            lane.LeftWay?.MoveAlongNormal(widthEdit.moveWidth * 0.5f);
                            lane.RightWay?.MoveAlongNormal(widthEdit.moveWidth * 0.5f);
                            break;
                        case LaneWayMoveOption.MoveLeftWay:
                            lane.LeftWay?.MoveAlongNormal(widthEdit.moveWidth);
                            break;
                        case LaneWayMoveOption.MoveRightWay:
                            lane.RightWay?.MoveAlongNormal(widthEdit.moveWidth);
                            break;
                    }
                    widthEdit.moveWidth = 0f;
                }
            }
        }
        LaneEdit laneEdit = new LaneEdit();


        private class RoadEdit
        {
            // 左側レーン数
            public int leftLaneCount = -1;
            // 右側レーン数
            public int rightLaneCount = -1;

            // 中央分離帯幅
            public float medianWidth = 0;
            public LaneWayMoveOption medianWidthOption = LaneWayMoveOption.MoveBothWay;

            public void Update(RnRoad road)
            {
                if (road == null)
                    return;
                var roadGroup = road.CreateRoadGroupOrDefault();
                if (roadGroup == null)
                    return;

                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Road ID", road.DebugMyId.ToString());
                    EditorGUILayout.LongField("Prev", (long)(road.Prev?.DebugMyId ?? ulong.MaxValue));
                    EditorGUILayout.LongField("Next", (long)(road.Next?.DebugMyId ?? ulong.MaxValue));

                }

                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(false))
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.IntField("Left Lanes", roadGroup.GetLeftLaneCount());
                        EditorGUILayout.IntField("Right Lanes", roadGroup.GetRightLaneCount());
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        leftLaneCount = EditorGUILayout.IntField("Left Lanes", leftLaneCount);
                        rightLaneCount = EditorGUILayout.IntField("Right Lanes", rightLaneCount);
                    }

                    if (GUILayout.Button("ChangeLaneCount"))
                    {
                        roadGroup.SetLeftLaneCount(leftLaneCount);
                        roadGroup.SetRightLaneCount(rightLaneCount);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    medianWidth = EditorGUILayout.FloatField("MedianWidth", medianWidth);
                    medianWidthOption = (LaneWayMoveOption)EditorGUILayout.EnumPopup("MoveOption", medianWidthOption);
                    if (GUILayout.Button("SetMedianWidth"))
                    {
                        roadGroup.SetMedianWidth(medianWidth, medianWidthOption);
                    }

                    if (GUILayout.Button("RemoveMedian"))
                    {
                        roadGroup.RemoveMedian(medianWidthOption);
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
            }
        }
        RoadEdit roadEdit = new RoadEdit();

        private class IntersectionEdit
        {
            public long convertPrevRoadId = -1;
            public long convertNextRoadId = -1;
            public void Update(RnIntersection intersection)
            {
                if (intersection == null)
                    return;

                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField("Intersection ID", intersection.DebugMyId.ToString());
                    foreach (var b in intersection.Neighbors)
                    {
                        EditorGUILayout.LabelField($"Road:{((RnRoadBase)b.Road).GetDebugMyIdOrDefault()}, Border:{b.Border.GetDebugMyIdOrDefault()}");
                    }
                }

                if (GUILayout.Button("DisConnect"))
                {
                    intersection.DisConnect(false);
                }


                using (new EditorGUILayout.HorizontalScope())
                {
                    convertPrevRoadId = EditorGUILayout.LongField("PrevRoadId", convertPrevRoadId);
                    convertNextRoadId = EditorGUILayout.LongField("NextRoadId", convertNextRoadId);

                    var prev = intersection.Neighbors.Select(n => n.Road)
                        .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)convertPrevRoadId);
                    var next = intersection.Neighbors.Select(n => n.Road)
                        .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)convertNextRoadId);

                    if (GUILayout.Button("Convert2Road"))
                    {
                        intersection.ParentModel.Convert2Road(intersection, prev, next);
                    }
                }

            }
        }
        IntersectionEdit intersectionEdit = new IntersectionEdit();

        public class WayEdit
        {

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
            InstanceHelper.TargetLaneId = EditorGUILayout.LongField("Target Lane ID", InstanceHelper.TargetLaneId);
            InstanceHelper.TargetRoadId = EditorGUILayout.LongField("Target Road ID", InstanceHelper.TargetRoadId);
            InstanceHelper.TargetIntersectionId = EditorGUILayout.LongField("Target Intersection ID", InstanceHelper.TargetIntersectionId);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Lane Edit");
            var lane = model.CollectAllLanes().FirstOrDefault(l => l.DebugMyId == (ulong)InstanceHelper.TargetLaneId);
            laneEdit.Update(lane);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Road Edit");
            var road = model.Roads.FirstOrDefault(r => r.DebugMyId == (ulong)InstanceHelper.TargetRoadId);
            roadEdit.Update(road);
            foreach (var r in model.Roads)
            {
                if (InstanceHelper.IsTarget(r) == false)
                    continue;
                EditorGUILayout.Separator();
                roadEdit.Update(r);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Intersection Edit");
            var intersection = model.Intersections.FirstOrDefault(r => r.DebugMyId == (ulong)InstanceHelper.TargetIntersectionId);
            intersectionEdit.Update(intersection);
            foreach (var i in model.Intersections)
            {
                if (InstanceHelper.IsTarget(i) == false)
                    continue;
                EditorGUILayout.Separator();
                intersectionEdit.Update(i);
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