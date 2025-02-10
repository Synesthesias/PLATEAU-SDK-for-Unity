using PLATEAU.RoadNetwork.Structure;
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
            // ボーダーの元の点リストを取得
            var borderPointsList = neighbor.Border.Points.ToList();
            // edgeLineOrigin と edgeLineDirection を求める (index と index+1 の点を使用)
            Vector3 edgeLineOrigin = borderPointsList.ElementAt(index).Vertex;
            Vector3 edgeLineDirection = borderPointsList.ElementAt(index + 1).Vertex - borderPointsList.ElementAt(index).Vertex;

            // ボーダーを分割し、最小な拡張可能部分だけ切り出す
            // (ボーダーの始端側の余分な部分を新たなエッジとして target に追加)
            if (index > 0)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Take(index + 1)));
                target.AddEdge(null, newWay);
                // ボーダーを先頭側で分割
                neighbor.Border.Points = neighbor.Border.Points.Skip(index);
                borderPointsList = neighbor.Border.Points.ToList();
            }
            // (ボーダーの終端側の余分な部分を新たなエッジとして target に追加)
            if (index < borderPointsList.Count() - 1)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Skip(1)));
                target.AddEdge(null, newWay);
                // ボーダーを終端側で分割（先頭2点のみ残す）
                neighbor.Border.Points = neighbor.Border.Points.Take(2);
                borderPointsList = neighbor.Border.Points.ToList();
            }

            // ここから、target 内の全てのエッジから連続した同一線上にある辺を抜き出し、ボーダーに移動する処理
            // （※ここでの「同一線上」とは、edgeLineOrigin と edgeLineDirection で定義される直線上にあるかを tolerance で判定）
            float tolerance = 0.1f;
            // neighbor.Border の現在のボーダー点（RnLineString の Points）を取得
            List<RnPoint> targetBorderPoints = neighbor.Border.LineString.Points.ToList();

            // ボーダーの始点・終点を取得
            RnPoint borderStart = targetBorderPoints.First();
            RnPoint borderEnd = targetBorderPoints.Last();

            // target 内の各エッジを走査
            foreach (var otherEdge in target.Edges)
            {
                // 自分自身（neighbor）とは比較しない
                if (otherEdge == neighbor)
                    continue;

                // 他エッジの点リストを取得
                List<RnPoint> otherPoints = otherEdge.Border.LineString.Points.ToList();
                if (otherPoints.Count == 0)
                    continue;

                // --- ケース1: 他エッジの始点がボーダーの終点と一致する場合 ---
                if (Vector3.Distance(otherPoints.First().Vertex, borderEnd.Vertex) < 0.001f)
                {
                    List<RnPoint> collinearSegment = new List<RnPoint>();
                    // 他エッジの先頭は既にボーダーと接続しているので、2点目以降から同一直線上の連続点を抜き出す
                    for (int i = 1; i < otherPoints.Count; i++)
                    {
                        if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                        {
                            collinearSegment.Add(otherPoints[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 連続点をボーダーの終端に追加
                    targetBorderPoints.AddRange(collinearSegment);
                }
                // --- ケース2: 他エッジの終点がボーダーの終点と一致する場合 ---
                else if (Vector3.Distance(otherPoints.Last().Vertex, borderEnd.Vertex) < 0.001f)
                {
                    // 他エッジの順序を反転して、始点として扱う
                    otherPoints.Reverse();
                    List<RnPoint> collinearSegment = new List<RnPoint>();
                    for (int i = 1; i < otherPoints.Count; i++)
                    {
                        if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                        {
                            collinearSegment.Add(otherPoints[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 連続点をボーダーの終端に追加
                    targetBorderPoints.AddRange(collinearSegment);
                }
                // --- ケース3: 他エッジの始点がボーダーの始点と一致する場合 ---
                else if (Vector3.Distance(otherPoints.First().Vertex, borderStart.Vertex) < 0.001f)
                {
                    List<RnPoint> collinearSegment = new List<RnPoint>();
                    // 他エッジの始点は既にボーダーと接続しているので、2点目以降から連続点を抜き出す
                    for (int i = 1; i < otherPoints.Count; i++)
                    {
                        if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                        {
                            collinearSegment.Add(otherPoints[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // ボーダーの始点に連続点を前方へ追加するため、抜き出した点列を逆順にして先頭に挿入
                    collinearSegment.Reverse();
                    targetBorderPoints.InsertRange(0, collinearSegment);
                }
                // --- ケース4: 他エッジの終点がボーダーの始点と一致する場合 ---
                else if (Vector3.Distance(otherPoints.Last().Vertex, borderStart.Vertex) < 0.001f)
                {
                    otherPoints.Reverse();
                    List<RnPoint> collinearSegment = new List<RnPoint>();
                    for (int i = 1; i < otherPoints.Count; i++)
                    {
                        if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                        {
                            collinearSegment.Add(otherPoints[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    collinearSegment.Reverse();
                    targetBorderPoints.InsertRange(0, collinearSegment);
                }
            }
            // ボーダーに統合した点列で、neighbor のボーダーを更新
            neighbor.Border.LineString.Points.Clear();
            neighbor.Border.LineString.Points.AddRange(targetBorderPoints);

            // 既存の処理で歩道エッジを取得
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
