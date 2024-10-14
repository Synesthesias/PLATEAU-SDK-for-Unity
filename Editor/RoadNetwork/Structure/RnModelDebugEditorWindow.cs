using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Util;
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

            // 非表示対象オブジェクト
            HashSet<object> InVisibleObjects { get; }

            // 選択済みオブジェクト
            HashSet<object> SelectedObjects { get; }
        }

        private const string WindowName = "Debug RnModel Editor";

        private IInstanceHelper InstanceHelper { get; set; }

        private AddTargetType addTargetType = AddTargetType.Road;
        private long addTargetId = -1;

        private LaneEdit laneEdit = new LaneEdit();
        private RoadEdit roadEdit = new RoadEdit();
        private IntersectionEdit intersectionEdit = new IntersectionEdit();
        private SideWalkEdit sideWalkEdit = new SideWalkEdit();

        // FoldOutの状態を保持する
        private HashSet<object> FoldOuts { get; } = new();

        /// <summary>
        /// Id指定での対象追加用タイプ
        /// </summary>
        private enum AddTargetType
        {
            Road,
            Intersection,
            SideWalk,
        }

        public class Work
        {
            // foreach文で回している最中に実行するとまずいもの(リストの変換等)の遅延実行用
            public List<Action> DelayExec { get; } = new();
        }

        private void ShowBase(ARnPartsBase parts)
        {
            using var _ = new EditorGUILayout.HorizontalScope();
            RnEditorUtil.SelectToggle($"select", InstanceHelper.SelectedObjects, parts);
            RnEditorUtil.VisibleToggle(InstanceHelper.InVisibleObjects, parts);
        }

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
            public float rightWayPos = 0f;
            public float leftWayPos = 0f;
        }

        /// <summary>
        /// レーンの編集
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="work"></param>
        public void EditLane(RnLane lane, Work work)
        {
            if (lane == null)
                return;
            var p = laneEdit;
            ShowBase(lane);
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

                lane.IsReverse = EditorGUILayout.Toggle("IsReverse", lane.IsReverse);
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

            public HashSet<object> Foldouts { get; } = new HashSet<object>();
        }

        /// <summary>
        /// 道路の編集
        /// </summary>
        /// <param name="road"></param>
        /// <param name="work"></param>
        private void EditRoad(RnRoad road, Work work)
        {
            var p = roadEdit;
            if (road == null)
                return;

            ShowBase(road);
            using (new EditorGUI.DisabledScope(false))
            {
                using var _ = (new EditorGUILayout.HorizontalScope());
                EditorGUILayout.LabelField($"Prev: {road.Prev.GetDebugLabelOrDefault()} | Next: {road.Next.GetDebugLabelOrDefault()}");
                EditorGUILayout.LabelField($"LaneCount L({road.GetLeftLaneCount()})-R({road.GetRightLaneCount()})");
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("Lanes", p.Foldouts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var lane in road.AllLanesWithMedian)
                {
                    var foldout =
                        RnEditorUtil.Foldout($"{(lane.IsMedianLane ? "[Median]" : "")}{lane.GetDebugLabelOrDefault()}",
                            p.Foldouts, lane);
                    if (!foldout)
                        continue;

                    RnEditorUtil.Separator();
                    using var _ = new EditorGUI.IndentLevelScope();
                    EditLane(lane, work);
                }
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("SideWalks", p.Foldouts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var sideWalk in road.SideWalks)
                {
                    var foldout = RnEditorUtil.Foldout(sideWalk.GetDebugLabelOrDefault(), p.Foldouts, sideWalk);
                    if (!foldout)
                        continue;
                    using var _ = new EditorGUI.IndentLevelScope();
                    RnEditorUtil.Separator();
                    EditSideWalk(sideWalk, work);
                }
            }

            var roadGroup = road.CreateRoadGroupOrDefault();
            if (RnEditorUtil.Foldout("RoadGroupOption", p.Foldouts, ("RoadGroupOption", road)))
            {
                if (GUILayout.Button("Align"))
                {
                    roadGroup.Align();
                }

                EditorGUILayout.LabelField($"LaneCount");
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        p.leftLaneCount = EditorGUILayout.IntField($"Left({roadGroup.GetLeftLaneCount()}) ->", roadGroup.GetLeftLaneCount());
                        if (GUILayout.Button("Change Left"))
                            work.DelayExec.Add(() => roadGroup.SetLeftLaneCount(p.leftLaneCount));
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        p.rightLaneCount = EditorGUILayout.IntField($"Right({roadGroup.GetRightLaneCount()}) -> ", p.rightLaneCount);
                        if (GUILayout.Button("Change Right"))
                            work.DelayExec.Add(() => roadGroup.SetRightLaneCount(p.rightLaneCount));
                    }
                    if (GUILayout.Button("Change Both"))
                    {
                        roadGroup.SetLaneCount(p.leftLaneCount, p.rightLaneCount);
                    }
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

            }
            if (RnEditorUtil.Foldout("Option", p.Foldouts, ("Option", road)))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                if (GUILayout.Button("DisConnect"))
                {
                    work.DelayExec.Add(() => road.DisConnect(false));
                }

                if (GUILayout.Button("Convert2Intersection"))
                {
                    work.DelayExec.Add(() => road.ParentModel.Convert2Intersection(road));
                }

            }
        }
        private class IntersectionEdit
        {
            public long convertPrevRoadId = -1;
            public long convertNextRoadId = -1;

            public HashSet<object> Foldouts { get; } = new HashSet<object>();

            // Trackのスクロール位置
            public Vector2 trackScrollPosition;
        }

        public void EditIntersection(RnIntersection intersection, Work work)
        {
            if (intersection == null)
                return;
            var p = intersectionEdit;

            ShowBase(intersection);
            using (new EditorGUI.DisabledScope(false))
            {
                if (RnEditorUtil.Foldout("Borders", p.Foldouts, ("Borders", intersection)))
                {
                    foreach (var b in intersection.Neighbors)
                    {
                        using var _ = new EditorGUI.IndentLevelScope();
                        EditorGUILayout.LabelField($"{b.Road.GetDebugLabelOrDefault()}/{b.Border.GetDebugIdLabelOrDefault()}");
                    }
                }
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("Tracks", p.Foldouts, ("Tracks", intersection)))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                if (GUILayout.Button("Build Track"))
                {
                    intersection.BuildTracks();
                }

                using var scope = new EditorGUILayout.ScrollViewScope(p.trackScrollPosition);
                p.trackScrollPosition = scope.scrollPosition;
                foreach (var track in intersection.Tracks)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        void ShowLabel(string label, RnNeighbor edge)
                        {
                            var lane = edge.GetConnectedLane();
                            EditorGUILayout.LabelField($"{label}:{edge.Border.GetDebugIdLabelOrDefault()}/{edge?.Road?.GetDebugLabelOrDefault()}]/{lane.GetDebugLabelOrDefault()}");
                        }
                        EditorGUILayout.LabelField($"{track.GetDebugLabelOrDefault()}");
                        ShowLabel("From", intersection.FindEdges(track.FromBorder).FirstOrDefault());
                        ShowLabel("To", intersection.FindEdges(track.ToBorder).FirstOrDefault());
                        track.TurnType = (RnTurnType)EditorGUILayout.EnumPopup("TurnType", track.TurnType);
                    }
                }
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("SideWalks", p.Foldouts, ("SideWalks", intersection)))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var sideWalk in intersection.SideWalks)
                {
                    var foldout = RnEditorUtil.Foldout(sideWalk.GetDebugLabelOrDefault(), p.Foldouts, sideWalk);
                    if (!foldout)
                        continue;
                    using var _ = new EditorGUI.IndentLevelScope();
                    RnEditorUtil.Separator();
                    EditSideWalk(sideWalk, work);
                }
            }

            if (RnEditorUtil.Foldout("Option", p.Foldouts, ("Option", intersection)))
            {
                using var indent = new EditorGUI.IndentLevelScope();
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
                        work.DelayExec.Add(() => intersection.ParentModel.Convert2Road(intersection, prev, next));
                    }
                }
                if (GUILayout.Button("DisConnect"))
                {
                    work.DelayExec.Add(() => intersection.DisConnect(false));
                }

            }

        }

        public class SideWalkEdit
        {
        }

        public void EditSideWalk(RnSideWalk sideWalk, Work work)
        {
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.LabelField($"ParentRoad:{sideWalk.ParentRoad.GetDebugMyIdOrDefault()}");
            }
        }

        /// <summary>
        /// 選択/非表示オブジェクトなどの情報を削除する
        /// </summary>
        public void ClearPickedObjects()
        {
            if (InstanceHelper == null)
                return;
            FoldOuts.Clear();
            InstanceHelper.SelectedObjects?.Clear();
            InstanceHelper.InVisibleObjects?.Clear();
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
            var work = new Work();
            if (GUILayout.Button("Clear Picked Objects"))
            {
                ClearPickedObjects();
            }

            bool isAdded;
            using (new EditorGUILayout.HorizontalScope())
            {
                addTargetType = (AddTargetType)EditorGUILayout.EnumPopup("AddTargetType", addTargetType);
                addTargetId = EditorGUILayout.LongField("AddTarget", addTargetId);
                isAdded = GUILayout.Button("+");
            }

            RnEditorUtil.Separator();
            InstanceHelper.SelectedObjects.RemoveWhere(s =>
            {
                if (s is RnRoad && model.Roads.Contains(s) == false)
                    return true;
                if (s is RnIntersection && model.Intersections.Contains(s) == false)
                    return true;
                return false;
            });

            if (RnEditorUtil.Foldout($"Roads[{model.Roads.Count}]", FoldOuts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var r in model.Roads)
                {
                    if (addTargetType == AddTargetType.Road && isAdded && r.DebugMyId == (ulong)addTargetId)
                        InstanceHelper.SelectedObjects.Add(r);
                    if (IsSceneSelected(r) == false && InstanceHelper.SelectedObjects.Contains(r) == false)
                        continue;

                    if (RnEditorUtil.Foldout($"Road {r.GetDebugMyIdOrDefault()}", FoldOuts, r) == false)
                        continue;

                    RnEditorUtil.Separator();
                    using var _ = new EditorGUI.IndentLevelScope();
                    EditRoad(r, work);
                }
            }


            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout($"Intersections[{model.Intersections.Count}]", FoldOuts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var i in model.Intersections)
                {
                    if (addTargetType == AddTargetType.Intersection && isAdded && i.DebugMyId == (ulong)addTargetId)
                        InstanceHelper.SelectedObjects.Add(i);
                    if (IsSceneSelected(i) == false && InstanceHelper.SelectedObjects.Contains(i) == false)
                        continue;

                    if (RnEditorUtil.Foldout(i.GetDebugLabelOrDefault(), FoldOuts, i) == false)
                        continue;

                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        FoldOuts.Add(i);
                        EditIntersection(i, work);
                    }
                }
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout($"SideWalks[{model.SideWalks.Count}]", FoldOuts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var sw in model.SideWalks)
                {
                    if (addTargetType == AddTargetType.SideWalk && isAdded && sw.DebugMyId == (ulong)addTargetId)
                        InstanceHelper.SelectedObjects.Add(sw);
                    if (IsSceneSelected(sw) == false && InstanceHelper.SelectedObjects.Contains(sw) == false)
                        continue;
                    if (RnEditorUtil.Foldout($"{sw.GetDebugLabelOrDefault()}", FoldOuts, sw) == false)
                        continue;
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        FoldOuts.Add(sw);
                        EditSideWalk(sw, work);
                    }
                }
            }

            // 選択したレーン情報は表示しておく
            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("Picked Lanes", FoldOuts))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                var pickedLanes = InstanceHelper.SelectedObjects.Select(x => x as RnLane).Where(x => x != null)
                    .ToList();

                //addTargetId = RnEditorUtil.CheckAddTarget(InstanceHelper.TargetLanes, this.addTargetId, out var isAddLane);
                // 内部でTargetLanesを更新するため、ToListでコピーを取得
                foreach (var l in pickedLanes)
                {
                    EditLane(l, work);
                }
            }

            RnEditorUtil.Separator();
            if (RnEditorUtil.Foldout("Option", FoldOuts))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create Empty Road"))
                        model.CreateEmptyRoadBetweenInteraction();

                    if (GUILayout.Button("Remove Empty Road"))
                        model.RemoveEmptyRoadBetweenIntersection();

                    if (GUILayout.Button("Create Empty Intersection"))
                        model.CreateEmptyIntersectionBetweenRoad();

                    if (GUILayout.Button("Remove Empty Intersection"))
                        model.RemoveEmptyIntersectionBetweenRoad();
                }
            }


            foreach (var e in work.DelayExec)
                e.Invoke();
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

        /// <summary>
        /// Scene上で選択されているかどうか
        /// </summary>
        /// <param name="roadBase"></param>
        /// <returns></returns>
        public static bool IsSceneSelected(RnRoadBase roadBase)
        {
            if (roadBase == null)
                return false;
            return RnEx.IsEditorSceneSelected(roadBase.CityObjectGroup);
        }
        /// <summary>
        /// Scene上で選択されているかどうか
        /// </summary>
        /// <param name="sideWalk"></param>
        /// <returns></returns>
        public static bool IsSceneSelected(RnSideWalk sideWalk)
        {
            return IsSceneSelected(sideWalk.ParentRoad);
        }
    }
}