using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// レーンとの境界線
    /// </summary>
    [Serializable]
    public class RoadNetworkBorder : IPrimitiveData
    {
        public List<Vector3> vertices = new List<Vector3>();

        public int neighborLaneIndex = -1;

        /// <summary>
        /// 境界線の中央の点を返す
        /// </summary>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public bool TryGetCenterVertex(out Vector3 midPoint)
        {
            return GeoGraph2d.TryGetLineSegmentMidPoint(vertices, out midPoint);
        }
    }
}