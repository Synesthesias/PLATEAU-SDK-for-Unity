using UnityEngine;

namespace PLATEAU.Util
{
    public class LineSegment2D
    {
        public Vector2 start;
        public Vector2 end;
    }

    public static class LineUtil
    {
        /// <summary>
        /// a,bを通る直線,c,dを通る直線の交点を求める
        /// 平行な場合はfalse
        /// 交わる場合 intersectionにこう
        /// </summary>
        /// <param name="ray1"></param>
        /// <param name="ray2"></param>
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

            var deno = Vector2Util.Cross(b - a, d - c);
            if (deno < float.Epsilon)
                return false;

            t1 = Vector2Util.Cross(c - a, d - c) / deno;
            t2 = Vector2Util.Cross(b - a, a - c) / deno;
            intersection = Vector2.Lerp(a, b, t1);
            return true;
        }

        /// <summary>
        /// 半直線halfLineと線分(p1, p2)の交点を返す.
        /// 交わらない場合はfalseが返る
        /// </summary>
        /// <param name="halfLine"></param>
        /// <param name="p1">線分を構成する点1</param>
        /// <param name="p2">線分を構成する点2</param>
        /// <param name="intersection"></param>
        /// <param name="t1">intersection = halfLine.origin + halfLine.direction * t1となるt1が入る</param>
        /// <param name="t2">intersection = Vector2.Lerp(p1, p2, t2)となるt2が入る</param>
        /// <returns></returns>
        public static bool HalfLineSegmentIntersection(Ray2D halfLine, Vector2 p1, Vector2 p2, out Vector2 intersection, out float t1, out float t2)
        {
            LineIntersection(halfLine.origin, halfLine.origin + halfLine.direction, p1, p2, out intersection, out t1,
                out t2);
            // halfLineは半直線なので後ろになければOK
            // p1,p2は線分なので0~1の範囲内ならOK
            return t1 >= 0f && t2 is >= 0f and <= 1f;
        }

        /// <summary>
        /// 半直線halfLineと線分(p1, p2)の交点を返す.
        /// 交わらない場合はfalseが返る
        /// </summary>
        /// <param name="halfLine"></param>
        /// <param name="p1">線分を構成する点1</param>
        /// <param name="p2">線分を構成する点2</param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static bool HalfLineSegmentIntersection(Ray2D halfLine, Vector2 p1, Vector2 p2, out Vector2 intersection)
        {
            return HalfLineSegmentIntersection(halfLine, p1, p2, out intersection, out var t1, out var t2);
        }
    }
}