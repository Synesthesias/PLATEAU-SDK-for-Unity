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
    public class RnLineString : ARnParts<RnLineString>, IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        public List<RnPoint> Points { get; } = new List<RnPoint>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public int Count => Points.Count;

        // 頂点が2つ以上ある有効な線分かどうか
        public bool IsValid => Count >= 2;

        public static RnLineString Create(IEnumerable<RnPoint> vertices)
        {
            var ret = new RnLineString();
            foreach (var v in vertices)
                ret.AddPointOrSkip(v);
            return ret;
        }

        public static RnLineString Create(IEnumerable<Vector3> vertices)
        {
            return Create(vertices.Select(v => new RnPoint(v)));
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// </summary>
        /// <returns></returns>
        public List<RnLineString> Split(int num, bool insertNewPoint)
        {
            if (Points.Count <= 1)
                return new List<RnLineString>();

            // #TODO : マジックナンバー
            //       : 分割点が隣り合う点とこれ以下の場合は新規で作らず使いまわす
            var threshold = 1e-2f;
            var ret = new List<List<RnPoint>>();
            var totalLength = LineUtil.GetLineSegmentLength(this);
            var length = totalLength / num;
            // 分割長さ
            var mergeLength = Mathf.Min(1e-1f, length * 0.5f);
            threshold = mergeLength * mergeLength;
            var len = 0f;
            var subVertices = new List<RnPoint> { Points[0] };
            for (var i = 1; i < Points.Count; ++i)
            {
                var p0 = subVertices.Last();
                var p1 = Points[i];
                var l = (p1.Vertex - p0.Vertex).magnitude;
                len += l;
                // lenがlengthを超えたら分割線分を追加
                while (len >= length && l > GeoGraph2D.Epsilon)
                {
                    var f = 1f - (len - length) / l;
                    var end = new RnPoint(Vector3.Lerp(p0, p1, f));

                    // もし,p0/p1とほぼ同じ点ならそっちを使う
                    // ただし、その結果subVerticesが線分にならない場合は無視する
                    if (subVertices.Count > 1)
                    {
                        if ((p1.Vertex - end.Vertex).sqrMagnitude < threshold)
                        {
                            end = p1;
                        }
                        else if ((p0.Vertex - end.Vertex).sqrMagnitude < threshold)
                        {
                            end = p0;
                        }
                    }

                    // 同一頂点が複数あった場合は無視する
                    if (f >= float.Epsilon)
                    {
                        subVertices.Add(end);
                        // 自分自身にも追加する場合
                        if (insertNewPoint && p1 != end && p0 != end)
                        {
                            Points.Insert(i, end);
                            i += 1;
                        }
                    }

                    ret.Add(subVertices);
                    subVertices = new List<RnPoint> { end };
                    len -= length;
                    //p0 = end;
                }
                if (subVertices.LastOrDefault() != p1)
                    subVertices.Add(p1);
            }

            // 最後の要素は無条件で返す
            if (ret.Count < num && subVertices.Any())
            {
                if (subVertices.Last() != Points.Last())
                    subVertices.Add(Points.Last());
                if (subVertices.Count > 1)
                    ret.Add(subVertices);
            }

            // 分割できない時は空を返す
            return ret.Select(Create).ToList();
        }

        /// <summary>
        /// 後ろの点が同じなら追加しない
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distanceEpsilon">距離誤差</param>
        /// <param name="degEpsilon">角度誤差(p0 -> p1 -> p2の角度が180±degEpsilon以内になるときp1を削除する</param>
        public void AddPointOrSkip(RnPoint p, float distanceEpsilon = 0f, float degEpsilon = 0.5f)
        {
            if (Points.Count > 0 && RnPoint.Equals(Points.Last(), p, distanceEpsilon))
                return;
            if (Points.Count > 1)
            {
                var deg = Vector3.Angle(Points[^2].Vertex - Points[^1].Vertex, p.Vertex - Points[^1].Vertex);
                if (Mathf.Abs(180f - deg) <= degEpsilon)
                    Points.RemoveAt(Points.Count - 1);
            }
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

        /// <summary>
        /// 線分の長さを計算する
        /// </summary>
        /// <returns></returns>
        public float CalcLength()
        {
            if (Count <= 1)
                return 0f;
            return GeoGraphEx.GetEdges(Points.Select(x => x.Vertex), false).Sum(e => (e.Item1 - e.Item2).magnitude);
        }
    }

    public static class RoadNetworkLineStringEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RnLineString self)
        {
            foreach (var e in GeoGraphEx.GetEdges(self.Points.Select(x => x.Vertex.Xz()), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }
    }
}