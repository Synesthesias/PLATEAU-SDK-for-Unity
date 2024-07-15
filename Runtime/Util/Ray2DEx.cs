using UnityEngine;

namespace PLATEAU.Util
{
    public static class Ray2DEx
    {
        public static string ToLogString(this Ray2D self)
        {
            return $"origin: {self.origin}, direction: {self.direction}";
        }
    }
}