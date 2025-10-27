using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Structure
{
    internal class RnModelDebugEditorWindow : EditorWindow
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
            LineString,
        }

        public class Work
        {
            // foreach文で回している最中に実行するとまずいもの(リストの変換等)の遅延実行用
            public List<Action> DelayExec { get; } = new();

            public RnModelDebugEditorWindow Self { get; }

            public Work(RnModelDebugEditorWindow self)
            {
                Self = self;
            }
        }

        private void ShowBase(ARnPartsBase parts)
        {
            using var _ = new EditorGUILayout.HorizontalScope();
            RnEditorUtil.SelectToggle($"select", InstanceHelper.SelectedObjects, parts);
            RnEditorUtil.VisibleToggle(InstanceHelper.InVisibleObjects, parts);
        }

        private class LaneEdit
        {
            private class LaneSplitEdit
            {
                public int splitNum = 2;
            }

            private LaneSplitEdit splitEdit = new LaneSplitEdit();

            private class LaneWidthEdit
            {
                public LaneWayMoveOption moveOption = LaneWayMoveOption.MoveBothWay;
                public float width = 0f;
                public float moveWidth = 0f;
            }

            private LaneWidthEdit widthEdit = new LaneWidthEdit();
            private float rightWayPos = 0f;
            private float leftWayPos = 0f;


            /// <summary>
            /// レーンの編集
            /// </summary>
            /// <param name="lane"></param>
            /// <param name="work"></param>
            public void Update(RnLane lane, Work work)
            {
                if (lane == null)
                    return;

                work.Self.ShowBase(lane);
                using (new EditorGUI.DisabledScope(false))
                {
                    void Draw(RnLaneBorderType type)
                    {
                        RnEditorUtil.Separator();
                        EditorGUILayout.LabelField($"Border {type}");
                        using (new EditorGUI.IndentLevelScope())
                        {
                            var border = lane.GetBorder(type);
                            EditorGUILayout.LabelField($"BorderWay {border?.GetDebugIdLabelOrDefault()}");
                            EditorGUILayout.LabelField($"Connect Lanes [{lane.GetConnectedLanes(type).Select(l => l.GetDebugLabelOrDefault()).Join2String()}]");
                            EditorGUILayout.LabelField($"Connect Roads [{lane.GetConnectedRoads(type).Select(l => l.GetDebugLabelOrDefault()).Join2String()}]");
                        }
                    }

                    lane.IsReversed = EditorGUILayout.Toggle("IsReverse", lane.IsReversed);
                    lane.Attributes = (RnLaneAttribute)EditorGUILayout.EnumFlagsField("Attribute", lane.Attributes);
                    Draw(RnLaneBorderType.Prev);
                    Draw(RnLaneBorderType.Next);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Left Way {lane.LeftWay.GetDebugIdLabelOrDefault()}");
                        EditorGUILayout.LabelField($"Right Way {lane.RightWay.GetDebugIdLabelOrDefault()}");
                    }
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



        private class RoadEdit
        {
            // 左側レーン数
            private int leftLaneCount = -1;
            // 右側レーン数
            private int rightLaneCount = -1;

            // 中央分離帯幅
            private float medianWidth = 0;
            private LaneWayMoveOption medianWidthOption = LaneWayMoveOption.MoveBothWay;

            private HashSet<object> Foldouts { get; } = new HashSet<object>();

            /// <summary>
            /// 道路の編集
            /// </summary>
            /// <param name="road"></param>
            /// <param name="work"></param>
            public void Update(RnRoad road, Work work)
            {
                if (road == null)
                    return;

                work.Self.EditRoadBase(road, work);
                using (new EditorGUI.DisabledScope(false))
                {
                    using var _ = (new EditorGUILayout.HorizontalScope());
                    EditorGUILayout.LabelField($"Prev: {road.Prev.GetDebugLabelOrDefault()} | Next: {road.Next.GetDebugLabelOrDefault()}");
                    EditorGUILayout.LabelField($"LaneCount L({road.GetLeftLaneCount()})-R({road.GetRightLaneCount()})");
                }

                RnEditorUtil.Separator();
                if (RnEditorUtil.Foldout("Lanes", Foldouts))
                {
                    using var indent = new EditorGUI.IndentLevelScope();
                    foreach (var lane in road.AllLanesWithMedian)
                    {
                        var foldout =
                            RnEditorUtil.Foldout($"{(lane.IsMedianLane ? "[Median]" : "")}{lane.GetDebugLabelOrDefault()}",
                                Foldouts, lane);
                        if (!foldout)
                            continue;

                        RnEditorUtil.Separator();
                        using var _ = new EditorGUI.IndentLevelScope();
                        work.Self.laneEdit.Update(lane, work);
                    }
                }

                RnEditorUtil.Separator();
                if (RnEditorUtil.Foldout("SideWalks", Foldouts))
                {
                    using var indent = new EditorGUI.IndentLevelScope();
                    foreach (var sideWalk in road.SideWalks)
                    {
                        var foldout = RnEditorUtil.Foldout(sideWalk.GetDebugLabelOrDefault(), Foldouts, sideWalk);
                        if (!foldout)
                            continue;
                        using var _ = new EditorGUI.IndentLevelScope();
                        RnEditorUtil.Separator();
                        work.Self.sideWalkEdit.Update(sideWalk, work);
                    }
                }

                var roadGroup = road.CreateRoadGroupOrDefault();
                if (RnEditorUtil.Foldout($"RoadGroupOption [{roadGroup.Roads.Count}] ({roadGroup.Roads.Select(x => x.GetDebugLabelOrDefault()).Join2String()})", Foldouts, ("RoadGroupOption", road)))
                {
                    if (GUILayout.Button("Align"))
                    {
                        work.DelayExec.Add(() => roadGroup.Align());
                    }

                    if (GUILayout.Button("Merge"))
                    {
                        work.DelayExec.Add(() => roadGroup.MergeRoads());
                    }

                    EditorGUILayout.LabelField($"LaneCount");
                    using (new EditorGUI.IndentLevelScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            leftLaneCount = EditorGUILayout.IntField($"Left({roadGroup.GetLeftLaneCount()}) ->", leftLaneCount);
                            if (GUILayout.Button("Change Left"))
                                work.DelayExec.Add(() => roadGroup.SetLeftLaneCount(leftLaneCount));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            rightLaneCount = EditorGUILayout.IntField($"Right({roadGroup.GetRightLaneCount()}) -> ", rightLaneCount);
                            if (GUILayout.Button("Change Right"))
                                work.DelayExec.Add(() => roadGroup.SetRightLaneCount(rightLaneCount));
                        }
                        if (GUILayout.Button("Change Both"))
                        {
                            roadGroup.SetLaneCount(leftLaneCount, rightLaneCount);
                        }
                    }



                    if (roadGroup.HasMedian())
                    {
                        medianWidthOption = (LaneWayMoveOption)EditorGUILayout.EnumPopup("MoveOption", medianWidthOption);
                        if (GUILayout.Button("RemoveMedian"))
                        {
                            roadGroup.RemoveMedian(medianWidthOption);
                        }
                    }
                    else
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            medianWidth = EditorGUILayout.FloatField("MedianWidth", medianWidth);
                            if (GUILayout.Button("CreateMedian"))
                            {
                                roadGroup.CreateMedianOrSkip(medianWidth);
                            }
                        }
                    }

                    if (GUILayout.Button("Adjust Border"))
                    {
                        work.DelayExec.Add(() => roadGroup.AdjustBorder());
                    }
                }

                if (RnEditorUtil.Foldout("Option", Foldouts, ("Option", road)))
                {
                    using var indent = new EditorGUI.IndentLevelScope();
                    if (GUILayout.Button("DisConnect"))
                    {
                        work.DelayExec.Add(() => road.DisConnect(true));
                    }
                    if (GUILayout.Button("SeparateContinuousBorder"))
                    {
                        work.DelayExec.Add(() => road.SeparateContinuousBorder());
                    }
                    if (GUILayout.Button("Convert2Intersection"))
                    {
                        work.DelayExec.Add(() => road.ParentModel.Convert2Intersection(road));
                    }

                    if (road.Next is RnIntersection && GUILayout.Button("Merge2NextIntersection"))
                    {
                        work.DelayExec.Add(() => road.TryMerge2NeighborIntersection(RnLaneBorderType.Next));
                    }
                    if (road.Prev is RnIntersection && GUILayout.Button("Merge2PrevIntersection"))
                    {
                        work.DelayExec.Add(() => road.TryMerge2NeighborIntersection(RnLaneBorderType.Prev));
                    }

                    if (GUILayout.Button("TrySliceRoadHorizontalNearByBorder"))
                    {
                        work.DelayExec.Add(() => road.ParentModel.TrySliceRoadHorizontalNearByBorder(
                            road, new RnModelEx.CalibrateIntersectionBorderOption()
                            , out var prev
                            , out var center
                            , out var next
                            ));
                    }
                    if (GUILayout.Button("CalibrateIntersectionBorder"))
                    {
                        work.DelayExec.Add(() => road.ParentModel.CalibrateIntersectionBorder(road, new RnModelEx.CalibrateIntersectionBorderOption()));
                    }
                }
            }
        }

        private void EditRoadBase(RnRoadBase roadBase, Work work)
        {
            if (roadBase == null)
                return;
            ShowBase(roadBase);
            EditorGUILayout.LabelField($"Check : {roadBase.Check()}");
            if (RnEditorUtil.Foldout("TargetTrans", FoldOuts, (roadBase, "TargetTrans")))
            {
                using var indent = new EditorGUI.IndentLevelScope();
                foreach (var tran in roadBase.TargetGroupKeys)
                {
                    RnEditorUtil.RnCityObjectGroupField(tran);
                }
            }

            if (GUILayout.Button("SideWalk Reduction"))
            {
                roadBase.MergeConnectedSideWalks();
            }

            if (GUILayout.Button("Merge Same LineString"))
            {
                roadBase.MergeSamePointLineStrings();
            }
        }

        /// <summary>
        /// 交差点編集
        /// </summary>
        private class IntersectionEdit
        {
            /// <summary>
            /// 交差点の辺の編集
            /// </summary>
            private class IntersectionEdgeEdit
            {
                public void Update(RnIntersectionEdge edge, Work work)
                {
                    if (edge == null)
                        return;
                    work.Self.ShowBase(edge);
                }
            }

            private long convertPrevRoadId = -1;
            private long convertNextRoadId = -1;

            private bool devTrackAllowCenterLine = true;
            private HashSet<object> Foldouts { get; } = new HashSet<object>();

            private bool showBorderEdgeOnly = false;

            // Trackのスクロール位置
            private Vector2 trackScrollPosition;

            private IntersectionEdgeEdit edgeEdit = new();

            public void Update(RnIntersection intersection, Work work)
            {
                work.Self.EditRoadBase(intersection, work);
                using (new EditorGUI.DisabledScope(false))
                {
                    if (RnEditorUtil.Foldout("Edges", Foldouts, ("Edges", intersection)))
                    {
                        using var _2 = new EditorGUI.IndentLevelScope();
                        showBorderEdgeOnly = EditorGUILayout.Toggle("Show Border Only", showBorderEdgeOnly);
                        foreach (var e in intersection.Edges)
                        {
                            if (e.IsBorder == false && showBorderEdgeOnly)
                                continue;
                            var label =
                                $"{e.GetDebugLabelOrDefault()}{e.Road.GetDebugLabelOrDefault()}/{e.Border.GetDebugIdLabelOrDefault()}-{e.IsMedianBorder}";
                            if (RnEditorUtil.Foldout(label, Foldouts, e))
                            {
                                using var _ = new EditorGUI.IndentLevelScope();
                                edgeEdit.Update(e, work);
                            }
                        }
                    }
                }

                RnEditorUtil.Separator();
                if (RnEditorUtil.Foldout("Tracks", Foldouts, ("Tracks", intersection)))
                {
                    using var indent = new EditorGUI.IndentLevelScope();

                    using (var _ = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Build Track"))
                        {
                            intersection.BuildTracks(new BuildTrackOption
                            {
                                DevAllowCenterLine = devTrackAllowCenterLine
                            });
                        }
                        devTrackAllowCenterLine = EditorGUILayout.Toggle("Allow Center Line", devTrackAllowCenterLine);
                    }

                    using var scope = new EditorGUILayout.ScrollViewScope(trackScrollPosition);
                    trackScrollPosition = scope.scrollPosition;
                    foreach (var track in intersection.Tracks)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            void ShowLabel(string label, RnIntersectionEdge edge)
                            {
                                var lane = edge.GetConnectedLane();
                                EditorGUILayout.LabelField($"{label}:{edge.Border.GetDebugIdLabelOrDefault()}/{edge?.Road?.GetDebugLabelOrDefault()}]/{lane.GetDebugLabelOrDefault()}");
                            }
                            EditorGUILayout.LabelField($"{track.GetDebugLabelOrDefault()}");
                            ShowLabel("From", intersection.FindEdges(track.FromBorder).FirstOrDefault());
                            ShowLabel("To", intersection.FindEdges(track.ToBorder).FirstOrDefault());
                            track.TurnType = (RnTurnType)EditorGUILayout.EnumPopup("TurnType", track.TurnType);
                            if (GUILayout.Button("Build Track"))
                            {
                                work.DelayExec.Add(() =>
                                {
                                    var op = new BuildTrackOption();
                                    op.TargetBorderLineStrings.Add(track.FromBorder.LineString);
                                    op.TargetBorderLineStrings.Add(track.ToBorder.LineString);
                                    intersection.BuildTracks(op);
                                });
                            }
                        }
                    }
                }

                RnEditorUtil.Separator();
                if (RnEditorUtil.Foldout("SideWalks", Foldouts, ("SideWalks", intersection)))
                {
                    using var indent = new EditorGUI.IndentLevelScope();
                    foreach (var sideWalk in intersection.SideWalks)
                    {
                        var foldout = RnEditorUtil.Foldout(sideWalk.GetDebugLabelOrDefault(), Foldouts, sideWalk);
                        if (!foldout)
                            continue;
                        using var _ = new EditorGUI.IndentLevelScope();
                        RnEditorUtil.Separator();
                        work.Self.sideWalkEdit.Update(sideWalk, work);
                    }
                }

                if (RnEditorUtil.Foldout("Option", Foldouts, ("Option", intersection)))
                {
                    using var indent = new EditorGUI.IndentLevelScope();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        convertPrevRoadId = EditorGUILayout.LongField("PrevRoadId", convertPrevRoadId);
                        convertNextRoadId = EditorGUILayout.LongField("NextRoadId", convertNextRoadId);

                        var prev = intersection.Borders.Select(n => n.Road)
                            .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)convertPrevRoadId);
                        var next = intersection.Borders.Select(n => n.Road)
                            .FirstOrDefault(r => r != null && r.DebugMyId == (ulong)convertNextRoadId);

                        if (GUILayout.Button("Convert2Road"))
                        {
                            work.DelayExec.Add(() => intersection.ParentModel.Convert2Road(intersection, prev, next));
                        }
                    }
                    if (GUILayout.Button("DisConnect"))
                    {
                        work.DelayExec.Add(() => intersection.DisConnect(true));
                    }

                    if (GUILayout.Button("SeparateContinuousBorder"))
                    {
                        work.DelayExec.Add(intersection.SeparateContinuousBorder);
                    }

                    if (GUILayout.Button("Create Inside Objects"))
                    {
                        List<RnPoint> points = new List<RnPoint>(intersection.Edges.Sum(x => x.Border.Count));
                        foreach (var pos in intersection.Edges.SelectMany(e => e.Border.Points))
                        {
                            if (points.Any() && points.Last() == pos)
                                continue;
                            points.Add(pos);
                        }
                        if (points.Count > 1 && points[0] == points[^1])
                            points.RemoveAt(points.Count - 1);

                        var obj = new GameObject("InsideObjects");
                        foreach (var pos in points)
                        {
                            var go = new GameObject("Point");
                            go.transform.SetParent(obj.transform);
                            go.transform.position = pos.Vertex;
                        }
                    }

                    if (GUILayout.Button("Align"))
                        intersection.Align();
                }

            }

        }


        public class SideWalkEdit
        {
            public void Update(RnSideWalk sideWalk, Work work)
            {
                work.Self.ShowBase(sideWalk);
                using (new EditorGUI.DisabledScope(false))
                {
                    EditorGUILayout.LabelField($"ParentRoad:{sideWalk.ParentRoad.GetDebugMyIdOrDefault()}");
                    EditorGUILayout.EnumPopup("LaneType", sideWalk.LaneType);
                }

                EditorGUILayout.LabelField($"Outside Way {sideWalk.OutsideWay.GetDebugIdLabelOrDefault()}");
                EditorGUILayout.LabelField($"Inside Way {sideWalk.InsideWay.GetDebugIdLabelOrDefault()}");
                EditorGUILayout.LabelField($"StartEdge Way {sideWalk.StartEdgeWay.GetDebugIdLabelOrDefault()}");
                EditorGUILayout.LabelField($"EndEdge Way {sideWalk.EndEdgeWay.GetDebugIdLabelOrDefault()}");
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
            var work = new Work(this);
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

            if (isAdded)
            {
                if (addTargetType == AddTargetType.LineString)
                {
                    var ls = model.CollectAllLineStrings()
                        .FirstOrDefault(ls => ls != null && ls.DebugMyId == (ulong)addTargetId);
                    if (ls != null)
                        InstanceHelper.SelectedObjects.Add(ls);
                }

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
                    roadEdit.Update(r, work);
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
                        intersectionEdit.Update(i, work);
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
                        work.Self.sideWalkEdit.Update(sw, work);
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
                    work.Self.laneEdit.Update(l, work);
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

                    if (GUILayout.Button("Collect Check"))
                    {
                        var work2 = new CollectRnModelWork();
                        using (new DebugTimer("Collect[Generated Code]"))
                        {
                            model.Collect(work2);
                        }
                        CollectRnModelWork work1 = null;
                        using (new DebugTimer("Collect[Reflection]"))
                        {
                            work1 = Collect(model);
                        }

                        bool success = true;
                        void Check<T>(HashSet<T> a, HashSet<T> b)
                        {
                            var name = typeof(T).Name;
                            var aList = a.OrderBy(x => x.GetHashCode()).ToList();
                            var bList = b.OrderBy(x => x.GetHashCode()).ToList();
                            if (aList.Count != bList.Count)
                            {
                                Debug.LogError($"{name} Count Error {aList.Count} != {bList.Count}");
                                success = false;
                                return;
                            }

                            for (var i = 0; i < aList.Count; i++)
                            {
                                if (aList[i].Equals(bList[i]) == false)
                                {
                                    Debug.LogError($"{name} Error {aList[i]} != {bList[i]}");
                                    success = false;
                                }
                            }
                        }

                        Check(work1.RnLanes, work2.RnLanes);
                        Check(work1.RnLineStrings, work2.RnLineStrings);
                        Check(work1.RnPoints, work2.RnPoints);
                        Check(work1.RnRoadBases, work2.RnRoadBases);
                        Check(work1.RnSideWalks, work2.RnSideWalks);
                        Check(work1.RnWays, work2.RnWays);
                        Check(work1.TrafficSignalControllerPatterns, work2.TrafficSignalControllerPatterns);
                        Check(work1.TrafficSignalControllerPhases, work2.TrafficSignalControllerPhases);
                        Check(work1.TrafficSignalLights, work2.TrafficSignalLights);
                        Check(work1.TrafficSignalLightControllers, work2.TrafficSignalLightControllers);
                        if (success)
                            DebugEx.Log("Collect Check Success");
                    }

                    if (GUILayout.Button("Edge Reduction"))
                    {
                        foreach (var inter in model.Intersections)
                            inter.MergeContinuousNonBorderEdge();
                    }
                }
            }


            foreach (var e in work.DelayExec)
                e.Invoke();
        }

        internal static CollectRnModelWork Collect(RnModel roadNetworkModel)
        {
            void Impl<T>(HashSet<T> hashSet) where T : class
            {
                var src = TypeUtil
                    .GetAllMembersRecursively(roadNetworkModel, typeof(T), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(x => x.Item2)
                    .Where(x => x != null)
                    .Distinct()
                    .ToList();

                foreach (var obj in src)
                {
                    hashSet.Add((T)obj);
                }
            }

            var ret = new CollectRnModelWork();
            Impl(ret.TrafficSignalLightControllers);
            Impl(ret.TrafficSignalLights);
            Impl(ret.TrafficSignalControllerPatterns);
            Impl(ret.TrafficSignalControllerPhases);
            Impl(ret.RnPoints);
            Impl(ret.RnLineStrings);
            Impl(ret.RnLanes);
            Impl(ret.RnRoadBases);
            Impl(ret.RnWays);
            Impl(ret.RnSideWalks);
            return ret;
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
            return roadBase.TargetGroupKeys.Any(RnEx.IsEditorSceneSelected);
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