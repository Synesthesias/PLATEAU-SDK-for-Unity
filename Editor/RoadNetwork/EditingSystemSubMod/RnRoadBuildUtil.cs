using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using PLATEAU.RoadNetwork.Structure;
using Unity.Mathematics;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプラインから道路への反映処理を共通化したユーティリティ
    /// </summary>
    internal static class RnRoadBuildUtil
    {
        /// <summary>
        /// RnSplineEditorの ApplySpline と同様のロジックを抜き出し。
        /// スプラインを元に、指定した roadGroup.Roads[0] を更新する例。
        /// </summary>
        public static void BuildRoadFromSpline(RnRoadGroup roadGroup, Spline spline)
        {
            // FIXME: UIから変更可能にする（RnSplineEditorと同じ仮値）
            const float medianWidth = 2f;
            const float laneWidth = 3f;

            var road = roadGroup.Roads[0];
            var totalWidth = road.AllLanes.Count() * laneWidth;
            if (road.MedianLane != null)
            {
                totalWidth += medianWidth;
            }

            // 歩道の結合など
            MergeSeparatedSideWalks(road, out var leftSideWalk, out var rightSideWalk);

            // 道路中心からのオフセット量
            var offset = totalWidth / 2;

            // 左車線群
            foreach (var leftLane in road.GetLeftLanes())
            {
                var points = leftLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.LeftWay.IsReversed);
                offset -= laneWidth;

                points = leftLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.RightWay.IsReversed);
            }

            // 中央帯
            if (road.MedianLane != null)
            {
                offset -= medianWidth;
            }

            // 右車線群
            foreach (var rightLane in road.GetRightLanes())
            {
                var points = rightLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.RightWay.IsReversed);

                offset -= laneWidth;

                points = rightLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, rightLane.LeftWay.IsReversed);
            }
        }

        /// <summary>
        /// ApplySpline内部の ConvertSplineToLineStringPoints 
        /// </summary>
        private static void ConvertSplineToLineStringPoints(Spline spline, ref List<RnPoint> destPoints, float offset, bool isForward)
        {
            var firstPoint = destPoints.First();
            var lastPoint = destPoints.Last();
            destPoints.Clear();

            // 始点
            float t = isForward ? 0f : 1f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            firstPoint.Vertex = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            while (isForward ? t < 1f : t > 0f)
            {
                // 1m毎にスプライン上の点を取ってきて、30m以上離れているか5度以上角度が異なる場合に頂点追加
                spline.GetPointAtLinearDistance(t, isForward ? 1f : -1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > 30 || Vector3.Angle(prevTangent, newTangent) > 5)
                {
                    destPoints.Add(new RnPoint(GetOffsetPointToNormalDirection(newPoint, newTangent, offset)));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }

                t = newT;

                if (isForward ? t >= 1f : t <= 0f)
                {
                    // 終点
                    lastPoint.Vertex = GetOffsetPointToNormalDirection(
                        spline.EvaluatePosition(isForward ? 1f : 0f),
                        spline.EvaluateTangent(isForward ? 1f : 0f),
                        offset);
                    destPoints.Add(lastPoint);
                }
            }
        }

        private static Vector3 GetOffsetPointToNormalDirection(Vector3 point, Vector3 tangent, float offset)
        {
            var normal = Vector3.Cross(tangent, Vector3.up).normalized;
            return point + normal * offset;
        }

        /// <summary>
        /// ApplySpline内部の MergeSeparatedSideWalks
        /// </summary>
        private static void MergeSeparatedSideWalks(RnRoad road, out RnSideWalk leftSideWalk, out RnSideWalk rightSideWalk)
        {
            leftSideWalk = null;
            rightSideWalk = null;

            var leftWay = road.GetLeftWayOfLanes();
            var rightWay = road.GetRightWayOfLanes();

            var leftSideWalkAndSharedPoints = new List<(RnSideWalk, HashSet<RnPoint>)>();
            var rightSideWalkAndSharedPoints = new List<(RnSideWalk, HashSet<RnPoint>)>();

            foreach (var sidewalk in road.SideWalks)
            {
                var leftSharedPoints = GetSharedPointSet(leftWay, sidewalk.InsideWay);
                if (leftSharedPoints.Count >= 1)
                {
                    leftSideWalkAndSharedPoints.Add((sidewalk, leftSharedPoints));
                    leftSideWalk = sidewalk;
                }

                var rightSharedPoints = GetSharedPointSet(rightWay, sidewalk.InsideWay);
                if (rightSharedPoints.Count >= 1)
                {
                    rightSideWalkAndSharedPoints.Add((sidewalk, rightSharedPoints));
                    rightSideWalk = sidewalk;
                }
            }

            if (leftSideWalkAndSharedPoints.Count > 1)
            {
                MergeSeparatedWalkWays(road, leftSideWalkAndSharedPoints, leftWay, out leftSideWalk);
            }

            if (rightSideWalkAndSharedPoints.Count > 1)
            {
                MergeSeparatedWalkWays(road, rightSideWalkAndSharedPoints, rightWay, out rightSideWalk);
            }
        }

        private static void MergeSeparatedWalkWays(RnRoad road,
            List<(RnSideWalk, HashSet<RnPoint>)> sideWalkAndSharedPointSets,
            RnWay sideWay, out RnSideWalk newSideWalk)
        {
            // ... RnSplineEditorと同様の実装
            newSideWalk = null;
            // （中略：元のMergeSeparatedWalkWaysロジックを移植）
            // 必要ならそのままコピペでもOK
        }

        private static HashSet<RnPoint> GetSharedPointSet(RnWay way1, RnWay way2)
        {
            var way1PointSet = new HashSet<RnPoint>(way1.LineString.Points);
            var way2PointSet = new HashSet<RnPoint>(way2.LineString.Points);

            way1PointSet.IntersectWith(way2PointSet);
            return way1PointSet;
        }
    }
}