using PLATEAU.RoadNetwork.Structure;
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
    public class RnIntersectionEdgeMaker : RnRoadEdgeMakerBase<RnIntersection>
    {
        RnIntersection target;

        public RnIntersectionEdgeMaker(RnIntersection target)
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

        public IntersectionEdgeInfo Execute(RnNeighbor neighbor, int index)
        {
            var edgeLineOrigin = neighbor.Border.Points.ElementAt(index);
            var edgeLineDirection = neighbor.Border.Points.ElementAt(index + 1).Vertex - neighbor.Border.Points.ElementAt(index);

            // ボーダーを分割し、拡張可能部分だけ切り出し
            if (index > 0)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Take(index + 1)));
                target.AddEdge(null, newWay);
                neighbor.Border.Points = neighbor.Border.Points.Skip(index);
            }
            if (index < neighbor.Border.Points.Count() - 1)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Skip(1)));
                target.AddEdge(null, newWay);
                neighbor.Border.Points = neighbor.Border.Points.Take(2);
            }

            var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
            var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
            target.Align();

            return new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
        }

        private RnWay FindRightSideWalkEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection, RnNeighbor neighbor)
        {
            RnWay rightSideWalkEdge = null;
            foreach (var sideWalk in target.SideWalks)
            {
                // ボーダーと同一直線状にある歩道エッジを取得もしくは既存道路外縁から生成
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                // ボーダーから見て右側のエッジだけ使用。ボーダーは時計回りのはずなので、最後の点がエッジに含まれているかで判定
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
                // ボーダーと同一直線状にある歩道エッジを取得もしくは既存道路外縁から生成
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                // ボーダーから見て左側のエッジだけ使用。ボーダーは時計回りのはずなので、最初の点がエッジに含まれているかで判定
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
    }
}
