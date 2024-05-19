using System;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    /// <summary>
    /// 線分を表すクラス
    /// </summary>
    public struct LineSegment2D
    {
        private Vector2 start;
        private Vector2 end;

        private Vector2 direction;

        private float magnitude;

        /// <summary>
        /// 始点
        /// </summary>
        public Vector2 Start
        {
            get => start;
            set
            {
                start = value;
                magnitude = (end - start).magnitude;
                direction = (end - start) / magnitude;
            }
        }

        /// <summary>
        /// 終点
        /// </summary>
        public Vector2 End
        {
            get => end;
            set
            {
                end = value;
                magnitude = (end - start).magnitude;
                direction = (end - start) / magnitude;
            }
        }

        /// <summary>
        /// 方向
        /// </summary>
        public Vector2 Direction => direction;

        public float Magnitude => magnitude;

        public Ray2D Ray => new(start, direction);

        public LineSegment2D(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            magnitude = (end - start).magnitude;
            direction = (end - start) / magnitude;
        }

        /// <summary>
        /// 引数の線分(v0,v1)との交点を返す
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public bool TrySegmentIntersection(Vector2 v0, Vector2 v1, out Vector2 intersection, out float t1, out float t2)
        {
            return LineUtil.SegmentIntersection(Start, End, v0, v1, out intersection, out t1, out t2);
        }

        /// <summary>
        /// 引数の半直線(origin,dir)との交点を返す
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public bool TryHalfLineIntersection(Vector2 origin, Vector2 dir, out Vector2 intersection, out float t1,
            out float t2)
        {
            return LineUtil.HalfLineSegmentIntersection(new Ray2D(origin, dir), Start, End, out intersection, out t1,
                out t2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="intersection"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public bool TryLineIntersection(Vector2 origin, Vector2 dir, out Vector2 intersection, out float t1,
            out float t2)
        {
            return LineUtil.LineSegmentIntersection(new Ray2D(origin, dir), Start, End, out intersection, out t1, out t2);
        }

        /// <summary>
        /// 点vから線分に対して最も近い点を返す
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector2 GetNearestPoint(Vector2 v)
        {
            var t = Vector3.Dot(Direction, v) - Vector3.Dot(Start, v);
            if (t < 0)
                return Start;
            if (t > 1)
                return End;
            return Start + t * Direction;
        }
    }
}