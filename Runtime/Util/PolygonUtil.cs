using System.Collections.Generic;
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


    }
}