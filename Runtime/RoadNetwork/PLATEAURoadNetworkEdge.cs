using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class PLATEAURoadNetworkEdge
    {
        public List<Vector3> vertices = new List<Vector3>();

        public int neighborLaneIndex = -1;

        public bool TryGetEdgeCenter(out Vector3 midPoint)
        {
            return PolygonUtil.TryGetLineSegmentMidPoint(vertices, out midPoint);
        }
    }
}