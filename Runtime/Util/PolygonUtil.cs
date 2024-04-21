using PLATEAU.CityGML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class PolygonUtil
    {
        /// <summary>
        /// ポリゴンを構成する頂点配列を渡すと, そのポリゴンが時計回りなのか反時計回りなのかを返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static bool IsClockwise(IEnumerable<Vector2> vertices)
        {
            var total = 0f;
            Vector2? first = null;
            Vector2? current = null;
            foreach (var v in vertices)
            {
                if (current == null)
                {
                    first = v;
                    current = v;
                    continue;
                }

                total += Vector2Util.Cross(v, current.Value);
                current = v;
            }

            if (first != null)
            {
                total += Vector2Util.Cross(first.Value, current.Value);
            }

            return total < 0;
        }

        public static bool Contains(IEnumerable<Vector2> vertices, Vector2 p)
        {
            Vector2? first = null;
            Vector2? current = null;
            bool? isClockwise = null;
            var cn = 0;

            bool Check(Vector2 c, Vector2 v)
            {

                // 上向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、終点は含まない。(ルール1)
                // 下向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、始点は含まない。(ルール2)
                if (((c.y <= p.y) && (v.y > p.y)) || ((c.y > p.y) && (v.y <= p.y)))
                {
                    // ルール1,ルール2を確認することで、ルール3も確認できている。
                    // 辺は点pよりも右側にある。ただし、重ならない。(ルール4)
                    // 辺が点pと同じ高さになる位置を特定し、その時のxの値と点pのxの値を比較する。
                    var vt = (p.y - c.y) / (v.y - c.y);
                    if (p.x < (c.x + (vt * (v.x - c.x))))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var v in vertices)
            {
                if (current == null)
                {
                    first = v;
                    current = v;
                    continue;
                }

                if (Check(current.Value, v))
                    cn++;

                current = v;
            }

            if (first != null)
            {
                if (Check(current.Value, first.Value))
                    cn++;
            }
            return (cn % 2) == 1;
        }

        /// <summary>
        /// verticesで構成された線分の長さを求める
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static float GetLineSegmentLength(IEnumerable<Vector3> vertices)
        {
            var ret = vertices.Aggregate(new Tuple<float, Vector3>(-1, Vector3.zero), (a, v) =>
            {
                if (a.Item1 < 0)
                    return new Tuple<float, Vector3>(0f, v);
                return new Tuple<float, Vector3>(a.Item1 + (a.Item2 - v).magnitude, v);
            });
            return Math.Max(0f, ret.Item1);
        }

        /// <summary>
        /// verticesで表される線分の中央地点を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public static bool TryGetLineSegmentMidPoint(IList<Vector3> vertices, out Vector3 midPoint)
        {
            var halfLength = PolygonUtil.GetLineSegmentLength(vertices) * 0.5f;

            var len = 0f;
            for (var i = 0; i < vertices.Count - 1; ++i)
            {
                var p0 = vertices[i];
                var p1 = vertices[i + 1];
                var l = (p1 - p0).magnitude;
                len += l;
                if (len >= halfLength && l > float.Epsilon)
                {
                    var f = (len - halfLength) / l;
                    midPoint = Vector3.Lerp(p0, p1, f);
                    return true;
                }
            }

            midPoint = Vector3.zero;
            return false;
        }
    }
}