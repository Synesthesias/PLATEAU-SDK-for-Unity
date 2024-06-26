using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 線分群クラス. 頂点のリストを持つ
    /// </summary>
    [Serializable]
    public class RoadNetworkLineString : ARoadNetworkParts<RoadNetworkLineString>, IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        public List<RoadNetworkPoint> Points { get; } = new List<RoadNetworkPoint>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public int Count => Points.Count;

        // 頂点が2つ以上ある有効な線分かどうか
        public bool IsValid => Count >= 2;

        public static RoadNetworkLineString Create(IEnumerable<RoadNetworkPoint> vertices)
        {
            var ret = new RoadNetworkLineString();
            foreach (var v in vertices)
                ret.AddPointOrSkip(v);
            return ret;
        }

        public static RoadNetworkLineString Create(IEnumerable<Vector3> vertices)
        {
            return Create(vertices.Select(v => new RoadNetworkPoint(v)));
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// </summary>
        /// <returns></returns>
        public List<RoadNetworkLineString> Split(int num)
        {
            // 分割できない時は空を返す
            var splitLines = LineUtil.SplitLineSegments(this, num).ToList();
            return splitLines.Select(x => Create(x.Select(a => new RoadNetworkPoint(a)))).ToList();
        }

        /// <summary>
        /// 後ろの点が同じなら追加しない
        /// </summary>
        /// <param name="p"></param>
        public void AddPointOrSkip(RoadNetworkPoint p)
        {
            if (Points.Count > 0 && Points.Last().Vertex == p.Vertex)
                return;
            Points.Add(p);
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return Points.Select(v => v.Vertex).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Vector3 this[int index] => Points[index].Vertex;
    }

    public static class RoadNetworkLineStringEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RoadNetworkLineString self)
        {
            foreach (var e in GeoGraphEx.GetEdges(self.Points.Select(x => x.Vertex.Xz()), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }
    }
}