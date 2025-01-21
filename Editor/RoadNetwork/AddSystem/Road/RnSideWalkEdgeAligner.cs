using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
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
        RnSideWalk target;
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

        public void Execute(RnSideWalk target, Dictionary<RnLineString, (RnPoint oldEdgePoint, RnPoint newEdgePoint)> scannedLineStrings, bool isInsidePrev, bool isOutsidePrev, bool isStartEdge)
        {
            this.target = target;
            this.scannedLineStrings = scannedLineStrings;
            this.isInsidePrev = isInsidePrev;
            this.isOutsidePrev = isOutsidePrev;
            this.isStartEdge = isStartEdge;
            this.oldEdgePoints = new List<RnPoint>();
            this.newEdgePoints = new List<RnPoint>();

            AlignSideWalk();
        }

        private void AlignSideWalk()
        {
            AlignWay(target.InsideWay, isInsidePrev);
            AlignWay(target.OutsideWay, isOutsidePrev);

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

        private void AlignWay(RnWay way, bool isPrev)
        {
            RnPoint oldEdgePoint = null;
            RnPoint newEdgePoint = null;
            if (!scannedLineStrings.ContainsKey(way.LineString))
            {
                AlignWayEdge(way.LineString.Points, oldEdgeCenter, oldEdgeDirection, newEdgeCenter, newEdgeDirection, out oldEdgePoint, out newEdgePoint, isPrev);
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
    }
}
