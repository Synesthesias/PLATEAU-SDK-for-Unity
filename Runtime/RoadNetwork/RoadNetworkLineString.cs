using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLineString : IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnId<RoadNetworkDataLineString> MyId { get; set; }

        public List<RoadNetworkPoint> Points { get; } = new List<RoadNetworkPoint>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public int Count => Points.Count;

        public static RoadNetworkLineString Create(IEnumerable<RoadNetworkPoint> vertices)
        {
            var ret = new RoadNetworkLineString();
            ret.Points.AddRange(vertices);
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
}