using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    /// <summary>
    /// 直線に関する便利関数
    /// </summary>
    public static class LineUtil
    {
        private const float Epsilon = 1e-3f;
        /// <summary>
        /// a,bを通る直線,c,dを通る直線の交点を求める
        /// 平行な場合はfalse
        /// 交わる場合 intersectionにこう
        /// </summary>
        /// <param name="a">直線1の始点</param>
        /// <param name="b">直線1の終点</param>
        /// <param name="c">直線2の始点</param>
        /// <param name="d">直線2の終点</param>
        /// <param name="intersection">交点が入る</param>
        /// <param name="t1">intersection = Vector2.Lerp(a, b, t1)となるt1が入る</param>
        /// <param name="t2">intersection = Vector2.Lerp(c, d, t2)となるt2が入る</param>
        /// <returns></returns>
        public static bool LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersection, out float t1, out float t2)
        {
            // https://qiita.com/zu_rin/items/09876d2c7ec12974bc0f
            t1 = t2 = 0f;
            intersection = Vector2.zero;

            var deno = Vector2Ex.Cross(b - a, d - c);
            if (Mathf.Abs(deno) < Epsilon)
                return false;

            t1 = Vector2Ex.Cross(c - a, d - c) / deno;
            t2 = Vector2Ex.Cross(b - a, a - c) / deno;
            intersection = Vector2.LerpUnclamped(a, b, t1);
            return true;
        }

        public static bool LineIntersection(Ray2D rayA, Ray2D rayB, out Vector2 intersection, out float t1,
            out float t2)
        {
            return LineIntersection(rayA.origin, rayA.origin + rayA.direction, rayB.origin, rayB.origin + rayB.direction,
                out intersection, out t1, out t2);
        }

        /// <summary>
        /// 半直線halfLineと線分(p1, p2)の交点を返す.
        /// 交わらない場合はfalseが返る
        /// </summary>
        /// <param name="halfLine"></param>
        /// <param name="p1">線分を構成する点1</param>
        /// <param name="p2">線分を構成する点2</param>
        /// <param name="intersection">交点</param>
        /// <param name="t1">intersection = halfLine.origin + halfLine.direction * t1となるt1が入る</param>
        /// <param name="t2">intersection = Vector2.Lerp(p1, p2, t2)となるt2が入る</param>
        /// <returns></returns>
        public static bool HalfLineSegmentIntersection(Ray2D halfLine, Vector2 p1, Vector2 p2, out Vector2 intersection, out float t1, out float t2)
        {
            var ret = LineIntersection(halfLine.origin, halfLine.origin + halfLine.direction, p1, p2, out intersection, out t1,
                out t2);
            // halfLineは半直線なので後ろになければOK
            // p1,p2は線分なので0~1の範囲内ならOK
            return ret && t1 >= 0f && t2 is >= 0f and <= 1f;
        }

        /// <summary>
        /// 直線lineと線分(p1,p2)の交点を返す
        /// 交わらない場合はfalseが返る
        /// </summary>
        /// <param name="line"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool LineSegmentIntersection(Ray2D line, Vector2 p1, Vector2 p2, out Vector2 intersection,
            out float t1, out float t2)
        {
            var ret = LineIntersection(line.origin, line.origin + line.direction, p1, p2, out intersection, out t1,
                out t2);
            // p1,p2は線分なので0~1の範囲内ならOK
            return ret && t2 is >= 0f and <= 1f;

        }

        /// <summary>
        /// 線分h(s1St, s1En)と線分(s2St, s2En)の交点を返す.
        /// 交わらない場合はfalseが返る
        /// </summary>
        /// <param name="s1St">線分1を構成する点1</param>
        /// <param name="s1En">線分1を構成する点2</param>
        /// <param name="s2St">線分2を構成する点1</param>
        /// <param name="s2En">線分2を構成する点2</param>
        /// <param name="intersection"></param>
        /// <param name="t1">intersection = halfLine.origin + halfLine.direction * t1となるt1が入る</param>
        /// <param name="t2">intersection = Vector2.Lerp(p1, p2, t2)となるt2が入る</param>
        /// <returns></returns>
        public static bool SegmentIntersection(Vector2 s1St, Vector2 s1En, Vector2 s2St, Vector2 s2En,
            out Vector2 intersection, out float t1, out float t2)
        {
            var ret = LineIntersection(s1St, s1En, s2St, s2En, out intersection, out t1,
                out t2);
            // halfLineは半直線なので後ろになければOK
            // p1,p2は線分なので0~1の範囲内ならOK
            return ret && t1 is >= 0f and <= 1f && t2 is >= 0f and <= 1f;
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
        /// 線分の距離をp : (1-p)で分割した点をmidPointに入れて返す. 戻り値は midPointを含む線分のインデックス(i ~ i+1の線分上にmidPointがある) 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="p"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public static int GetLineSegmentLerpPoint(IReadOnlyList<Vector3> vertices, float p, out Vector3 midPoint)
        {
            // 0 ~ 1の間でClampする
            p = Mathf.Clamp(p, 0, 1);

            var length = GetLineSegmentLength(vertices) * p;
            var len = 0f;
            for (var i = 0; i < vertices.Count - 1; ++i)
            {
                var p0 = vertices[i];
                var p1 = vertices[i + 1];
                var l = (p1 - p0).magnitude;
                len += l;
                if (len >= length && l > float.Epsilon)
                {
                    var f = (len - length) / l;
                    midPoint = Vector3.Lerp(p0, p1, f);
                    return i;
                }
            }

            midPoint = Vector3.zero;
            return -1;
        }

        /// <summary>
        /// verticesで表される線分の中央地点を返す.
        /// 戻り値はvertices[i] ~ vertices[i+1]に中央地点があるときのi
        /// verticesが空の時は-1が返る
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public static int GetLineSegmentMidPoint(IReadOnlyList<Vector3> vertices, out Vector3 midPoint)
        {
            return GetLineSegmentLerpPoint(vertices, 0.5f, out midPoint);
        }

        /// <summary>
        /// verticesで表される線分をsplitNumで等分する
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<List<Vector3>> SplitLineSegments(IReadOnlyList<Vector3> vertices, int num)
        {
            if (vertices.Count == 0)
                return new List<List<Vector3>>();
            var ret = new List<List<Vector3>>();
            var length = GetLineSegmentLength(vertices) / num;
            var len = 0f;
            List<Vector3> subVertices = new List<Vector3> { vertices[0] };
            for (var i = 1; i < vertices.Count; ++i)
            {
                var p0 = subVertices.Last();
                var p1 = vertices[i];
                var l = (p1 - p0).magnitude;
                len += l;
                while (len >= length && l > Epsilon)
                {
                    var f = (len - length) / l;
                    var end = Vector3.Lerp(p0, p1, f);
                    if (f >= float.Epsilon)
                        subVertices.Add(end);
                    ret.Add(subVertices);
                    len -= length;
                    p1 = end;
                }

                subVertices.Add(p1);
            }

            // 最後の要素は無条件で返す
            if (subVertices.Any())
            {
                subVertices.Add(vertices.Last());
                ret.Add(subVertices);
            }

            return ret;
        }

        /// <summary>
        /// p0,p1を通る直線に対して,aから最も近い直線状のポイントを返す
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector3 GetNearestPointWithRay(Vector3 p0, Vector3 p1, Vector3 a)
        {
            var d = (p1 - p0).normalized;
            // (a.x - (p0.x + d.x*t), a.y - (p0.y + d.y*t))・(d.x, d.y) = 0
            //   d.x*a.x - (p0.x * d.x + d.x^2*t)
            // + d.y*a.y - (p0.y * d.y + d.y^2*t)
            // = 0

            var t = Vector3.Dot(d, a) - Vector3.Dot(p0, d);
            return p0 + t * d;
        }

        /// <summary>
        /// pからselfへの最も近い点を返す. tはreturn = self.origin + self.direction * tとなるt
        /// </summary>
        /// <param name="self"></param>
        /// <param name="p"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 GetNearestPoint(this Ray2D self, Vector2 p, out float t)
        {
            var d = self.direction;
            t = Vector2.Dot(self.direction, p - self.origin);
            return self.origin + t * d;
        }

        /// <summary>
        /// pからselfへの最も近い点を返す.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector2 GetNearestPoint(this Ray2D self, Vector2 p)
        {
            return self.GetNearestPoint(p, out var _);
        }
    }
}