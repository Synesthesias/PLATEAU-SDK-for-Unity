using PLATEAU.Editor.RoadNetwork.AddSystem;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.AddSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    internal class RnRoadAddSystem : RnSplineSystemBase, ICreatedSplineReceiver
    {
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// 道路追加後のアクション
        /// </summary>
        public Action<List<RnRoadBase>> OnRoadAdded { get; set; }

        private SplineCreateHandles splineCreateHandles;

        private RnSkeletonHandles extensiblePointHandles;

        private RoadNetworkAddSystemContext context;

        private ExtensibleIntersectionEdge selectedIntersection;
        private ExtensibleRoadEdge selectedRoad;
        private bool isRoadSelected = false;

        public RnRoadAddSystem(RoadNetworkAddSystemContext context)
        {
            this.context = context;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore, this);
            extensiblePointHandles = new RnSkeletonHandles(context);
            extensiblePointHandles.OnRoadSelected = (edge) =>
            {
                selectedRoad = edge;
                isRoadSelected = true;

                // スプラインの終点候補の設定
                //var endPoints = new List<SplineEndPoint>();
                //foreach (var intersectionSkeleton in context.SkeletonData.Intersections)
                //{
                //    intersectionSkeleton.ExtensibleEdges.ForEach(e =>
                //    {
                //        endPoints.Add(new SplineEndPoint
                //        {
                //            position = e.center,
                //            tangent = -e.forward
                //        });
                //    });
                //}

                //splineCreateHandles.SetEndPoints(endPoints);

                // 作図開始
                splineCreateHandles.BeginCreateSpline(edge.center, edge.forward);
            };
            extensiblePointHandles.OnIntersectionSelected = (edge) =>
            {
                selectedIntersection = edge;
                isRoadSelected = false;

                // スプラインの終点候補の設定(始点は外す)
                //var endPoints = new List<SplineEndPoint>();
                //foreach (var intersectionSkeleton in context.SkeletonData.Intersections)
                //{
                //    intersectionSkeleton.ExtensibleEdges.ForEach(e =>
                //    {
                //        if (e.intersection == edge.intersection)
                //            return;

                //        endPoints.Add(new SplineEndPoint
                //        {
                //            position = e.center,
                //            tangent = -e.forward
                //        });
                //    });
                //}

                //splineCreateHandles.SetEndPoints(endPoints);

                // 作図開始
                splineCreateHandles.BeginCreateSpline(edge.center, edge.forward);
            };
        }

        public void Activate()
        {
            IsActive = true;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore, this);
        }

        public void Deactivate()
        {
            IsActive = false;
            splineEditorCore.Reset();
            splineCreateHandles = null;
        }

        public override void HandleSceneGUI(UnityEngine.Object target)
        {
            if (!IsActive)
                return;

            // 1. 頂点をSphereで描画＆クリックしたらスプライン作図開始
            if (!splineCreateHandles.IsCreatingSpline)
                extensiblePointHandles.HandleSceneGUI(target, true, true);

            // 2. SplineCreateHandles でノット追加＆移動を処理
            splineCreateHandles.HandleSceneGUI();
        }

        /// <summary>
        /// スプライン作図が完了したタイミングで呼ばれる
        /// → クリック時に記憶しておいた `selectedRoad` を用いて、
        ///    その道路をスプラインに反映する
        /// </summary>
        public void OnSplineCreated(Spline spline)
        {
            var dirtyObjects = new List<RnRoadBase>();
            if (isRoadSelected)
            {
                var edgeInfo = new RnRoadEdgeMaker(selectedRoad.road.Roads[0]).Execute(selectedRoad);
                ExtendRoadAlongSpline(edgeInfo, spline);
                dirtyObjects.Add(selectedRoad.road.Roads[0]);
            }
            else
            {
                dirtyObjects = AddRoadAlongSpline(selectedIntersection, spline);
            }
            splineEditorCore.Reset();
            OnRoadAdded?.Invoke(dirtyObjects);
        }

        public void OnSplineCreatCanceled()
        {
            splineEditorCore.Reset();
        }

        /// <summary>
        /// Splineに沿って道路を拡張する。
        /// </summary>
        /// <param name="targetRoad"></param>
        /// <param name="spline"></param>
        private void ExtendRoadAlongSpline(RoadEdgeInfo edgeInfo, Spline spline)
        {
            var road = edgeInfo.Edge.road.Roads[0];

            var scannedLineStrings = new HashSet<RnLineString>();

            foreach (var lane in road.AllLanesWithMedian)
            {
                var oldEdgePoints = new List<RnPoint>();
                var newEdgePoints = new List<RnPoint>();
                int i = 0;
                foreach (var way in new[] { lane.LeftWay, lane.RightWay })
                {
                    if (!scannedLineStrings.Contains(way.LineString))
                    {
                        ExtendPointsAlongSpline(way.LineString.Points, spline, edgeInfo.Edge.isPrev ^ lane.IsReverse ^ way.IsReversed);
                        scannedLineStrings.Add(way.LineString);
                    }
                }

                var newEdge = new RnWay(new RnLineString(new List<RnPoint> {
                    edgeInfo.Edge.isPrev ^ lane.IsReverse ^ lane.RightWay.IsReversed ? lane.RightWay.LineString.Points.First() : lane.RightWay.LineString.Points.Last(),
                    edgeInfo.Edge.isPrev ^ lane.IsReverse ^ lane.LeftWay.IsReversed ? lane.LeftWay.LineString.Points.First() : lane.LeftWay.LineString.Points.Last()
                }));
                // ボーダー再構築
                if (edgeInfo.Edge.isPrev ^ lane.IsReverse)
                    lane.SetBorder(RnLaneBorderType.Prev, newEdge);
                else
                    lane.SetBorder(RnLaneBorderType.Next, newEdge);
            }

            if (edgeInfo.LeftSideWalkEdge.SideWalk != null)
                ExtendSideWalk(edgeInfo.LeftSideWalkEdge, spline, scannedLineStrings, road, true, edgeInfo.Edge.isPrev);
            if (edgeInfo.RightSideWalkEdge.SideWalk != null)
                ExtendSideWalk(edgeInfo.RightSideWalkEdge, spline, scannedLineStrings, road, false, edgeInfo.Edge.isPrev);

            //foreach (var way in GetAllWaysAlongRoad(road, isPrev))
            //{
            //    //ExtendPointsAlongSpline(way.Item2.LineString.Points, spline, way.Item1 ^ isPrev);
            //}
        }

        private void ExtendSideWalk(SideWalkEdgeInfo sideWalkEdgeInfo, Spline spline, HashSet<RnLineString> scannedLineStrings, RnRoad road, bool isLeft, bool isPrev)
        {
            var insideWay = sideWalkEdgeInfo.SideWalk.InsideWay;
            if (!scannedLineStrings.Contains(insideWay.LineString))
            {
                List<RnPoint> orderedLanePoints;
                if (isLeft)
                {
                    var laneWay = road.GetLeftWayOfLanes();
                    orderedLanePoints = new List<RnPoint>(laneWay.LineString.Points);
                    if (isPrev ^ road.MainLanes[0].IsReverse ^ laneWay.IsReversed)
                        orderedLanePoints.Reverse();
                    var index = orderedLanePoints.FindIndex(p => p.Vertex == (sideWalkEdgeInfo.IsInsidePrev ? sideWalkEdgeInfo.Edge.LineString.Points.First().Vertex : sideWalkEdgeInfo.Edge.LineString.Points.Last().Vertex));
                    for (; index < orderedLanePoints.Count; index++)
                    {
                        if (sideWalkEdgeInfo.IsInsidePrev)
                            insideWay.LineString.Points.Insert(0, orderedLanePoints[index]);
                        else
                            insideWay.LineString.Points.Add(orderedLanePoints[index]);
                    }
                }
                else
                {
                    var laneWay = road.GetRightWayOfLanes();
                    orderedLanePoints = new List<RnPoint>(laneWay.LineString.Points);
                    if (isPrev ^ road.MainLanes[road.MainLanes.Count - 1].IsReverse ^ laneWay.IsReversed)
                        orderedLanePoints.Reverse();
                    var index = orderedLanePoints.FindIndex(p => p.Vertex == (sideWalkEdgeInfo.IsInsidePrev ? sideWalkEdgeInfo.Edge.LineString.Points.First().Vertex : sideWalkEdgeInfo.Edge.LineString.Points.Last().Vertex));
                    for (; index < orderedLanePoints.Count; index++)
                    {
                        if (sideWalkEdgeInfo.IsInsidePrev)
                            insideWay.LineString.Points.Insert(0, orderedLanePoints[index]);
                        else
                            insideWay.LineString.Points.Add(orderedLanePoints[index]);
                    }
                }
                //ExtendPointsAlongSpline(insideWay.LineString.Points, spline, sideWalkEdgeInfo.IsInsidePrev);
                scannedLineStrings.Add(insideWay.LineString);
            }

            var outsideWay = sideWalkEdgeInfo.SideWalk.OutsideWay;
            if (!scannedLineStrings.Contains(outsideWay.LineString))
            {
                ExtendPointsAlongSpline(outsideWay.LineString.Points, spline, sideWalkEdgeInfo.IsOutsidePrev);
                scannedLineStrings.Add(outsideWay.LineString);
            }

            // エッジ再構築
            {
                var newEdge = new RnWay(new RnLineString(new List<RnPoint> {
                    sideWalkEdgeInfo.IsInsidePrev ? insideWay.LineString.Points.First() : insideWay.LineString.Points.Last(),
                    sideWalkEdgeInfo.IsOutsidePrev ? outsideWay.LineString.Points.First() : outsideWay.LineString.Points.Last()
                }));
                if (sideWalkEdgeInfo.IsStartEdge)
                    sideWalkEdgeInfo.SideWalk.SetStartEdgeWay(newEdge);
                else
                    sideWalkEdgeInfo.SideWalk.SetEndEdgeWay(newEdge);
            }
        }

        /// <summary>
        /// 交差点に道路を追加する
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="spline"></param>
        /// <returns></returns>
        private List<RnRoadBase> AddRoadAlongSpline(ExtensibleIntersectionEdge edge, Spline spline)
        {
            var edgeMaker = new RnIntersectionEdgeMaker(edge.intersection);
            var edgeInfo = edgeMaker.Execute(edge.neighbor, edge.index);

            // 長さ0の道路を生成
            var road = new RnRoad();
            RnWay leftmostWay = null, rightmostWay = null;
            RnSideWalk leftSideWalk = null, rightSideWalk = null;
            if (edgeInfo.LeftSideWalkEdge != null)
            {
                var startEdge = new RnWay(new RnLineString(edgeInfo.LeftSideWalkEdge.LineString.Points));
                startEdge.IsReversed = true;
                // 交差点の歩道エッジは時計回りのはず
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.Points.First() }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.Points.Last() }));
                var sideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdge, null);
                road.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);

                leftmostWay = insideWay;
                leftSideWalk = sideWalk;
            }
            if (edgeInfo.RightSideWalkEdge != null)
            {
                var startEdge = new RnWay(new RnLineString(edgeInfo.RightSideWalkEdge.LineString.Points));
                startEdge.IsReversed = true;
                // 交差点の歩道エッジは時計回りのはず
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.RightSideWalkEdge.Points.Last() }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.RightSideWalkEdge.LineString.Points.First() }));
                var sideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdge, null);
                road.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);

                rightmostWay = insideWay;
                rightSideWalk = sideWalk;
            }

            if (edgeInfo.Neighbor != null)
            {
                var leftWay = leftmostWay ?? new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.Neighbor.Border.Points.First() }));
                var rightWay = rightmostWay ?? new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.Neighbor.Border.Points.Last() }));
                var prevBorder = new RnWay(edgeInfo.Neighbor.Border.LineString);
                var lane = new RnLane(leftWay, rightWay, prevBorder, null);
                road.AddMainLane(lane);
                edgeInfo.Neighbor.Road = road;
                road.SetPrevNext(edge.intersection, null);
            }

            context.RoadNetwork.AddRoad(road);

            var roadGroup = road.CreateRoadGroup();

            var newEdgeInfo = new RoadEdgeInfo();
            newEdgeInfo.Edge = new ExtensibleRoadEdge(roadGroup, false, edge.center, edge.forward);

            if (leftSideWalk != null)
                newEdgeInfo.LeftSideWalkEdge = new SideWalkEdgeInfo
                {
                    IsInsidePrev = false,
                    IsOutsidePrev = false,
                    SideWalk = leftSideWalk,
                    Edge = leftSideWalk.EndEdgeWay,
                    IsStartEdge = false
                };

            if (rightSideWalk != null)
                newEdgeInfo.RightSideWalkEdge = new SideWalkEdgeInfo
                {
                    IsInsidePrev = false,
                    IsOutsidePrev = false,
                    SideWalk = rightSideWalk,
                    Edge = rightSideWalk.EndEdgeWay,
                    IsStartEdge = false
                };

            ExtendRoadAlongSpline(newEdgeInfo, spline);


            // 横断歩道を追加するために交差点を道路側に拡張
            var option = new RnModelEx.CalibrateIntersectionBorderOption();
            road.ParentModel.TrySliceRoadHorizontalNearByBorder(road, option, out var prevRoad, out var centerRoad, out var nextRoad);

            var result = prevRoad?.TryMerge2NeighborIntersection(RnLaneBorderType.Prev) ?? false;
            if (!result)
                Debug.LogWarning("Failed to merge 2 neighbor intersections");

            return new List<RnRoadBase>() { centerRoad.Prev, centerRoad };
        }

        /// <summary>
        /// Splineに沿ってLineStringの頂点を拡張する
        /// </summary>
        /// <param name="points"></param>
        /// <param name="spline"></param>
        /// <param name="isReversed">trueの場合末尾ではなく先頭から拡張する</param>
        private void ExtendPointsAlongSpline(List<RnPoint> points, Spline spline, bool isReversed)
        {
            var startPoint = isReversed ? points.First() : points.Last();
            // スプラインからLineStringを生成する際のオフセット値を推定
            var offset = EstimateOffset(startPoint.Vertex, spline, 0f);

            var newPoints = ConvertSplineToLineStringPoints(spline, offset, false);
            newPoints.RemoveAt(0); // 先頭の点は重複するため削除

            // 新しい点を追加
            foreach (var point in newPoints)
            {
                if (isReversed)
                    points.Insert(0, new RnPoint(point));
                else
                    points.Add(new RnPoint(point));
            }

            //if (isReversed)
            //    newEdgePoint = points.First();
            //else
            //    newEdgePoint = points.Last();
        }

        private static float EstimateOffset(Vector3 point, Spline spline, float t)
        {
            var nearestPoint = GetNearestPointToSplineNormal(point, spline, t);
            // スプラインの法線方向を正としてオフセットを算出
            var tangent = spline.EvaluateTangent(t);
            Vector3 origin = spline.EvaluatePosition(t);
            var distance = Vector3.Distance(origin, nearestPoint);
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            return Vector3.Dot(nearestPoint - origin, normal) > 0 ? distance : -distance;
        }

        /// <summary>
        /// pointから最も近いスプラインの法線直線上の点を取得
        /// </summary>
        /// <param name="point"></param>
        /// <param name="spline"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Vector3 GetNearestPointToSplineNormal(Vector3 point, Spline spline, float t)
        {
            var tangent = spline.EvaluateTangent(t);
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            Vector3 origin = spline.EvaluatePosition(t);
            point.y = origin.y;
            return origin + normal * Vector3.Dot(point - origin, normal);
        }
    }
}