using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.AddSystem;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.AddSystem
{
    internal class IntersectionAddSystem
    {
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// 交差点追加後のアクション
        /// </summary>
        public Action<RnIntersection> OnIntersectionAdded { get; set; }

        private RnSkeletonHandles extensiblePointHandles;
        private RoadNetworkAddSystemContext context;

        public IntersectionAddSystem(RoadNetworkAddSystemContext context)
        {
            this.context = context;
            extensiblePointHandles = new RnSkeletonHandles(context);
            extensiblePointHandles.OnRoadSelected += HandlePointPicked;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void HandleSceneGUI(SceneView sceneView)
        {
            if (!IsActive)
                return;

            extensiblePointHandles.HandleSceneGUI(sceneView, true, false);
        }

        private void HandlePointPicked(ExtensibleRoadEdge edge)
        {
            if (edge.road == null)
                return;

            var edgeMaker = new RnRoadEdgeMaker(edge.road.Roads[0]);
            edgeMaker.Execute(edge);

            var intersection = new RnIntersection();

            var forward = edge.forward.normalized;
            var right = Quaternion.AngleAxis(90f, Vector3.up) * edge.forward.normalized;

            var points = new List<RnPoint>();
            var firstPoint = edge.road.GetLeftLanes().Count() == 0
                ? edge.road.GetRightLanes().First().RightWay.LineString.Points.First()
                : edge.road.GetLeftLanes().First().LeftWay.LineString.Points.First();
            var lastPoint = edge.road.GetRightLanes().Count() == 0
                ? edge.road.GetLeftLanes().Last().RightWay.LineString.Points.First()
                : edge.road.GetRightLanes().Last().LeftWay.LineString.Points.Last();

            RnWay leftSideWalkEdge = null;
            bool isLeftStartEdge = false;
            foreach (var sideWalk in edge.road.Roads[0].SideWalks)
            {
                GetOrCreateSideWalkEdge(sideWalk, edge.center, right, out _);

                if (sideWalk.StartEdgeWay != null && sideWalk.StartEdgeWay.LineString.Points.Contains(firstPoint))
                {
                    leftSideWalkEdge = sideWalk.StartEdgeWay;
                    isLeftStartEdge = true;
                    break;
                }
                if (sideWalk.EndEdgeWay != null && sideWalk.EndEdgeWay.LineString.Points.Contains(firstPoint))
                {
                    leftSideWalkEdge = sideWalk.EndEdgeWay;
                    break;
                }
            }

            RnWay rightSideWalkEdge = null;
            bool isRightStartEdge = false;
            foreach (var sideWalk in edge.road.Roads[0].SideWalks)
            {
                GetOrCreateSideWalkEdge(sideWalk, edge.center, right, out _);

                if (sideWalk.StartEdgeWay != null && sideWalk.StartEdgeWay.LineString.Points.Contains(lastPoint))
                {
                    rightSideWalkEdge = sideWalk.StartEdgeWay;
                    isRightStartEdge = true;
                    break;
                }
                if (sideWalk.EndEdgeWay != null && sideWalk.EndEdgeWay.LineString.Points.Contains(lastPoint))
                {
                    rightSideWalkEdge = sideWalk.EndEdgeWay;
                    break;
                }
            }

            //var firstPoint = edge.road.Roads[0].SideWalks.First().LeftWay.LineString.Points.First();
            //var lastPoint = edge.road.GetRightLanes().Last().LeftWay.LineString.Points.Last();

            // 道路端点が中心線に垂直になるように整形
            //var road = edge.road.Roads[0];
            //foreach (var lane in road.AllLanesWithMedian)
            //{
            //    foreach (var way in new[] { lane.LeftWay, lane.RightWay })
            //    {
            //        var wayPoints = way.LineString.Points;
            //        var edgePoint = lane.IsReverse ^ way.IsReversed ? wayPoints.Last() : wayPoints.First();
            //        edgePoint.Vertex = GetNearestPointToLine(edgePoint.Vertex, edge.center, edge.forward);
            //    }
            //}

            //foreach (var sideWalk in road.SideWalks)
            //{
            //    foreach (var way in new[] { sideWalk.InsideWay, sideWalk.OutsideWay })
            //    {

            //    }
            //}

            var borderLength = Vector3.Distance(firstPoint.Vertex, lastPoint.Vertex);
            var leftSideWalkEdgeLength = leftSideWalkEdge == null ? 0f : Vector3.Distance(leftSideWalkEdge.LineString.Points.First().Vertex, leftSideWalkEdge.LineString.Points.Last().Vertex);
            var rightSideWalkEdgeLength = rightSideWalkEdge == null ? 0f : Vector3.Distance(rightSideWalkEdge.LineString.Points.First().Vertex, rightSideWalkEdge.LineString.Points.Last().Vertex);

            //var nonBorderLength = 3f + Mathf.Sqrt(2f) * leftSideWalkEdgeLength;
            //// edge.forwardの向きに８角形の頂点を追加
            //points.Add(firstPoint);
            //points.Add(new RnPoint(firstPoint.Vertex + (forward + right).normalized * nonBorderLength));
            //points.Add(new RnPoint(points.Last().Vertex + forward * borderLength));
            //points.Add(new RnPoint(points.Last().Vertex + (forward - right).normalized * nonBorderLength));
            //points.Add(new RnPoint(points.Last().Vertex + -right * borderLength));
            //points.Add(new RnPoint(points.Last().Vertex + (-forward - right).normalized * nonBorderLength));
            //points.Add(new RnPoint(points.Last().Vertex + -forward * borderLength));
            //points.Add(lastPoint);
            //var way = new RnWay(new RnLineString(points));
            //intersection.AddEdge(edge.road.Roads[0], way);

            var exteriorPoints = new List<RnPoint>();
            exteriorPoints.Add(firstPoint);
            exteriorPoints.Add(new RnPoint(firstPoint.Vertex + right * leftSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (forward + right) * 3f));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + forward * leftSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + forward * borderLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + forward * rightSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (forward - right) * 3f));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex - right * rightSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex - right * borderLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex - right * leftSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (-forward - right) * 3f));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (-forward) * leftSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (-forward) * borderLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (-forward) * rightSideWalkEdgeLength));
            exteriorPoints.Add(new RnPoint(exteriorPoints.Last().Vertex + (-forward + right) * 3f));
            exteriorPoints.Add(lastPoint);

            for (int i = 0; i < 15; i += 4)
            {
                var startWay = new RnWay(new RnLineString(new[] { exteriorPoints[i], exteriorPoints[i + 1] }));
                var endWay = new RnWay(new RnLineString(new[] { exteriorPoints[i + 2], exteriorPoints[i + 3] }));
                var insideWay = new RnWay(new RnLineString(new[] { exteriorPoints[i + 3], exteriorPoints[i] }));
                var outsideWay = new RnWay(new RnLineString(new[] { exteriorPoints[i + 1], exteriorPoints[i + 2] }));
                var sideWalk = RnSideWalk.Create(intersection, outsideWay, insideWay, startWay, endWay);
                intersection.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }

            for (int i = 0; i < 15; i += 4)
            {
                var borderWay = new RnWay(new RnLineString(new[] { exteriorPoints[i + 3], exteriorPoints[(i + 4) % 16] }));
                var nonBorderWay = new RnWay(new RnLineString(new[] { exteriorPoints[i], exteriorPoints[i + 3] }));
                if (i == 12)
                    intersection.AddEdge(edge.road.Roads[0], borderWay);
                else
                    intersection.AddEdge(null, borderWay);
                intersection.AddEdge(null, nonBorderWay);
            }

            intersection.Align();
            context.RoadNetwork.AddIntersection(intersection);

            var road = edge.road.Roads[0];
            if (edge.isPrev)
                road.SetPrevNext(intersection, road.Next);
            else
                road.SetPrevNext(road.Prev, intersection);

            OnIntersectionAdded?.Invoke(intersection);
        }

        private static float GetDistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
        {
            return Vector3.Cross(point - origin, direction).magnitude;
        }


        private RnWay GetOrCreateSideWalkEdge(RnSideWalk sideWalk, Vector3 edgeLineOrigin, Vector3 edgeLineDirection, out bool isStartEdge)
        {
            isStartEdge = false;
            var points = sideWalk.OutsideWay.LineString.Points;
            // pointsの終点側でエッジ上に存在する点群を取得
            var pointsOnEdge = new List<RnPoint>();
            foreach (var point in new Stack<RnPoint>(points))
            {
                Debug.Log($"AA: {GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection)}");
                if (GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection) < 1f)
                {
                    pointsOnEdge.Add(point);
                }
                else
                    break;
            }

            // エッジ上の点がない場合は始点側をチェック
            if (pointsOnEdge.Count == 0)
            {
                foreach (var point in points)
                {
                    Debug.Log($"AA: {GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection)}");
                    if (GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection) < 1f)
                    {
                        pointsOnEdge.Add(point);
                    }
                    else
                        break;
                }
            }

            if (pointsOnEdge.Count <= 1)
            {
                return null;
            }

            // エッジ上の最後の点以外pointsから削除
            foreach (var point in pointsOnEdge.GetRange(0, pointsOnEdge.Count - 1))
            {
                points.Remove(point);
            }

            if (sideWalk.StartEdgeWay == null || sideWalk.StartEdgeWay.LineString.Points.Contains(pointsOnEdge.First()))
            {
                isStartEdge = true;
                if (sideWalk.StartEdgeWay == null)
                {
                    sideWalk.SetStartEdgeWay(new RnWay(new RnLineString(pointsOnEdge)));
                }
                else
                {
                    sideWalk.StartEdgeWay.LineString.Points.AddRange(pointsOnEdge.Skip(1));
                }
                return sideWalk.StartEdgeWay;
            }
            else
            {
                if (sideWalk.EndEdgeWay == null || sideWalk.EndEdgeWay.LineString.Points.Contains(pointsOnEdge.First()))
                {
                    if (sideWalk.EndEdgeWay == null)
                    {
                        sideWalk.SetEndEdgeWay(new RnWay(new RnLineString(pointsOnEdge)));
                    }
                    else
                    {
                        sideWalk.EndEdgeWay.LineString.Points.AddRange(pointsOnEdge.Skip(1));
                    }
                    return sideWalk.EndEdgeWay;
                }
            }
            return null;
        }

        //private static List<RnPoint> GetEdgePoints(RnRoad road, RnLaneBorderType borderType)
        //{
        //    var points = new HashSet<RnPoint>();
        //    foreach (var edge in road.GetBorderWays(borderType))
        //    {
        //        points.Add
        //    }
        //}

        /// <summary>
        /// 任意の中心、および任意方向・大きさのベクトル(axisX, axisY)を指定し、
        /// 4分の1楕円（0°～90°）の上にある 6点 (0°,18°,36°,54°,72°,90°) を取得する。
        /// 
        ///   P(θ) = center + axisX * cos(θ) + axisY * sin(θ)
        /// </summary>
        /// <param name="center">楕円の中心座標</param>
        /// <param name="axisX">中心から x方向交点までのベクトル（長さ=a）</param>
        /// <param name="axisY">中心から y方向交点までのベクトル（長さ=b）</param>
        /// <returns>計6点のワールド座標</returns>
        public static Vector3[] GetQuarterEllipsePoints(
            Vector3 center,
            Vector3 axisX,
            Vector3 axisY
        )
        {
            // サンプリングしたい角度
            float[] anglesDeg = { 0f, 18f, 36f, 54f, 72f, 90f };

            Vector3[] result = new Vector3[anglesDeg.Length];

            for (int i = 0; i < anglesDeg.Length; i++)
            {
                float deg = anglesDeg[i];
                float rad = deg * Mathf.Deg2Rad;

                // P(θ) = center + axisX*cos(θ) + axisY*sin(θ)
                float cosVal = Mathf.Cos(rad);
                float sinVal = Mathf.Sin(rad);

                Vector3 pos = center + axisX * cosVal + axisY * sinVal;
                result[i] = pos;
            }

            return result;
        }
    }
}
