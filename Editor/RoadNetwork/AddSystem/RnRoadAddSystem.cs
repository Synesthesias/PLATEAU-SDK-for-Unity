using NetTopologySuite.Operation.Valid;
using PLATEAU.Editor.RoadNetwork.AddSystem;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.AddSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    internal class RnRoadAddSystem : RnSplineSystemBase
    {
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// 道路追加後のアクション
        /// </summary>
        public Action<RnRoadGroup> OnRoadAdded { get; set; }

        private SplineCreateHandles splineCreateHandles;

        // 前フレームで作図モード中かどうかのフラグ (作図完了判定に使用)
        private bool wasCreatingSpline = false;

        private RnExtensiblePointHandles extensiblePointHandles;

        private RoadNetworkAddSystemContext context;

        private ExtensibleIntersectionEdge selectedIntersection;
        private ExtensibleRoadEdge selectedRoad;
        private bool isRoadSelected = false;

        public RnRoadAddSystem(RoadNetworkAddSystemContext context)
        {
            this.context = context;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore);
            extensiblePointHandles = new RnExtensiblePointHandles(context);
            extensiblePointHandles.OnRoadSelected = (edge) =>
            {
                selectedRoad = edge;
                isRoadSelected = true;

                // 作図開始
                splineCreateHandles.BeginCreateSpline(edge.center);
            };
            extensiblePointHandles.OnIntersectionSelected = (edge) =>
            {
                selectedIntersection = edge;
                isRoadSelected = false;

                // 作図開始
                splineCreateHandles.BeginCreateSpline(edge.center);
            };
        }

        public void Activate()
        {
            IsActive = true;
            splineCreateHandles = new SplineCreateHandles(splineEditorCore);
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
            if (!wasCreatingSpline)
                extensiblePointHandles.HandleSceneGUI(target, true, true);

            // 2. SplineCreateHandles でノット追加＆移動を処理
            splineCreateHandles.HandleSceneGUI();

            // 3. 作図完了を検知
            DetectSplineCreationCompletion();
        }

        /// <summary>
        /// スプライン作図が完了したタイミングで呼ばれる
        /// → クリック時に記憶しておいた `selectedRoad` を用いて、
        ///    その道路をスプラインに反映する
        /// </summary>
        private void OnSplineCreationFinished()
        {
            Spline newSpline = splineEditorCore.Spline;
            RnRoadGroup newRoad = null;
            if (isRoadSelected)
            {
                ExtendRoadAlongSpline(selectedRoad.road, newSpline, selectedRoad.isPrev);
                newRoad = selectedRoad.road;
            }
            else
            {
                AddRoadAlongSpline(selectedIntersection, newSpline);
            }
            //else
            //    AddRoadAlongSpline(edge, newSpline);
            splineEditorCore.Reset();
            OnRoadAdded?.Invoke(newRoad);
        }

        /// <summary>
        /// Splineに沿って道路を拡張する。
        /// </summary>
        /// <param name="targetRoad"></param>
        /// <param name="spline"></param>
        private void ExtendRoadAlongSpline(RnRoadGroup targetRoad, Spline spline, bool isPrev)
        {
            var road = targetRoad.Roads[0];

            var scannedLineStrings = new Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)>();

            foreach (var lane in road.AllLanesWithMedian)
            {
                if (road.IsLeftLane(lane))
                    Debug.Log("Process left");
                else if (road.IsRightLane(lane))
                    Debug.Log("Process right");
                else
                    Debug.Log("Process median");

                var oldEdgePoints = new List<RnPoint>();
                var newEdgePoints = new List<RnPoint>();
                int i = 0;
                foreach (var way in new[] { lane.LeftWay, lane.RightWay })
                {
                    if (i == 0)
                        Debug.Log("Process left way");
                    else
                        Debug.Log("Process right way");
                    RnPoint oldEdgePoint = null;
                    RnPoint newEdgePoint = null;
                    if (!scannedLineStrings.ContainsKey(way.LineString))
                    {
                        ExtendPointsAlongSpline(way.LineString.Points, spline, out oldEdgePoint, out newEdgePoint);
                        scannedLineStrings.Add(way.LineString, (oldEdgePoint, newEdgePoint));
                    }
                    else
                    {
                        Debug.Log("skip");
                        (oldEdgePoint, newEdgePoint) = scannedLineStrings[way.LineString];
                    }

                    if (oldEdgePoint != null)
                        oldEdgePoints.Add(oldEdgePoint);
                    if (newEdgePoint != null)
                        newEdgePoints.Add(newEdgePoint);
                }

                // ボーダー再構築
                foreach (var laneBorderType in new List<RnLaneBorderType> { RnLaneBorderType.Prev, RnLaneBorderType.Next })
                {
                    Debug.Log($"{laneBorderType}, {oldEdgePoints.Count}, {newEdgePoints.Count}, {lane.GetBorder(laneBorderType).Contains(oldEdgePoints.Last())}");

                    if (lane.GetBorder(laneBorderType) == null || !lane.GetBorder(laneBorderType).Contains(oldEdgePoints.Last()))
                        continue;
                    lane.SetBorder(laneBorderType, new RnWay(new RnLineString(newEdgePoints)));
                }
            }

            foreach (var sideWalk in road.SideWalks)
            {
                var oldEdgePoints = new List<RnPoint>();
                var newEdgePoints = new List<RnPoint>();
                int i = 0;
                foreach (var way in new[] { sideWalk.InsideWay, sideWalk.OutsideWay })
                {
                    if (i++ == 0)
                        Debug.Log("Process inside way");
                    else
                        Debug.Log("Process outside way");
                    RnPoint oldEdgePoint = null;
                    RnPoint newEdgePoint = null;
                    if (!scannedLineStrings.ContainsKey(way.LineString))
                    {
                        ExtendPointsAlongSpline(way.LineString.Points, spline, out oldEdgePoint, out newEdgePoint);
                        scannedLineStrings.Add(way.LineString, (oldEdgePoint, newEdgePoint));
                    }
                    else
                    {
                        Debug.Log("skip");
                        (oldEdgePoint, newEdgePoint) = scannedLineStrings[way.LineString];
                    }

                    if (oldEdgePoint != null)
                        oldEdgePoints.Add(oldEdgePoint);
                    if (newEdgePoint != null)
                        newEdgePoints.Add(newEdgePoint);
                }

                // エッジ再構築
                {
                    var edge = sideWalk.StartEdgeWay;
                    if (edge != null && edge.LineString.Points.Contains(oldEdgePoints.First()))
                        sideWalk.SetStartEdgeWay(new RnWay(new RnLineString(newEdgePoints)));
                }
                {
                    var edge = sideWalk.EndEdgeWay;
                    if (edge != null && edge.LineString.Points.Contains(oldEdgePoints.First()))
                        sideWalk.SetEndEdgeWay(new RnWay(new RnLineString(newEdgePoints)));
                }
            }

            //foreach (var way in GetAllWaysAlongRoad(road, isPrev))
            //{
            //    //ExtendPointsAlongSpline(way.Item2.LineString.Points, spline, way.Item1 ^ isPrev);
            //}
        }


        private RnRoadGroup AddRoadAlongSpline(ExtensibleIntersectionEdge edge, Spline spline)
        {
            var edgeMaker = new RnRoadEdgeMaker(edge.intersection);
            var edgeInfo = edgeMaker.Execute(edge.neighbor);

            // 長さ0の道路を生成
            var road = new RnRoad();
            if (edgeInfo.LeftSideWalkEdge != null)
            {
                var startEdge = new RnWay(new RnLineString(edgeInfo.LeftSideWalkEdge.LineString.Points));
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.LineString.Points.First() }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.LineString.Points.Last() }));
                var sideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdge, null);
                road.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }
            //if (edgeInfo.LeftSideWalkEdge != null)
            //{
            //    var startEdge = new RnWay(new RnLineString(edgeInfo.LeftSideWalkEdge.LineString.Points));
            //    var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.LineString.Points.First() }));
            //    var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.LeftSideWalkEdge.LineString.Points.Last() }));
            //    var sideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdge, null);
            //    road.AddSideWalk(sideWalk);
            //    context.RoadNetwork.AddSideWalk(sideWalk);
            //}
            if (edgeInfo.Neighbor != null)
            {
                var leftWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.Neighbor.Border.Points.First() }));
                var rightWay = new RnWay(new RnLineString(new List<RnPoint>() { edgeInfo.Neighbor.Border.Points.Last() }));
                var prevBorder = new RnWay(new RnLineString(edgeInfo.Neighbor.Border.Points));
                var nextBorder = new RnWay(new RnLineString(edgeInfo.Neighbor.Border.Points));
                var lane = new RnLane(leftWay, rightWay, prevBorder, nextBorder);
                road.AddMainLane(lane);
                edgeInfo.Neighbor.Road = road;
            }

            context.RoadNetwork.AddRoad(road);

            var roadGroup = road.CreateRoadGroup();
            ExtendRoadAlongSpline(roadGroup, spline, true);
            return roadGroup;
        }

        private static float GetDistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
        {
            return Vector3.Cross(point - origin, direction).magnitude;
        }

        /// <summary>
        /// Splineに沿ってLineStringを拡張する
        /// </summary>
        /// <param name="points"></param>
        /// <param name="spline"></param>
        /// <param name="isReversed">trueの場合末尾ではなく先頭から拡張する</param>
        private void ExtendPointsAlongSpline(List<RnPoint> points, Spline spline, out RnPoint oldEdgePoint, out RnPoint newEdgePoint)
        {
            bool shouldInsert = false;
            oldEdgePoint = null;
            // pointsの終点側でエッジ上に存在する点群を取得
            var pointsOnEdge = new List<RnPoint>();
            foreach (var point in new Stack<RnPoint>(points))
            {
                Debug.Log($"{GetDistanceToSplineNormal(point.Vertex, spline, 0f)}, {point.DebugMyId}");
                if (GetDistanceToSplineNormal(point.Vertex, spline, 0f) < 1f)
                {
                    pointsOnEdge.Insert(0, point);
                }
                else
                    break;
            }

            // エッジ上の点がない場合は始点側をチェック
            if (pointsOnEdge.Count != 0)
            {
                shouldInsert = false;
                oldEdgePoint = points.Last();
            }
            else
            {
                foreach (var point in points)
                {
                    Debug.Log($"{GetDistanceToSplineNormal(point.Vertex, spline, 0f)}, {point.DebugMyId}");
                    if (GetDistanceToSplineNormal(point.Vertex, spline, 0f) < 1f)
                    {
                        pointsOnEdge.Insert(0, point);
                    }
                    else
                        break;
                }
                shouldInsert = true;
                oldEdgePoint = points.First();
            }

            if (pointsOnEdge.Count == 0)
            {
                oldEdgePoint = null;
                newEdgePoint = null;
                return;
            }

            // エッジ上の最初の点以外pointsから削除（そこを根本として道路を生やすため）
            foreach (var point in pointsOnEdge)
            {
                points.Remove(point);
            }
            if (shouldInsert)
                points.Insert(0, pointsOnEdge.First());
            else
                points.Add(pointsOnEdge.First());

            // スプラインからLineStringを生成する際のオフセット値を推定
            var offset = EstimateOffset(pointsOnEdge.First().Vertex, spline, 0f);

            var newPoints = ConvertSplineToLineStringPoints(spline, offset, false);
            newPoints.RemoveAt(0); // 先頭の点は重複するため削除

            // 新しい点を追加
            foreach (var point in newPoints)
            {
                if (shouldInsert)
                    points.Insert(0, new RnPoint(point));
                else
                    points.Add(new RnPoint(point));
            }

            if (shouldInsert)
                newEdgePoint = points.First();
            else
                newEdgePoint = points.Last();
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

        private static float GetDistanceToSplineNormal(Vector3 point, Spline spline, float t)
        {
            var nearestPoint = GetNearestPointToSplineNormal(point, spline, t);
            point.y = nearestPoint.y;
            return Vector3.Distance(point, nearestPoint);
        }

        private void DetectSplineCreationCompletion()
        {
            bool isCreatingNow = splineCreateHandles.IsCreatingSpline;
            if (!isCreatingNow && wasCreatingSpline)
            {
                // 直前まで作図モードだった → 今フレームで終了した
                OnSplineCreationFinished();
            }
            wasCreatingSpline = isCreatingNow;
        }
    }
}