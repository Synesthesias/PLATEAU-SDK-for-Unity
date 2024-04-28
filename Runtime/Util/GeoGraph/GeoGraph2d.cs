using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    public static class GeoGraph2d
    {
        /// <summary>
        /// 凸多角形を計算して返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<Vector2> ComputeConvexVolume(IEnumerable<Vector2> vertices)
        {
            // リストの最後の辺が時計回りになっているかを確認
            bool IsLastClockwise(List<Vector2> list)
            {
                if (list.Count <= 2)
                    return true;
                return Vector2Util.Cross(list[^1] - list[^2], list[^2] - list[^3]) > 0;
            }

            var sortedVertices = vertices.OrderBy(v => v.x).ThenBy(v => v.y).ToList();
            if (sortedVertices.Count <= 2)
                return new List<Vector2>();

            // 上方の凸形状計算
            var ret = new List<Vector2> { sortedVertices[0], sortedVertices[1] };
            for (var i = 2; i < sortedVertices.Count; i++)
            {
                ret.Add(sortedVertices[i]);
                while (IsLastClockwise(ret) == false)
                    ret.RemoveAt(ret.Count - 2);
            }

            // 下方の凸形状計算
            ret.Add(sortedVertices[^2]);
            for (var i = sortedVertices.Count - 3; i >= 0; --i)
            {
                ret.Add(sortedVertices[i]);
                while (IsLastClockwise(ret) == false)
                    ret.RemoveAt(ret.Count - 2);
            }

            return ret;
        }

        /// <summary>
        /// ポリゴンを構成する頂点配列を渡すと, そのポリゴンが時計回りなのか反時計回りなのかを返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static bool IsClockwise(IEnumerable<Vector2> vertices)
        {
            var total = GeoGraphEx.GetEdges(vertices, true).Sum(item => Vector2Util.Cross(item.Item1, item.Item2));
            return total >= 0;
        }

        /// <summary>
        /// verticesで表される多角形が点pを内包するかどうか
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool Contains(IEnumerable<Vector2> vertices, Vector2 p)
        {
            // https://www.nttpc.co.jp/technology/number_algorithm.html
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

            var cnt = GeoGraphEx.GetEdges(vertices, true).Count(item => Check(item.Item1, item.Item2));
            return (cnt % 2) == 1;
        }

        /// <summary>
        /// verticesで構成された線分の長さを求める
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static float GetLineSegmentLength(IEnumerable<Vector3> vertices)
        {
            return GeoGraphEx.GetEdges(vertices, false).Sum(item => (item.Item2 - item.Item1).magnitude);
        }

        /// <summary>
        /// verticesで表される線分の中央地点を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public static bool TryGetLineSegmentMidPoint(IList<Vector3> vertices, out Vector3 midPoint)
        {
            var halfLength = GetLineSegmentLength(vertices) * 0.5f;

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

        /// <summary>
        /// 頂点verticesで構成されるポリゴン(isLoop = falseの時は開いている)と半直線rayとの交点を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="ray"></param>
        /// <param name="intersection"></param>
        /// <param name="t"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static bool PolygonHalfLineIntersection(IEnumerable<Vector2> vertices, Ray2D ray, out Vector2 intersection, out float t, bool isLoop = true)
        {
            var ret = GeoGraphEx.GetEdges(vertices, isLoop)
                .Select(p =>
                {
                    var success = LineUtil.HalfLineSegmentIntersection(ray, p.Item1, p.Item2, out Vector2 intersection,
                        out float f1,
                        out float f2);
                    return new { success, intersection, f1, f2 };
                })
                .Where(p => p.success)
                .TryFindMin(p => p.f1, out var o);

            intersection = o.intersection;
            t = o.f1;
            return ret;
        }

        /// <summary>
        /// 頂点verticesで構成されるポリゴン(isLoop = falseの時は開いている)と半直線rayとの交点を返す.
        /// ただし、y座標は無視してXz平面だけで当たり判定を行う
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="ray"></param>
        /// <param name="intersection"></param>
        /// <param name="t"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static bool PolygonHalfLineIntersectionXZ(IEnumerable<Vector3> vertices, Ray ray,
            out Vector3 intersection, out float t, bool isLoop = true)
        {
            var ret = PolygonHalfLineIntersection(vertices.Select(v => v.Xz()),
                new Ray2D(ray.origin.Xz(), ray.direction.Xz()), out Vector2 _, out float f1, isLoop);
            if (ret == false)
            {
                intersection = Vector3.zero;
                t = 0f;
            }
            else
            {
                intersection = ray.origin + ray.direction * f1;
                t = f1;
            }
            return ret;
        }


        /// <summary>
        /// 頂点verticesで構成されるポリゴン(isLoop = falseの時は開いている)と半直線rayとの交点を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="ray"></param>
        /// <param name="intersection"></param>
        /// <param name="t"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static bool PolygonSegmentIntersection(IEnumerable<Vector2> vertices, Vector2 st, Vector2 en, out Vector2 intersection, out float t, bool isLoop = true)
        {
            var ret = GeoGraphEx.GetEdges(vertices, isLoop)
                .Select(p =>
                {
                    var success = LineUtil.SegmentIntersection(st, en, p.Item1, p.Item2, out Vector2 intersection,
                        out float f1,
                        out float f2);
                    return new { success, intersection, f1, f2 };
                })
                .Where(p => p.success)
                .TryFindMin(p => p.f1, out var o);

            intersection = o.intersection;
            t = o.f1;
            return ret;
        }

        /// <summary>
        /// 頂点verticesで構成されるポリゴン(isLoop = falseの時は開いている)と半直線rayとの交点を返す.
        /// ただし、y座標は無視してXz平面だけで当たり判定を行う
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="en"></param>
        /// <param name="intersection"></param>
        /// <param name="t"></param>
        /// <param name="isLoop"></param>
        /// <param name="st"></param>
        /// <returns></returns>
        public static bool PolygonSegmentIntersectionXZ(IEnumerable<Vector3> vertices, Vector3 st, Vector3 en,
            out Vector3 intersection, out float t, bool isLoop = true)
        {
            var ret = PolygonHalfLineIntersection(vertices.Select(v => v.Xz()),
                new Ray2D(st.Xz(), en.Xz()), out Vector2 _, out float f1, isLoop);
            if (ret == false)
            {
                intersection = Vector3.zero;
                t = 0f;
            }
            else
            {
                intersection = Vector3.Lerp(st, en, f1);
                t = f1;
            }
            return ret;
        }

        public static void RemoveSelfCrossing<T>(List<T> self, Func<T, Vector2> selector, Func<T, T, T, T, Vector2, float, float, T> creater)
        {
            for (var i = 0; i < self.Count - 2; ++i)
            {
                var p1 = selector(self[i]);
                var p2 = selector(self[i + 1]);
                for (var j = i + 2; j < self.Count - 1;)
                {
                    var p3 = selector(self[j]);
                    var p4 = selector(self[j + 1]);

                    if (LineUtil.SegmentIntersection(p1, p2, p3, p4, out var intersection, out var f1, out var f2))
                    {
                        var newNode = creater(self[i], self[i + 1], self[j], self[j + 1], intersection, f1, 2);
                        self.RemoveRange(i + 1, j - i);
                        self.Insert(i + 1, newNode);
                        // もう一回最初から検索しなおす
                        j = i + 2;
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
        }

        public static Dictionary<Vector2, List<Tuple<Vector2, Vector2>>> ComputeIntersections(IEnumerable<Tuple<Vector2, Vector2>> originalSegments)
        {
            // key   : index
            // value : 線分
            var segments = originalSegments
                .Distinct()
                .Select((v, i) => new { v, i })
                .ToDictionary(x => x.i, x => x.v);

            var comparer = new Vector2Comparer();
            // key   : 端点 or 交点
            // value : keyを上端に持つ線分のリスト
            var eventQueue = new SortedDictionary<Vector2, List<int>>(comparer);
            foreach (var x in segments)
            {
                var refer = eventQueue.GetValueOrCreate(x.Value.Item1);
                refer.Add(x.Key);

                eventQueue.GetValueOrCreate(x.Value.Item2);
            }

            var lastP = Vector2.zero;

            float GetInterY(Tuple<Vector2, Vector2> a)
            {
                var p = (a.Item2.x - lastP.x) / (a.Item2.x - a.Item1.x);
                return Vector2.Lerp(a.Item1, a.Item2, p).y;
            }

            var tauComparer = Comparer<int>.Create(new Comparison<int>((x, y) => GetInterY(segments[x]).CompareTo(GetInterY(segments[y]))));
            var states = new SortedList<int, int>(tauComparer);
            var lowers = new HashSet<int>();
            var combines = new HashSet<int>();
            while (eventQueue.Count > 0)
            {
                var q = eventQueue.First();


                lastP = q.Key;
                foreach (var c in combines)
                    states.Remove(c);

                foreach (var l in lowers)
                    states.Remove(l);

                foreach (var u in q.Value)
                    states.Remove(u);

                while (true)
                {

                }
            }

        }
    }
}