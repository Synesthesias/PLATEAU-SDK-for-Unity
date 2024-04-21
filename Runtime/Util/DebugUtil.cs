using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class DebugUtil
    {
        /// <summary>
        /// start -> endの方向に矢印を描画する
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawArrow(
            Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            Vector3 up = arrowUp ?? Vector3.up;
            Debug.DrawLine(start, end, color ?? Color.white, duration, depthTest);
            up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;

            var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
            var a2 = Quaternion.AngleAxis(-90, up) * a1;

            a1 = a1.normalized;
            a2 = a2.normalized;
            Debug.DrawLine(end + a1 * arrowSize, end);
            Debug.DrawLine(end + a2 * arrowSize, end);
        }

        /// <summary>
        /// verticesの要素を結ぶように矢印描画を行う
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isLoop">要素の最後 -> 最初の頂点も矢印で結ぶ</param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawArrows(
            IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            Vector3? first = null;
            Vector3? current = null;
            foreach (var v in vertices)
            {
                if (first == null)
                {
                    first = current = v;
                    continue;
                }

                DrawArrow(current.Value, v, arrowSize, arrowUp, color, duration, depthTest);
                current = v;
            }

            if (isLoop && first.HasValue)
            {
                DrawArrow(current.Value, first.Value, arrowSize, arrowUp, color, duration, depthTest);
            }
        }



        public static void DrawCenters(IEnumerable<Vector3> vertices
            , bool isLoop = false
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? color = null
            , float duration = 0f
            , bool depthTest = true)
        {
            var polygon = vertices.ToList();

            for (var i = 0; i < polygon.Count; i++)
            {
                var v1 = polygon[i];
                var v2 = polygon[(i + 1) % polygon.Count];

                var dir = v2 - v1;

            }
        }
    }
}