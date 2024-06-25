using CodiceApp.EventTracking.Plastic;
using System;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    /// <summary>
    /// 線分を表すクラス
    /// </summary>
    public struct LineSegment3D
    {
        // --------------
        // start:フィールド
        // --------------
        // 始点
        private Vector3 start;
        // 終戦
        private Vector3 end;
        // 方向
        private Vector3 direction;
        // 長さ(キャッシュ)
        private float magnitude;
        // --------------
        // end:フィールド
        // --------------

        /// <summary>
        /// 始点
        /// </summary>
        public Vector3 Start
        {
            get => start;
            set
            {
                start = value;
                magnitude = (end - start).magnitude;
                direction = magnitude <= 0f ? Vector3.zero : (end - start) / magnitude;
            }
        }

        /// <summary>
        /// 終点
        /// </summary>
        public Vector3 End
        {
            get => end;
            set
            {
                end = value;
                magnitude = (end - start).magnitude;
                direction = magnitude <= 0f ? Vector3.zero : (end - start) / magnitude;
            }
        }

        /// <summary>
        /// 方向
        /// </summary>
        public Vector3 Direction => direction;

        /// <summary>
        /// 長さ
        /// </summary>
        public float Magnitude => magnitude;

        /// <summary>
        /// Rayにして返す
        /// </summary>
        public Ray Ray => new(start, direction);

        public LineSegment3D(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
            magnitude = (end - start).magnitude;
            direction = magnitude <= 0f ? Vector3.zero : (end - start) / magnitude;
        }
    }

    public static class LineSegment3DEx
    {
        public static LineSegment2D To2D(this LineSegment3D self, Func<Vector3, Vector2> toVec2)
        {
            return new LineSegment2D(toVec2(self.Start), toVec2(self.End));
        }
        /// <summary>
        /// 点vから線分に対して最も近い点を返す
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 GetNearestPoint(this LineSegment3D self, Vector3 v)
        {
            return self.GetNearestPoint(v, out var _);
        }

        /// <summary>
        /// tにはStartからの距離が入る
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 GetNearestPoint(this LineSegment3D self, Vector3 v, out float t)
        {
            t = Vector3.Dot(self.Direction, v - self.Start);
            t = Mathf.Clamp(t, 0f, self.Magnitude);
            return self.Start + t * self.Direction;

        }

        /// <summary>
        /// self.Start + distance * self.Direction
        /// </summary>
        /// <param name="self"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetPoint(this LineSegment3D self, float distance)
        {
            return self.Start + self.Direction * distance;
        }

        /// <summary>
        /// Start/End反転させたものを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static LineSegment3D Reversed(this LineSegment3D self)
        {
            return new LineSegment3D(self.End, self.Start);
        }
    }
}