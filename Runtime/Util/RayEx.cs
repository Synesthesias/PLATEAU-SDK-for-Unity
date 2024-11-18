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
    }
}