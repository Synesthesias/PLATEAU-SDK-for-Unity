using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.AddSystem
{
    public struct IntersectionEdgeInfo
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public RnWay LeftSideWalkEdge;
        public RnWay RightSideWalkEdge;
        public RnNeighbor Neighbor;

        public IntersectionEdgeInfo(Vector3 origin, Vector3 direction, RnWay leftSideWalkEdge, RnWay rightSideWalkEdge, RnNeighbor neighbor)
        {
            Origin = origin;
            Direction = direction;
            LeftSideWalkEdge = leftSideWalkEdge;
            RightSideWalkEdge = rightSideWalkEdge;
            Neighbor = neighbor;
        }
    }

    /// <summary>
    /// 新しく道路・交差点を接続するためのエッジを作成するクラス
    /// </summary>
    public class RnRoadEdgeMaker
    {
        RnIntersection target;

        public RnRoadEdgeMaker(RnIntersection target)
        {
            this.target = target;
        }

        /// <summary>
        /// Lineに重なるエッジを作成する
        /// </summary>
        /// <param name="edgeLineOrigin"></param>
        /// <param name="edgeLineDirection"></param>
        /// <returns></returns>
        public IntersectionEdgeInfo Execute(Vector3 edgeLineOrigin, Vector3 edgeLineDirection)
        {
            var neighbor = FindNeighborOnEdge(edgeLineOrigin, edgeLineDirection);

            if (neighbor == null)
            {
                return new IntersectionEdgeInfo();
            }

            var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
            var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);

            return new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
        }

        public IntersectionEdgeInfo Execute(RnNeighbor neighbor)
        {
            var edgeLineOrigin = neighbor.Border.Points.First();
            var edgeLineDirection = neighbor.Border.Points.Last().Vertex - neighbor.Border.Points.First().Vertex;
            var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
            var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
            return new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
        }

        private RnWay FindRightSideWalkEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection, RnNeighbor neighbor)
        {
            RnWay rightSideWalkEdge = null;
            foreach (var sideWalk in target.SideWalks)
            {
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                if (isStartEdge && sideWalk.StartEdgeWay.LineString.Points.Contains(neighbor.Border.Points.Last()))
                {
                    rightSideWalkEdge = sideWalk.StartEdgeWay;
                    break;
                }
                if (!isStartEdge && sideWalk.EndEdgeWay.LineString.Points.Contains(neighbor.Border.Points.Last()))
                {
                    rightSideWalkEdge = sideWalk.EndEdgeWay;
                    break;
                }
            }

            return rightSideWalkEdge;
        }

        private RnWay FindLeftSideWalkEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection, RnNeighbor neighbor)
        {
            foreach (var sideWalk in target.SideWalks)
            {
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                if (isStartEdge && sideWalk.StartEdgeWay.LineString.Points.Contains(neighbor.Border.Points.First()))
                {
                    return sideWalk.StartEdgeWay;
                }
                if (!isStartEdge && sideWalk.EndEdgeWay.LineString.Points.Contains(neighbor.Border.Points.First()))
                {
                    return sideWalk.EndEdgeWay;
                }
            }
            return null;
        }

        private bool IsBorderOnEdge(RnWay border, Vector3 edgeLineOrigin, Vector3 edgeLineDirection)
        {
            var points = border.LineString.Points;
            foreach (var point in points)
            {
                if (GetDistanceToLine(point, edgeLineOrigin, edgeLineDirection) > 1f)
                {
                    return false;
                }
            }
            return true;
        }

        private RnNeighbor FindNeighborOnEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection)
        {
            foreach (var neighbor in target.Edges)
            {
                if (neighbor.IsBorder)
                    continue;

                if (IsBorderOnEdge(neighbor.Border, edgeLineOrigin, edgeLineDirection))
                {
                    return neighbor;
                }
            }
            return null;
        }

        private static RnWay GetOrCreateSideWalkEdge(RnSideWalk sideWalk, Vector3 edgeLineOrigin, Vector3 edgeLineDirection, out bool isStartEdge)
        {
            isStartEdge = false;
            var points = sideWalk.OutsideWay.LineString.Points;
            // pointsの終点側でエッジ上に存在する点群を取得
            var pointsOnEdge = new List<RnPoint>();
            foreach (var point in new Stack<RnPoint>(points))
            {
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

        private static float GetDistanceToLine(Vector3 point, Vector3 origin, Vector3 direction)
        {
            return Vector3.Cross(point - origin, direction).magnitude;
        }
    }
}
