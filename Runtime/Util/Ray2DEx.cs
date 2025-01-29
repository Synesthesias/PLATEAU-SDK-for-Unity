using PLATEAU.Util.GeoGraph;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class Ray2DEx
    {
        public static string ToLogString(this Ray2D self)
        {
            return $"origin: {self.origin}, direction: {self.direction}";
        }

        /// <summary>
        /// 半直線selfと半直線otherの交点を求める.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool CalcIntersection(this Ray2D self, Ray2D other, out Vector2 intersection, out float t1,
            out float t2)
        {
            var ret = LineUtil.LineIntersection(self, other, out intersection, out t1, out t2);
            return ret && t1 >= 0f && t2 >= 0f;
        }

        /// <summary>
        /// pointがselfの左側にあるかどうか
        /// </summary>
        /// <param name="self"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsPointOnLeftSide(this Ray2D self, Vector2 point)
        {
            return Vector2Ex.Cross(self.direction, point - self.origin) > 0f;
        }
    }
}