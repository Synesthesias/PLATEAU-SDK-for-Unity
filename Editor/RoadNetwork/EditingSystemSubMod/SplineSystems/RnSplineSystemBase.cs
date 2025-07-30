using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// スプラインを利用したシステムの抽象クラス。
    /// 
    /// - SplineContainer / SplineInstantiate の初期化
    /// - SplineCreateHandles を使ったノット追加・移動
    /// - 作図完了の検知
    /// 
    /// 継承先で以下を実装する:
    /// 1. 頂点リストの取得 (GetVertexList)
    /// 2. 頂点クリック時の処理 (HandleVertexPicking)
    /// 3. スプライン作図完了時の処理 (OnSplineCreationFinished)
    /// </summary>
    internal abstract class RnSplineSystemBase
    {
        /// <summary>
        /// 編集結果保持用のSpline 
        /// </summary>
        protected Spline spline = new Spline();

        /// <summary>
        /// スプライン編集用のコアクラス
        /// </summary>
        protected SplineEditorCore splineEditorCore;

        protected GameObject roadNetworkObject;

        public RnSplineSystemBase()
        {
            splineEditorCore = new SplineEditorCore(spline);
        }

        public abstract void HandleSceneGUI(Object target);

        /// <summary>
        /// SplineをLineStringに変換する。
        /// </summary>
        protected static List<Vector3> ConvertSplineToLineStringPoints(Spline spline, float offset, bool isReversed, float distanceThreshold = 30f, float angleThreshold = 5f)
        {
            var destPoints = new List<Vector3>();

            // 始点に頂点を追加
            float t = isReversed ? 1f : 0f;
            Vector3 prevPoint = spline.EvaluatePosition(t);
            Vector3 prevTangent = spline.EvaluateTangent(t);
            destPoints.Add(GetOffsetPointToNormalDirection(prevPoint, prevTangent, offset));

            while (isReversed ? t > 0f : t < 1f)
            {
                // 1m毎にスプライン上の点を取ってきて、distanceThreshold以上離れているかangleThreshold以上角度が異なる場合に頂点として追加
                spline.GetPointAtLinearDistance(t, isReversed ? -1f : 1f, out float newT);
                var newPoint = spline.EvaluatePosition(newT);
                var newTangent = spline.EvaluateTangent(newT);

                if (Vector3.Distance(prevPoint, newPoint) > distanceThreshold || Vector3.Angle(prevTangent, newTangent) > angleThreshold)
                {
                    destPoints.Add(GetOffsetPointToNormalDirection(newPoint, newTangent, offset));
                    prevPoint = newPoint;
                    prevTangent = newTangent;
                }

                t = newT;

                if (isReversed ? t <= 0f : t >= 1f)
                {
                    // 終点に頂点を追加
                    var lastPoint = GetOffsetPointToNormalDirection(
                        spline.EvaluatePosition(isReversed ? 0f : 1f),
                        spline.EvaluateTangent(isReversed ? 0f : 1f),
                        offset);

                    // 頂点重複を避けるため、最後の点と前回の点が近い場合は最後の点を削除
                    if (Vector3.Distance(lastPoint, prevPoint) <= 1f)
                    {
                        destPoints.RemoveAt(destPoints.Count - 1);
                    }

                    destPoints.Add(lastPoint);
                }
            }

            return destPoints;
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

        private static void MergeSeparatedWalkWays(RnRoad road, List<(RnSideWalk, HashSet<RnPoint>)> sideWalkAndSharedPointSets,
            RnWay sideWay, out RnSideWalk newSideWalk)
        {
            // leftWayの点の順番でSideWalkを並べ替える
            sideWalkAndSharedPointSets.Sort((a, b) =>
            {
                var aFirstPoint = a.Item2.First();
                var bFirstPoint = b.Item2.First();
                return sideWay.LineString.Points.IndexOf(aFirstPoint) - sideWay.LineString.Points.IndexOf(bFirstPoint);
            });

            // 並べ替えたWalkWayを結合
            var outsidePoints = new List<RnPoint>();
            int cnt = 0;
            foreach (var (sideWalk, _) in sideWalkAndSharedPointSets)
            {
                var isFirstWay = cnt == 0;
                var isLastWay = cnt == sideWalkAndSharedPointSets.Count - 1;
                cnt++;

                var separatedOutsidePoints = sideWalk.OutsideWay.LineString.Points;
                int firstPointIndex;
                int lastPointIndex;
                if (isFirstWay)
                {
                    firstPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.First())
                        ? int.MinValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.Last())
                        ? int.MinValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }
                else if (isLastWay)
                {
                    firstPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.First())
                        ? int.MaxValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = IsEdgePoint(sideWalk, separatedOutsidePoints.Last())
                        ? int.MaxValue
                        : sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }
                else
                {
                    firstPointIndex = sideWay.LineString.Points.IndexOf(separatedOutsidePoints.First());
                    lastPointIndex = sideWay.LineString.Points.IndexOf(separatedOutsidePoints.Last());
                }

                if (firstPointIndex > lastPointIndex)
                    separatedOutsidePoints.Reverse();

                for (int i = 0; i < separatedOutsidePoints.Count; ++i)
                {
                    // 端の点は接続情報を保持するために参照を保持
                    if (i == 0 && isFirstWay)
                    {
                        outsidePoints.Add(separatedOutsidePoints[i]);
                        continue;
                    }
                    if (i == separatedOutsidePoints.Count - 1 && isLastWay)
                    {
                        outsidePoints.Add(separatedOutsidePoints[i]);
                        continue;
                    }

                    outsidePoints.Add(new RnPoint(separatedOutsidePoints[i]));
                }
                road.RemoveSideWalk(sideWalk);
            }
            var outsideWay = new RnWay(new RnLineString(outsidePoints.AsReadOnly()));
            var insideWay = new RnWay(sideWay.LineString);
            var startEdgeWay = sideWalkAndSharedPointSets.First().Item1.StartEdgeWay;
            var endEdgeWay = sideWalkAndSharedPointSets.Last().Item1.EndEdgeWay;
            newSideWalk = RnSideWalk.Create(road, outsideWay, insideWay, startEdgeWay, endEdgeWay);
            road.AddSideWalk(newSideWalk);
        }

        private static bool IsEdgePoint(RnSideWalk sideWalk, RnPoint point)
        {
            if (sideWalk.StartEdgeWay != null && sideWalk.StartEdgeWay.LineString.Points.Contains(point))
                return true;
            if (sideWalk.EndEdgeWay != null && sideWalk.EndEdgeWay.LineString.Points.Contains(point))
                return true;

            return false;
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