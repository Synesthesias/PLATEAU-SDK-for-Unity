using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using PLATEAU.RoadNetwork.Structure;
using Unity.Mathematics;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// �X�v���C�����瓹�H�ւ̔��f���������ʉ��������[�e�B���e�B
    /// </summary>
    internal static class RnRoadBuildUtil
    {
        /// <summary>
        /// RnSplineEditor�� ApplySpline �Ɠ��l�̃��W�b�N�𔲂��o���B
        /// �X�v���C�������ɁA�w�肵�� roadGroup.Roads[0] ���X�V�����B
        /// </summary>
        public static void BuildRoadFromSpline(RnRoadGroup roadGroup, Spline spline)
        {
            // FIXME: UI����ύX�\�ɂ���iRnSplineEditor�Ɠ������l�j
            const float medianWidth = 2f;
            const float laneWidth = 3f;

            var road = roadGroup.Roads[0];
            var totalWidth = road.AllLanes.Count() * laneWidth;
            if (road.MedianLane != null)
            {
                totalWidth += medianWidth;
            }

            // �����̌����Ȃ�
            MergeSeparatedSideWalks(road, out var leftSideWalk, out var rightSideWalk);

            // ���H���S����̃I�t�Z�b�g��
            var offset = totalWidth / 2;

            // ���Ԑ��Q
            foreach (var leftLane in road.GetLeftLanes())
            {
                var points = leftLane.LeftWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.LeftWay.IsReversed);
                offset -= laneWidth;

                points = leftLane.RightWay.LineString.Points;
                ConvertSplineToLineStringPoints(spline, ref points, offset, !leftLane.RightWay.IsReversed);
            }

            // ������
            if (road.MedianLane != null)
            {
                offset -= medianWidth;
            }

            // �E�Ԑ��Q
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
        /// ApplySpline������ ConvertSplineToLineStringPoints 
        /// </summary>
        private static void ConvertSplineToLineStringPoints(Spline spline, ref List<RnPoint> destPoints, float offset, bool isForward)
        {
            var firstPoint = destPoints.First();
            var lastPoint = destPoints.Last();
            destPoints.Clear();

            // �n�_
            float t = isForward ? 0f : 1f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            firstPoint.Vertex = GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset);
            destPoints.Add(firstPoint);

            while (isForward ? t < 1f : t > 0f)
            {
                // 1m���ɃX�v���C����̓_������Ă��āA30m�ȏ㗣��Ă��邩5�x�ȏ�p�x���قȂ�ꍇ�ɒ��_�ǉ�
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
                    // �I�_
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
        /// ApplySpline������ MergeSeparatedSideWalks
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
            // ... RnSplineEditor�Ɠ��l�̎���
            newSideWalk = null;
            // �i�����F����MergeSeparatedWalkWays���W�b�N���ڐA�j
            // �K�v�Ȃ炻�̂܂܃R�s�y�ł�OK
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