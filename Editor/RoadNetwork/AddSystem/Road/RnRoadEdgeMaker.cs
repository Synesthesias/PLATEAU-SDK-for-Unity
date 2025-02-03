using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.AddSystem
{
    internal struct SideWalkEdgeInfo
    {
        public bool IsInsidePrev;
        public bool IsOutsidePrev;
        public RnSideWalk SideWalk;
        public RnWay Edge;
        public bool IsStartEdge;
    }

    internal struct RoadEdgeInfo
    {
        public ExtensibleRoadEdge Edge;
        public SideWalkEdgeInfo LeftSideWalkEdge;
        public SideWalkEdgeInfo RightSideWalkEdge;
    }

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

        public RoadEdgeInfo Execute(ExtensibleRoadEdge extensibleEdge)
        {
            var road = extensibleEdge.road.Roads[0];
            var scannedLineStrings = new Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)>();
            var edgeCenter = extensibleEdge.center;
            var edgeDirection = Vector3.Cross(extensibleEdge.forward, Vector3.up);
            // TODO: 端点取得の関数化
            Vector3 oldEdgeCenter;
            {
                var way = road.GetLeftWayOfLanes();
                oldEdgeCenter = extensibleEdge.isPrev ^ road.MainLanes[0].IsReverse ^ way.IsReversed ? way.LineString.Points.First().Vertex : way.LineString.Points.Last().Vertex;
            }
            Vector3 oldEdgeDirection;
            {
                var way = road.GetRightWayOfLanes();
                var secondPoint = extensibleEdge.isPrev ^ road.MainLanes.Last().IsReverse ^ way.IsReversed ? way.LineString.Points.First().Vertex : way.LineString.Points.Last().Vertex;
                oldEdgeDirection = secondPoint - oldEdgeCenter;
            }

            var sideWalks = new List<RnSideWalk>();
            var leftSideWalk = FindLeftEdgeSideWalk(road, extensibleEdge.isPrev, out var isLeftInsidePrev, out var isLeftOutsidePrev, out var isLeftStartEdge);
            if (leftSideWalk != null)
            {
                sideWalks.Add(leftSideWalk);
            }
            var rightSideWalk = FindRightEdgeSideWalk(road, extensibleEdge.isPrev, out var isRightInsidePrev, out var isRightOutsidePrev, out var isRightStartEdge);
            if (rightSideWalk != null)
            {
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
                        (oldEdgePoint, newEdgePoint) = scannedLineStrings[way.LineString];
                    }

                    if (oldEdgePoint != null)
                        oldEdgePoints.Add(oldEdgePoint);
                    if (newEdgePoint != null)
                        newEdgePoints.Add(newEdgePoint);
                }

                if (oldEdgePoints.Count == 0 || newEdgePoints.Count == 0)
                {
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
            sideWalkEdgeAligner.Execute(road, leftSideWalk, scannedLineStrings, isLeftInsidePrev, isLeftOutsidePrev, isLeftStartEdge, true, extensibleEdge.isPrev);
            sideWalkEdgeAligner.Execute(road, rightSideWalk, scannedLineStrings, isRightInsidePrev, isRightOutsidePrev, isRightStartEdge, false, extensibleEdge.isPrev);

            var edgeInfo = new RoadEdgeInfo();
            edgeInfo.Edge = RnRoadSkeleton.FindExtensibleEdge(extensibleEdge.road, RnRoadSkeleton.CreateCenterSpline(extensibleEdge.road)).First();

            if (leftSideWalk != null)
                edgeInfo.LeftSideWalkEdge = new SideWalkEdgeInfo
                {
                    IsInsidePrev = isLeftInsidePrev,
                    IsOutsidePrev = isLeftOutsidePrev,
                    SideWalk = leftSideWalk,
                    Edge = isLeftStartEdge ? leftSideWalk.StartEdgeWay : leftSideWalk.EndEdgeWay,
                    IsStartEdge = isLeftStartEdge
                };

            if (rightSideWalk != null)
                edgeInfo.RightSideWalkEdge = new SideWalkEdgeInfo
                {
                    IsInsidePrev = isRightInsidePrev,
                    IsOutsidePrev = isRightOutsidePrev,
                    SideWalk = rightSideWalk,
                    Edge = isRightStartEdge ? rightSideWalk.StartEdgeWay : rightSideWalk.EndEdgeWay,
                    IsStartEdge = isRightStartEdge
                };

            return edgeInfo;
        }

        /// <summary>
        /// 左側の歩道のうち、端に存在するものを取得する
        /// </summary>
        /// <returns></returns>
        private RnSideWalk FindLeftEdgeSideWalk(RnRoad road, bool isPrev, out bool isInsidePrev, out bool isOutsidePrev, out bool isStartEdge)
        {
            var sideWalks = road.SideWalks;
            var leftWay = road.GetLeftWayOfLanes();
            var edgePoint = (isPrev ^ road.MainLanes.First().IsReverse ^ leftWay.IsReversed) ? leftWay.LineString.Points.First() : leftWay.LineString.Points.Last();
            var sideWalk = sideWalks.FirstOrDefault(sideWalk => sideWalk.InsideWay.Contains(edgePoint));

            if (sideWalk == null)
            {
                isInsidePrev = false;
                isOutsidePrev = false;
                isStartEdge = false;
                return null;
            }

            // 内側Wayの端点はもっとも左のレーンのWayの端点と一致するはず
            isInsidePrev = sideWalk.InsideWay.LineString.Points.First() == edgePoint;

            isStartEdge = isInsidePrev ^ sideWalk.InsideWay.IsReversed;

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
            var edgePoint = (isPrev ^ road.MainLanes.Last().IsReverse ^ rightWay.IsReversed) ? rightWay.LineString.Points.First() : rightWay.LineString.Points.Last();
            var sideWalk = sideWalks.FirstOrDefault(sideWalk => sideWalk.InsideWay.Contains(edgePoint));

            if (sideWalk == null)
            {
                isInsidePrev = false;
                isOutsidePrev = false;
                isStartEdge = false;
                return null;
            }

            // 内側Wayの端点はもっとも右のレーンのWayの端点と一致するはず
            isInsidePrev = sideWalk.InsideWay.LineString.Points.First() == edgePoint;

            isStartEdge = isInsidePrev ^ sideWalk.InsideWay.IsReversed;

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
