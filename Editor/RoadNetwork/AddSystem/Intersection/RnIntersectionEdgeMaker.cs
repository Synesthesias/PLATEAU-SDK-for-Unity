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

        public IntersectionEdgeInfo Execute(RnNeighbor neighbor, int index)
        {
            // ボーダーの元の点リストを取得
            var borderPointList = neighbor.Border.Points.ToList();
            // edgeLineOrigin と edgeLineDirection を求める (index と index+1 の点を使用)
            Vector3 edgeLineOrigin = borderPointList.ElementAt(index).Vertex;
            Vector3 edgeLineDirection = borderPointList.ElementAt(index + 1).Vertex - borderPointList.ElementAt(index).Vertex;

            // ボーダーを分割し、最小な拡張可能部分だけ切り出す
            // (ボーダーの始端側の余分な部分を新たなエッジとして target に追加)
            if (index > 0)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Take(index + 1)));
                target.AddEdge(null, newWay);
                // ボーダーを先頭側で分割
                neighbor.Border.Points = neighbor.Border.Points.Skip(index);
                borderPointList = neighbor.Border.Points.ToList();
                // 先頭部分削除したのでindexを更新
                index = 0;
            }
            // (ボーダーの終端側の余分な部分を新たなエッジとして target に追加)
            if (index < borderPointList.Count() - 1)
            {
                var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Skip(1)));
                target.AddEdge(null, newWay);
                // ボーダーを終端側で分割（先頭2点のみ残す）
                neighbor.Border.Points = neighbor.Border.Points.Take(2);
                borderPointList = neighbor.Border.Points.ToList();
            }

            // tolerance の設定（edgeLine上かどうかの判定用）
            float tolerance = 1f;
            // 現在のボーダー点のリストを取得
            List<RnPoint> targetBorderPoints = neighbor.Border.Points.ToList();

            // 追加できる辺がなくなるまで繰り返す
            bool addedNewSegment;
            do
            {
                addedNewSegment = false;
                // 最新のボーダー始点・終点を取得
                RnPoint borderStart = targetBorderPoints.First();
                RnPoint borderEnd = targetBorderPoints.Last();

                // target 内の各エッジを走査
                foreach (var otherEdge in target.Edges)
                {
                    // 自分自身（neighbor）は除外
                    if (otherEdge == neighbor)
                        continue;

                    // 他エッジのボーダー点リストを取得
                    var otherPoints = otherEdge.Border.LineString.Points;
                    if (otherPoints.Count == 0)
                        continue;

                    // --- ケース1: 他エッジの始点がボーダーの終点と一致する場合 ---
                    if (Vector3.Distance(otherPoints.First().Vertex, borderEnd.Vertex) < 0.001f)
                    {
                        List<RnPoint> collinearSegment = new List<RnPoint>();
                        // 他エッジの先頭は既に接続しているので、2点目以降から連続する点を抜き出す
                        for (int i = 1; i < otherPoints.Count; i++)
                        {
                            if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                                collinearSegment.Add(otherPoints[i]);
                            else
                                break;
                        }
                        if (collinearSegment.Count > 0)
                        {
                            targetBorderPoints.AddRange(collinearSegment);
                            otherPoints.RemoveRange(0, collinearSegment.Count);
                            addedNewSegment = true;
                        }
                    }
                    // --- ケース2: 他エッジの終点がボーダーの終点と一致する場合 ---
                    else if (Vector3.Distance(otherPoints.Last().Vertex, borderEnd.Vertex) < 0.001f)
                    {
                        // 他エッジの順序を反転して、始点側として扱う
                        otherPoints.Reverse();
                        List<RnPoint> collinearSegment = new List<RnPoint>();
                        for (int i = 1; i < otherPoints.Count; i++)
                        {
                            if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                                collinearSegment.Add(otherPoints[i]);
                            else
                                break;
                        }
                        if (collinearSegment.Count > 0)
                        {
                            targetBorderPoints.AddRange(collinearSegment);
                            otherPoints.RemoveRange(otherPoints.Count - collinearSegment.Count, collinearSegment.Count);
                            addedNewSegment = true;
                        }
                    }
                    // --- ケース3: 他エッジの始点がボーダーの始点と一致する場合 ---
                    else if (Vector3.Distance(otherPoints.First().Vertex, borderStart.Vertex) < 0.001f)
                    {
                        List<RnPoint> collinearSegment = new List<RnPoint>();
                        for (int i = 1; i < otherPoints.Count; i++)
                        {
                            if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                                collinearSegment.Add(otherPoints[i]);
                            else
                                break;
                        }
                        if (collinearSegment.Count > 0)
                        {
                            // 先頭側に追加するため、逆順にして挿入
                            collinearSegment.Reverse();
                            targetBorderPoints.InsertRange(0, collinearSegment);
                            otherPoints.RemoveRange(0, collinearSegment.Count);
                            addedNewSegment = true;
                        }
                    }
                    // --- ケース4: 他エッジの終点がボーダーの始点と一致する場合 ---
                    else if (Vector3.Distance(otherPoints.Last().Vertex, borderStart.Vertex) < 0.001f)
                    {
                        otherPoints.Reverse();
                        List<RnPoint> collinearSegment = new List<RnPoint>();
                        for (int i = 1; i < otherPoints.Count; i++)
                        {
                            if (GetDistanceToLine(otherPoints[i].Vertex, edgeLineOrigin, edgeLineDirection) < tolerance)
                                collinearSegment.Add(otherPoints[i]);
                            else
                                break;
                        }
                        if (collinearSegment.Count > 0)
                        {
                            collinearSegment.Reverse();
                            targetBorderPoints.InsertRange(0, collinearSegment);
                            otherPoints.RemoveRange(otherPoints.Count - collinearSegment.Count, collinearSegment.Count);
                            addedNewSegment = true;
                        }
                    }

                    if (otherPoints.Count == 0)
                    {
                        target.RemoveEdges(x => x.Border == otherEdge.Border);
                    }
                }
            } while (addedNewSegment);

            // ループ終了後、統合した点列で neighbor のボーダーを更新
            neighbor.Border.LineString.Points.Clear();
            neighbor.Border.LineString.Points.Add(targetBorderPoints.First());
            neighbor.Border.LineString.Points.Add(targetBorderPoints.Last());
            neighbor.Border.IsReversed = false;

            // 歩道エッジを取得
            var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor, out var leftSideWalk);
            var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor, out var rightSideWalk);
            target.Align();

            if (leftSideWalkEdge != null)
                OrderSideWalkEdge(leftSideWalkEdge, leftSideWalk, true);
            if (rightSideWalkEdge != null)
                OrderSideWalkEdge(rightSideWalkEdge, rightSideWalk, false);

            var newEdgeInfo = new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
            DebugShowEdgeInfo(newEdgeInfo);
            return newEdgeInfo;
        }

        public static void DebugShowEdgeInfo(IntersectionEdgeInfo edgeInfo)
        {
            // 左から右の順番で点が表示されていればOK
            if (edgeInfo.LeftSideWalkEdge != null)
            {
                new GameObject("leftSW0").transform.position = edgeInfo.LeftSideWalkEdge.LineString.Points[0];
                new GameObject("leftSW1").transform.position = edgeInfo.LeftSideWalkEdge.LineString.Points[^1];
            }
            new GameObject("border0").transform.position = edgeInfo.Neighbor.Border.LineString.Points[0];
            new GameObject("border1").transform.position = edgeInfo.Neighbor.Border.LineString.Points[^1];
            if (edgeInfo.RightSideWalkEdge != null)
            {
                new GameObject("rightSW0").transform.position = edgeInfo.RightSideWalkEdge.LineString.Points[0];
                new GameObject("rightSW1").transform.position = edgeInfo.RightSideWalkEdge.LineString.Points[^1];
            }
        }

        private RnWay FindRightSideWalkEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection, RnNeighbor neighbor, out RnSideWalk outSideWalk)
        {
            RnWay rightSideWalkEdge = null;
            foreach (var sideWalk in target.SideWalks)
            {
                // ボーダーと同一直線状にある歩道エッジを取得もしくは既存道路外縁から生成
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                // ボーダーから見て右側のエッジだけ使用。ボーダーは時計回りのはずなので、最後の点がエッジに含まれているかで判定
                if (isStartEdge && sideWalk.StartEdgeWay.LineString.Points.Any(p => Vector3.Distance(p.Vertex, neighbor.Border.Points.Last().Vertex) < 0.01f))
                {
                    rightSideWalkEdge = sideWalk.StartEdgeWay;
                    break;
                }
                if (!isStartEdge && sideWalk.EndEdgeWay.LineString.Points.Any(p => Vector3.Distance(p.Vertex, neighbor.Border.Points.Last().Vertex) < 0.01f))
                {
                    rightSideWalkEdge = sideWalk.EndEdgeWay;
                    break;
                }
            }

            outSideWalk = target.SideWalks.First();
            return rightSideWalkEdge;
        }

        private RnWay FindLeftSideWalkEdge(Vector3 edgeLineOrigin, Vector3 edgeLineDirection, RnNeighbor neighbor, out RnSideWalk outSideWalk)
        {
            foreach (var sideWalk in target.SideWalks)
            {
                // ボーダーと同一直線状にある歩道エッジを取得もしくは既存道路外縁から生成
                var sideWalkEdge = GetOrCreateSideWalkEdge(sideWalk, edgeLineOrigin, edgeLineDirection, out bool isStartEdge);

                if (sideWalkEdge == null)
                    continue;

                // ボーダーから見て左側のエッジだけ使用。ボーダーは時計回りのはずなので、最初の点がエッジに含まれているかで判定
                if (isStartEdge && sideWalk.StartEdgeWay.LineString.Points.Any(p => Vector3.Distance(p.Vertex, neighbor.Border.Points.First().Vertex) < 0.01f))
                {
                    outSideWalk = sideWalk;
                    return sideWalk.StartEdgeWay;
                }
                if (!isStartEdge && sideWalk.EndEdgeWay.LineString.Points.Any(p => Vector3.Distance(p.Vertex, neighbor.Border.Points.First().Vertex) < 0.01f))
                {
                    outSideWalk = sideWalk;
                    return sideWalk.EndEdgeWay;
                }
            }
            outSideWalk = null;
            return null;
        }

        /// <summary>
        /// 左から右に頂点を並べる
        /// </summary>
        /// <param name=""></param>
        private static void OrderSideWalkEdge(RnWay edge, RnSideWalk sideWalk, bool isLeft)
        {
            var points = edge.LineString.Points.ToList();
            var sideWalkOutPoints = sideWalk.OutsideWay.LineString.Points.ToList();
            if (isLeft)
            {
                if (sideWalkOutPoints.Contains(points.Last()))
                {
                    points.Reverse();
                }
            }
            else
            {
                if (sideWalkOutPoints.Contains(points.First()))
                {
                    points.Reverse();
                }
            }
            edge.LineString.Points.Clear();
            edge.LineString.Points.AddRange(points);
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
