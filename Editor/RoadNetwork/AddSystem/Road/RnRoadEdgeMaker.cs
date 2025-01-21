using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.AddSystem
{
    //public struct SideWalkEdgeInfo
    //{
    //    public bool isInsidePrev;
    //    public bool isOutsidePrev;
    //    public RnSideWalk sideWalk;
    //    public RnWay edge;
    //}

    //public struct RoadEdgeInfo
    //{
    //    public ExtensibleRoadEdge road;
    //    public RnWay LeftSideWalkEdge;
    //    public RnWay RightSideWalkEdge;
    //    public RnNeighbor Neighbor;

    //}

    /// <summary>
    /// 新しく道路・交差点を接続するためのエッジを作成するクラス
    /// 道路の中心線に対しエッジを垂直になるように整形し、歩道のエッジが存在しない場合生成する。
    /// </summary>
    internal class RnRoadEdgeMaker : RnRoadEdgeMakerBase<RnRoad>
    {
        RnRoad target;

        public RnRoadEdgeMaker(RnRoad target)
        {
            this.target = target;
        }

        public ExtensibleRoadEdge Execute(ExtensibleRoadEdge extensibleEdge)
        {
            var road = extensibleEdge.road.Roads[0];
            var scannedLineStrings = new Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)>();
            var edgeCenter = extensibleEdge.center;
            var edgeDirection = Vector3.Cross(extensibleEdge.forward, Vector3.up);
            var oldEdgeCenter = road.GetLeftWayOfLanes().LineString.Points.First().Vertex;
            var oldEdgeDirection = road.GetRightWayOfLanes().LineString.Points.Last().Vertex - oldEdgeCenter;
            new GameObject("oldEdgeCenter").transform.position = road.GetLeftWayOfLanes().LineString.Points.First().Vertex;
            new GameObject("oldEdgeNext").transform.position = road.GetRightWayOfLanes().LineString.Points.Last().Vertex;

            var sideWalks = new List<RnSideWalk>();
            var leftSideWalk = FindLeftEdgeSideWalk(road, extensibleEdge.isPrev, out var isLeftInsidePrev, out var isLeftOutsidePrev, out var isLeftStartEdge);
            if (leftSideWalk != null)
            {
                Debug.Log("Left side walk");
                sideWalks.Add(leftSideWalk);
            }
            var rightSideWalk = FindRightEdgeSideWalk(road, extensibleEdge.isPrev, out var isRightInsidePrev, out var isRightOutsidePrev, out var isRightStartEdge);
            if (rightSideWalk != null)
            {
                Debug.Log("Right side walk");
                sideWalks.Add(rightSideWalk);
            }

            foreach (var lane in road.AllLanesWithMedian)
            {
                var oldEdgePoints = new List<RnPoint>();
                var newEdgePoints = new List<RnPoint>();

                foreach (var way in new[] { lane.LeftWay, lane.RightWay })
                {
                    var isPrev = extensibleEdge.isPrev ^ way.IsReversed ^ lane.IsReverse;

                    RnPoint oldEdgePoint = null;
                    RnPoint newEdgePoint = null;
                    if (!scannedLineStrings.ContainsKey(way.LineString))
                    {
                        AlignWayEdge(way.LineString.Points, oldEdgeCenter, oldEdgeDirection, edgeCenter, edgeDirection, out oldEdgePoint, out newEdgePoint, isPrev);
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

                if (oldEdgePoints.Count == 0 || newEdgePoints.Count == 0)
                {
                    //Debug.LogWarning($"{laneBorderType}, {oldEdgePoints.Count}, {newEdgePoints.Count}, {lane.GetBorder(laneBorderType).Contains(oldEdgePoints.Last())}");
                    Debug.LogWarning($"old edge count: {oldEdgePoints.Count}, new edge count: {newEdgePoints.Count}");
                    continue;
                }

                // ボーダー再構築
                foreach (var laneBorderType in new List<RnLaneBorderType> { RnLaneBorderType.Prev, RnLaneBorderType.Next })
                {
                    if (lane.GetBorder(laneBorderType) == null || !lane.GetBorder(laneBorderType).Contains(oldEdgePoints.Last()))
                        continue;
                    lane.SetBorder(laneBorderType, new RnWay(new RnLineString(newEdgePoints)));
                }
            }

            var sideWalkEdgeAligner = new RnSideWalkEdgeAligner(oldEdgeCenter, oldEdgeDirection, edgeCenter, edgeDirection);
            sideWalkEdgeAligner.Execute(leftSideWalk, scannedLineStrings, isLeftInsidePrev, isLeftOutsidePrev, isLeftStartEdge);
            sideWalkEdgeAligner.Execute(rightSideWalk, scannedLineStrings, isRightInsidePrev, isRightOutsidePrev, isRightStartEdge);

            return RnRoadSkeleton.FindExtensibleEdge(extensibleEdge.road, RnRoadSkeleton.CreateCenterSpline(extensibleEdge.road)).First();
        }

        ///// <summary>
        ///// Lineに重なるエッジを作成する
        ///// </summary>
        ///// <param name="edgeLineOrigin"></param>
        ///// <param name="edgeLineDirection"></param>
        ///// <returns></returns>
        //public IntersectionEdgeInfo Execute(Vector3 edgeLineOrigin, Vector3 edgeLineDirection)
        //{
        //    var neighbor = FindNeighborOnEdge(edgeLineOrigin, edgeLineDirection);

        //    if (neighbor == null)
        //    {
        //        return new IntersectionEdgeInfo();
        //    }

        //    var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
        //    var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);

        //    return new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
        //}

        //public IntersectionEdgeInfo Execute(RnNeighbor neighbor, int index)
        //{
        //    var edgeLineOrigin = neighbor.Border.Points.ElementAt(index);
        //    var edgeLineDirection = neighbor.Border.Points.ElementAt(index + 1).Vertex - neighbor.Border.Points.ElementAt(index);
        //    var leftSideWalkEdge = FindLeftSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
        //    var rightSideWalkEdge = FindRightSideWalkEdge(edgeLineOrigin, edgeLineDirection, neighbor);
        //    if (index > 0)
        //    {
        //        var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Take(index + 1)));
        //        target.AddEdge(null, newWay);
        //        neighbor.Border.Points = neighbor.Border.Points.Skip(index);
        //    }
        //    if (index < neighbor.Border.Points.Count() - 1)
        //    {
        //        var newWay = new RnWay(new RnLineString(neighbor.Border.Points.Skip(1)));
        //        target.AddEdge(null, newWay);
        //        neighbor.Border.Points = neighbor.Border.Points.Take(2);
        //    }
        //    target.Align();

        //    return new IntersectionEdgeInfo(edgeLineOrigin, edgeLineDirection, leftSideWalkEdge, rightSideWalkEdge, neighbor);
        //}

        /// <summary>
        /// 左側の歩道のうち、端に存在するものを取得する
        /// </summary>
        /// <returns></returns>
        private RnSideWalk FindLeftEdgeSideWalk(RnRoad road, bool isPrev, out bool isInsidePrev, out bool isOutsidePrev, out bool isStartEdge)
        {
            var sideWalks = road.SideWalks;
            var leftWay = road.GetLeftWayOfLanes();
            var edgePoint = isPrev ? leftWay.LineString.Points.First() : leftWay.LineString.Points.Last();
            new GameObject("edgePoint").transform.position = edgePoint;
            var sideWalk = sideWalks.FirstOrDefault(sideWalk => sideWalk.InsideWay.Contains(edgePoint));
            // 内側Wayの端点はもっとも左のレーンのWayの端点と一致するはず
            isInsidePrev = sideWalk.InsideWay.LineString.Points.First() == edgePoint;
            isStartEdge = sideWalk.StartEdgeWay == null || sideWalk.StartEdgeWay.LineString.Points.Contains(edgePoint);
            // HACK: 外側Wayの向きは無理やり判定
            var edge = isStartEdge ? sideWalk.StartEdgeWay : sideWalk.EndEdgeWay;
            isOutsidePrev = 
                sideWalk.OutsideWay.LineString.Points.First() == edgePoint ||
                sideWalk.OutsideWay.LineString.Points.First() == (isInsidePrev ? sideWalk.InsideWay.LineString.Points.First() : sideWalk.InsideWay.LineString.Points.Last()) ||
                (edge != null && edge.LineString.Points.Contains(sideWalk.OutsideWay.LineString.Points.First()));
            return sideWalk;
        }

        /// <summary>
        /// 右側の歩道のうち、端に存在するものを取得する
        /// </summary>
        /// <returns></returns>
        private RnSideWalk FindRightEdgeSideWalk(RnRoad road, bool isPrev, out bool isInsidePrev, out bool isOutsidePrev, out bool isStartEdge)
        {
            var sideWalks = road.SideWalks;
            var rightWay = road.GetRightWayOfLanes();
            var edgePoint = !isPrev ? rightWay.LineString.Points.First() : rightWay.LineString.Points.Last();
            var sideWalk = sideWalks.FirstOrDefault(sideWalk => sideWalk.InsideWay.Contains(edgePoint));
            // 内側Wayの端点はもっとも右のレーンのWayの端点と一致するはず
            isInsidePrev = sideWalk.InsideWay.LineString.Points.First() == edgePoint;
            isStartEdge = sideWalk.StartEdgeWay == null || sideWalk.StartEdgeWay.LineString.Points.Contains(edgePoint);
            // HACK: 外側Wayの向きは無理やり判定
            var edge = isStartEdge ? sideWalk.StartEdgeWay : sideWalk.EndEdgeWay;
            isOutsidePrev =
                sideWalk.OutsideWay.LineString.Points.First() == edgePoint ||
                sideWalk.OutsideWay.LineString.Points.First() == (isInsidePrev ? sideWalk.InsideWay.LineString.Points.First() : sideWalk.InsideWay.LineString.Points.Last()) ||
                (edge != null && edge.LineString.Points.Contains(sideWalk.OutsideWay.LineString.Points.First()));
            return sideWalk;
        }
    }
}
