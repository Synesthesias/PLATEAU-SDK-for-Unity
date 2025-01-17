using PLATEAU.Util.GeoGraph;
using System;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class RayEx
    {
        /// <summary>
        /// Ray2Dに変換する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Ray2D To2D(this Ray self, Func<Vector3, Vector2> toVec2)
        {
            return new Ray2D(toVec2(self.origin), toVec2(self.direction));
        }

        /// <summary>
        /// Ray2Dに変換する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Ray2D To2D(this Ray self, AxisPlane plane)
        {
            return new Ray2D(self.origin.ToVector2(plane), self.direction.ToVector2(plane));
        }


        /// <summary>
        /// 半直線selfと半直線otherの交点を求める.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="plane"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool CalcIntersectionBy2D(this Ray self, Ray other, AxisPlane plane, out Vector3 intersection, out float t1,
            out float t2)
        {
            var self2 = self.To2D(plane);
            var other2 = other.To2D(plane);
            var ret = self2.CalcIntersection(other2, out var inter2, out t1, out t2);
            if (ret == false)
            {
                intersection = Vector3.zero;
                return false;
            }

            float CalcY(Ray ray, out float t)
            {
                var d2 = new Vector2(ray.direction.GetTangent(plane).magnitude, ray.direction.GetNormal(plane)).normalized;
                var x = (inter2 - ray.origin.GetTangent(plane)).magnitude;
                var y = d2.y * x + ray.origin.GetNormal(plane);
                t = x * Mathf.Sqrt(1 + d2.y * d2.y);
                return y;
            }

            var selfY = CalcY(self, out t1);
            var otherY = CalcY(other, out t2);
            intersection = inter2.ToVector3(plane, (selfY + otherY) * 0.5f);
            return true;
        }
    }
}