using System;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class RayEx
    {
        public static Ray2D To2D(this Ray self, Func<Vector3, Vector2> toVec2)
        {
            return new Ray2D(toVec2(self.origin), toVec2(self.direction));
        }
    }
}