using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLineString
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();

        public int Count => Vertices.Count;

        public static RoadNetworkLineString Create(IEnumerable<Vector3> vertices)
        {
            var ret = new RoadNetworkLineString();
            ret.Vertices.AddRange(vertices);
            return ret;
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// </summary>
        /// <returns></returns>
        public List<RoadNetworkLineString> Split(int num)
        {
            // 分割できない時は空を返す
            var splitLines = LineUtil.SplitLineSegments(Vertices, num).ToList();
            return splitLines.Select(Create).ToList();
        }
    }
}