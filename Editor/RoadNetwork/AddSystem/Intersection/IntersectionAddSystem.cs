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
    enum IntersectionType
    {
        T = 0,
        Cross = 1,
    }

    enum TIntersectionType
    {
        Left = 0,
        Right = 1,
        Front = 2,
        None = 3,
    }

    internal class IntersectionAddSystem
    {
        public bool IsActive { get; private set; } = false;
        public IntersectionType IntersectionType { get; private set; } = IntersectionType.Cross;
        public bool IsCreating { get; private set; } = false;

        private RoadEdgeInfo edgeInfo;
        private TIntersectionType tIntersectionType = TIntersectionType.Front;

        /// <summary>
        /// 交差点追加後のアクション
        /// </summary>
        public Action<RnIntersection, RnRoad> OnIntersectionAdded { get; set; }

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
            IsCreating = false;
        }

        public void HandleSceneGUI(SceneView sceneView)
        {
            if (!IsActive)
                return;


            if (!IsCreating)
            {
                extensiblePointHandles.HandleSceneGUI(sceneView, true, false);
                return;
            }

            // Rキーで交差点の種類を切り替え
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
            {
                switch (tIntersectionType)
                {
                    case TIntersectionType.Left:
                        tIntersectionType = TIntersectionType.Right;
                        break;
                    case TIntersectionType.Right:
                        tIntersectionType = TIntersectionType.Front;
                        break;
                    case TIntersectionType.Front:
                        tIntersectionType = TIntersectionType.Left;
                        break;
                }
            }
            // エンターキーで交差点を追加
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                AddIntersection();
                IsCreating = false;
            }
            // Escキーで交差点追加をキャンセル
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                IsCreating = false;
            }

            DrawPreviewExterior();
        }

        private void DrawPreviewExterior()
        {
            var roadGroup = edgeInfo.Edge.road;
            var road = roadGroup.Roads[0];

            var isRoadPrev = edgeInfo.Edge.isPrev;
            var center = edgeInfo.Edge.center;
            var forward = edgeInfo.Edge.forward.normalized;
            var right = Quaternion.AngleAxis(90f, Vector3.up) * forward.normalized;

            var points = new List<RnPoint>();
            RnPoint firstPoint;
            {
                var way = road.GetLeftWayOfLanes();
                firstPoint = isRoadPrev ^ road.MainLanes[0].IsReverse ^ way.IsReversed
                    ? way.LineString.Points.First() : way.LineString.Points.Last();
            }

            RnPoint lastPoint;
            {
                var way = road.GetRightWayOfLanes();
                lastPoint = isRoadPrev ^ road.MainLanes.Last().IsReverse ^ way.IsReversed
                    ? way.LineString.Points.First() : way.LineString.Points.Last();
            }

            if (!isRoadPrev)
                (lastPoint, firstPoint) = (firstPoint, lastPoint);

            var leftSideWalkEdge = edgeInfo.LeftSideWalkEdge.Edge;
            var rightSideWalkEdge = edgeInfo.RightSideWalkEdge.Edge;

            if (!isRoadPrev)
                (leftSideWalkEdge, rightSideWalkEdge) = (rightSideWalkEdge, leftSideWalkEdge);

            var borderLength = Vector3.Distance(firstPoint.Vertex, lastPoint.Vertex);
            var leftSideWalkEdgeLength = leftSideWalkEdge == null ? 0f : leftSideWalkEdge.CalcLength();
            var rightSideWalkEdgeLength = rightSideWalkEdge == null ? 0f : rightSideWalkEdge.CalcLength();
            var longerSideWalkEdgeLength = Math.Max(leftSideWalkEdgeLength, rightSideWalkEdgeLength);

            var leftSideWalkOuterPoint = edgeInfo.LeftSideWalkEdge.SideWalk == null
                ? new RnPoint(firstPoint.Vertex + right * leftSideWalkEdgeLength)
                : (edgeInfo.LeftSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());
            var rightSideWalkOuterPoint = edgeInfo.RightSideWalkEdge.SideWalk == null
                ? new RnPoint(lastPoint.Vertex - right * rightSideWalkEdgeLength)
                : (edgeInfo.RightSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());

            if (isRoadPrev)
            {
                (leftSideWalkOuterPoint, rightSideWalkOuterPoint) = (rightSideWalkOuterPoint, leftSideWalkOuterPoint);
                (leftSideWalkEdgeLength, rightSideWalkEdgeLength) = (rightSideWalkEdgeLength, leftSideWalkEdgeLength);
            }

            // 外輪郭を描画
            var intersectionType = IntersectionType == IntersectionType.Cross ? TIntersectionType.None : tIntersectionType;
            var exteriorPoints = CalcExteriorPoints(forward, right, firstPoint, lastPoint, borderLength, leftSideWalkEdgeLength, rightSideWalkEdgeLength, longerSideWalkEdgeLength, leftSideWalkOuterPoint, rightSideWalkOuterPoint, intersectionType);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(exteriorPoints.Select(p => p.Vertex).ToArray());
        }

        private void HandlePointPicked(ExtensibleRoadEdge edge)
        {
            if (edge.road == null)
                return;

            var edgeMaker = new RnRoadEdgeMaker(edge.road.Roads[0]);
            edgeInfo = edgeMaker.Execute(edge);
            IsCreating = true;
        }

        private void AddIntersection()
        {
            var roadGroup = edgeInfo.Edge.road;
            var road = roadGroup.Roads[0];

            var isRoadPrev = edgeInfo.Edge.isPrev;
            var center = edgeInfo.Edge.center;
            var forward = edgeInfo.Edge.forward.normalized;
            var right = Quaternion.AngleAxis(90f, Vector3.up) * forward.normalized;

            var points = new List<RnPoint>();
            RnPoint firstPoint;
            {
                var way = road.GetLeftWayOfLanes();
                firstPoint = isRoadPrev ^ road.MainLanes[0].IsReverse ^ way.IsReversed
                    ? way.LineString.Points.First() : way.LineString.Points.Last();
            }

            RnPoint lastPoint;
            {
                var way = road.GetRightWayOfLanes();
                lastPoint = isRoadPrev ^ road.MainLanes.Last().IsReverse ^ way.IsReversed
                    ? way.LineString.Points.First() : way.LineString.Points.Last();
            }

            if (!isRoadPrev)
                (lastPoint, firstPoint) = (firstPoint, lastPoint);

            var leftSideWalkEdge = edgeInfo.LeftSideWalkEdge.Edge;
            var rightSideWalkEdge = edgeInfo.RightSideWalkEdge.Edge;

            //if (!isRoadPrev)
            //    (leftSideWalkEdge, rightSideWalkEdge) = (rightSideWalkEdge, leftSideWalkEdge);

            var borderLength = Vector3.Distance(firstPoint.Vertex, lastPoint.Vertex);
            var leftSideWalkEdgeLength = leftSideWalkEdge == null ? 0f : leftSideWalkEdge.CalcLength();
            var rightSideWalkEdgeLength = rightSideWalkEdge == null ? 0f : rightSideWalkEdge.CalcLength();
            var longerSideWalkEdgeLength = Math.Max(leftSideWalkEdgeLength, rightSideWalkEdgeLength);

            var leftSideWalkOuterPoint = edgeInfo.LeftSideWalkEdge.SideWalk == null
                ? new RnPoint(firstPoint.Vertex + right * leftSideWalkEdgeLength)
                : (edgeInfo.LeftSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.LeftSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());
            var rightSideWalkOuterPoint = edgeInfo.RightSideWalkEdge.SideWalk == null
                ? new RnPoint(lastPoint.Vertex - right * rightSideWalkEdgeLength)
                : (edgeInfo.RightSideWalkEdge.IsOutsidePrev
                    ? edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.First()
                    : edgeInfo.RightSideWalkEdge.SideWalk.OutsideWay.LineString.Points.Last());

            if (isRoadPrev)
            {
                (leftSideWalkOuterPoint, rightSideWalkOuterPoint) = (rightSideWalkOuterPoint, leftSideWalkOuterPoint);
                (leftSideWalkEdgeLength, rightSideWalkEdgeLength) = (rightSideWalkEdgeLength, leftSideWalkEdgeLength);
            }

            RnIntersection intersection;
            if (IntersectionType == IntersectionType.Cross)
                intersection = CreateCrossIntersection(road, forward, right, firstPoint, lastPoint, borderLength, leftSideWalkEdgeLength, rightSideWalkEdgeLength, longerSideWalkEdgeLength, leftSideWalkOuterPoint, rightSideWalkOuterPoint);
            else
                intersection = CreateTIntersection(road, forward, right, firstPoint, lastPoint, borderLength, leftSideWalkEdgeLength, rightSideWalkEdgeLength, longerSideWalkEdgeLength, leftSideWalkOuterPoint, rightSideWalkOuterPoint, tIntersectionType);

            // 隣接道路がある輪郭を追加
            foreach (var roadBorder in road.MainLanes.Select(l => isRoadPrev ^ l.IsReverse ? l.PrevBorder : l.NextBorder))
            {
                intersection.AddEdge(road, roadBorder);
            }

            intersection.Align();
            context.RoadNetwork.AddIntersection(intersection);

            // 隣接情報更新
            if (edgeInfo.Edge.isPrev)
                road.SetPrevNext(intersection, road.Next);
            else
                road.SetPrevNext(road.Prev, intersection);

            // 横断歩道を追加するために道路側に拡張
            // 逆側を拡張しないために一時的に隣接関係を削除
            var oppositeIntersection = isRoadPrev ? road.Next : road.Prev;
            if (edgeInfo.Edge.isPrev)
                road.SetPrevNext(intersection, null);
            else
                road.SetPrevNext(null, intersection);
            var option = new RnModelEx.CalibrateIntersectionBorderOption();
            var sliceResult = road.ParentModel.TrySliceRoadHorizontalNearByBorder(road, option, out var prevRoad, out var centerRoad, out var nextRoad);
            if (sliceResult && (prevRoad != null || nextRoad != null))
            {
                bool result;
                if (edgeInfo.Edge.isPrev)
                    result = prevRoad.TryMerge2NeighborIntersection(RnLaneBorderType.Prev);
                else
                    result = nextRoad.TryMerge2NeighborIntersection(RnLaneBorderType.Next);
            }

            // 隣接関係を復元
            if (edgeInfo.Edge.isPrev)
                road.SetPrevNext(intersection, oppositeIntersection);
            else
                road.SetPrevNext(oppositeIntersection, intersection);

            intersection.TargetTrans.Clear();

            if (sliceResult)
                OnIntersectionAdded?.Invoke(intersection, road);
            else
                OnIntersectionAdded?.Invoke(intersection, null);
        }

        private RnIntersection CreateCrossIntersection(RnRoad road, Vector3 forward, Vector3 right, RnPoint firstPoint, RnPoint lastPoint, float borderLength, float leftSideWalkEdgeLength, float rightSideWalkEdgeLength, float longerSideWalkEdgeLength, RnPoint leftSideWalkOuterPoint, RnPoint rightSideWalkOuterPoint)
        {
            var intersection = new RnIntersection();

            var exteriorPoints = CalcExteriorPoints(forward, right, firstPoint, lastPoint, borderLength, leftSideWalkEdgeLength, rightSideWalkEdgeLength, longerSideWalkEdgeLength, leftSideWalkOuterPoint, rightSideWalkOuterPoint, TIntersectionType.None);

            foreach (var point in exteriorPoints)
            {
                new GameObject("Point").transform.position = point.Vertex;
            }

            // 歩道作成
            for (int i = 0; i < 15; i += 4)
            {
                var startWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i], exteriorPoints[i + 1] }));
                var endWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 2], exteriorPoints[i + 3] }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 3], exteriorPoints[i] }));
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 1], exteriorPoints[i + 2] }));
                var sideWalk = RnSideWalk.Create(intersection, outsideWay, insideWay, startWay, endWay);
                intersection.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }

            // 輪郭作成
            for (int i = 0; i < 15; i += 4)
            {
                var borderWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 3], exteriorPoints[(i + 4) % 16] }));
                var nonBorderWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i], exteriorPoints[i + 3] }));
                // 隣接道路がある輪郭は別途追加
                if (i != 12)
                    intersection.AddEdge(null, borderWay);
                intersection.AddEdge(null, nonBorderWay);
            }

            return intersection;
        }

        private static List<RnPoint> CalcExteriorPoints(Vector3 forward, Vector3 right, RnPoint firstPoint, RnPoint lastPoint, float borderLength, float leftSideWalkEdgeLength, float rightSideWalkEdgeLength, float longerSideWalkEdgeLength, RnPoint leftSideWalkOuterPoint, RnPoint rightSideWalkOuterPoint, TIntersectionType intersectionType)
        {
            var exteriorPoints = new List<RnPoint>();

            exteriorPoints.Add(firstPoint);
            exteriorPoints.Add(rightSideWalkOuterPoint);
            var position = rightSideWalkOuterPoint.Vertex;
            position += (forward + right) * 3f;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(position));
            position += forward * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(position));
            position += forward * borderLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(position));
            position += forward * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Right)
                exteriorPoints.Add(new RnPoint(position));

            position += (forward - right) * 3f;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(position));
            position -= right * leftSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(position));
            position -= right * borderLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(position));
            position -= right * rightSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Front)
                exteriorPoints.Add(new RnPoint(position));

            position += (-forward - right) * 3f;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(position));
            position += (-forward) * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(position));
            position += (-forward) * borderLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(position));
            position += (-forward) * longerSideWalkEdgeLength;
            if (intersectionType != TIntersectionType.Left)
                exteriorPoints.Add(new RnPoint(position));

            exteriorPoints.Add(leftSideWalkOuterPoint);
            exteriorPoints.Add(lastPoint);
            return exteriorPoints;
        }

        private RnIntersection CreateTIntersection(RnRoad road, Vector3 forward, Vector3 right, RnPoint firstPoint, RnPoint lastPoint, float borderLength, float leftSideWalkEdgeLength, float rightSideWalkEdgeLength, float longerSideWalkEdgeLength, RnPoint leftSideWalkOuterPoint, RnPoint rightSideWalkOuterPoint, TIntersectionType intersectionType)
        {
            var intersection = new RnIntersection();

            var exteriorPoints = CalcExteriorPoints(forward, right, firstPoint, lastPoint, borderLength, leftSideWalkEdgeLength, rightSideWalkEdgeLength, longerSideWalkEdgeLength, leftSideWalkOuterPoint, rightSideWalkOuterPoint, intersectionType);

            // 歩道作成
            for (int i = 0; i < 11; i += 4)
            {
                var startWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i], exteriorPoints[i + 1] }));
                var endWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 2], exteriorPoints[i + 3] }));
                var insideWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 3], exteriorPoints[i] }));
                var outsideWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 1], exteriorPoints[i + 2] }));
                var sideWalk = RnSideWalk.Create(intersection, outsideWay, insideWay, startWay, endWay);
                intersection.AddSideWalk(sideWalk);
                context.RoadNetwork.AddSideWalk(sideWalk);
            }

            // 輪郭作成
            for (int i = 0; i < 11; i += 4)
            {
                var borderWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i + 3], exteriorPoints[(i + 4) % 12] }));
                var nonBorderWay = new RnWay(new RnLineString(new List<RnPoint>() { exteriorPoints[i], exteriorPoints[i + 3] }));
                // 隣接道路がある輪郭は別途追加
                if (i != 8)
                    intersection.AddEdge(null, borderWay);
                intersection.AddEdge(null, nonBorderWay);
            }

            return intersection;
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

        internal void SetIntersectionType(IntersectionType t)
        {
            IntersectionType = t;
        }
    }
}
