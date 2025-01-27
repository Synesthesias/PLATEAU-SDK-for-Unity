using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.AddSystem
{
    internal class RnSideWalkEdgeAligner : RnRoadEdgeMakerBase<RnSideWalk>
    {
        Vector3 oldEdgeDirection;
        Vector3 oldEdgeCenter;
        Vector3 newEdgeDirection;
        Vector3 newEdgeCenter;

        // Cache
        Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)> scannedLineStrings;
        RnRoad road;
        RnSideWalk target;
        bool isRoadPrev;
        bool isLeftSideWalk;
        bool isInsidePrev;
        bool isOutsidePrev;
        bool isStartEdge;
        List<RnPoint> oldEdgePoints;
        List<RnPoint> newEdgePoints;

        public RnSideWalkEdgeAligner(Vector3 oldEdgeCenter, Vector3 oldEdgeDirection, Vector3 newEdgeCenter, Vector3 newEdgeDirection)
        {
            this.oldEdgeDirection = oldEdgeDirection;
            this.oldEdgeCenter = oldEdgeCenter;
            this.newEdgeDirection = newEdgeDirection;
            this.newEdgeCenter = newEdgeCenter;
        }

        public void Execute(RnRoad road, RnSideWalk target, Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)> scannedLineStrings, bool isInsidePrev, bool isOutsidePrev, bool isStartEdge, bool isLeftSideWalk, bool isRoadPrev)
        {
            this.road = road;
            this.isRoadPrev = isRoadPrev;
            this.target = target;
            this.scannedLineStrings = scannedLineStrings;
            this.isLeftSideWalk = isLeftSideWalk;
            this.isInsidePrev = isInsidePrev;
            this.isOutsidePrev = isOutsidePrev;
            this.isStartEdge = isStartEdge;
            this.oldEdgePoints = new List<RnPoint>();
            this.newEdgePoints = new List<RnPoint>();

            AlignSideWalk();
        }

        private void AlignSideWalk()
        {
            if (target == null)
                return;

            AlignWay(target.InsideWay, isInsidePrev, true);
            AlignWay(target.OutsideWay, isOutsidePrev, false);

            if (oldEdgePoints.Count == 0 || newEdgePoints.Count == 0)
            {
                Debug.LogWarning($"old edge count: {oldEdgePoints.Count}, new edge count: {newEdgePoints.Count}");
                return;
            }

            // エッジ再構築
            if (isStartEdge)
                target.SetStartEdgeWay(new RnWay(new RnLineString(newEdgePoints)));
            else
                target.SetEndEdgeWay(new RnWay(new RnLineString(newEdgePoints)));
        }

        private void AlignWay(RnWay way, bool isPrev, bool isInside)
        {
            RnPoint oldEdgePoint = null;
            RnPoint newEdgePoint = null;
            if (!scannedLineStrings.ContainsKey(way.LineString))
            {
                AlignWayEdge(way.LineString.Points, oldEdgeCenter, oldEdgeDirection, newEdgeCenter, newEdgeDirection, out oldEdgePoint, out newEdgePoint, isPrev);

                if (isInside)
                {
                    RnWay laneEdgeWay;
                    RnPoint edgePoint;

                    if (isLeftSideWalk)
                    {
                        laneEdgeWay = road.GetLeftWayOfLanes();
                        edgePoint = (isRoadPrev ^ road.MainLanes.First().IsReverse ^ laneEdgeWay.IsReversed) ? laneEdgeWay.LineString.Points.First() : laneEdgeWay.LineString.Points.Last();
                    }
                    else
                    {
                        laneEdgeWay = road.GetRightWayOfLanes();
                        edgePoint = (isRoadPrev ^ road.MainLanes.Last().IsReverse ^ laneEdgeWay.IsReversed) ? laneEdgeWay.LineString.Points.First() : laneEdgeWay.LineString.Points.Last();
                    }

                    // Laneの頂点と共有化
                    if (isPrev)
                    {
                        way.LineString.Points.RemoveAt(0);
                        way.LineString.Points.Insert(0, edgePoint);
                    }
                    else
                    {
                        way.LineString.Points.RemoveAt(way.LineString.Points.Count - 1);
                        way.LineString.Points.Add(edgePoint);
                    }
                    newEdgePoint = edgePoint;
                }

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
    }
}
