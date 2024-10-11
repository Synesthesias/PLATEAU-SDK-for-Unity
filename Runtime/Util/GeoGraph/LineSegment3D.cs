﻿using System;
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

        public static LineSegment2D To2D(this LineSegment3D self, AxisPlane plane)
        {
            return new LineSegment2D(self.Start.ToVector2(plane), self.End.ToVector2(plane));
        }

        /// <summary>
        /// 点vから線分に対して最も近い点を返す
        /// </summary>
        /// <param name="self"></param>
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
        /// <param name="distanceFromStart"></param>
        /// <returns></returns>
        public static Vector3 GetNearestPoint(this LineSegment3D self, Vector3 v, out float distanceFromStart)
        {
            distanceFromStart = Vector3.Dot(self.Direction, v - self.Start);
            distanceFromStart = Mathf.Clamp(distanceFromStart, 0f, self.Magnitude);
            return self.Start + distanceFromStart * self.Direction;
        }

        /// <summary>
        /// self, otherが交差するかチェック.
        /// ただし、planeで指定された平面に射影した状態での交差判定かつ、そのうえで平面の法線方向の差分がnormalTolerance以下の場合のみ交差と判定.
        /// normalToleranceが負数の時は無限大扱い
        /// normal
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="plane"></param>
        /// <param name="normalTolerance">法線成分のねじれ許容量</param>
        /// <param name="intersection">交点</param>
        /// <param name="t1">selfから見た交点を表す正規化位置</param>
        /// <param name="t2">otherから見た交点を表す正規化位置</param>
        /// <returns></returns>
        public static bool TrySegmentIntersectionBy2D(this LineSegment3D self, LineSegment3D other, AxisPlane plane,
            float normalTolerance, out Vector3 intersection, out float t1, out float t2)
        {
            intersection = Vector3.zero;
            var self2 = self.To2D(v => v.GetTangent(plane));
            var other2 = other.To2D(v => v.GetTangent(plane));
            // 2Dに射影した状態で交差するかチェック
            if (self2.TrySegmentIntersection(other2, out var inter2, out t1, out t2) == false)
                return false;

            // 法線方向の差分が指定値より大きい時はねじれの位置で交点無し
            var v1 = self.Lerp(t1);
            var v2 = other.Lerp(t2);
            if (Mathf.Abs((v2 - v1).GetNormal(plane)) > normalTolerance && normalTolerance >= 0f)
                return false;

            intersection = (v1 + v2) * 0.5f;
            return true;
        }

        /// <summary>
        /// self, otherが交差するかチェック. ただし、planeで指定された平面に射影した状態での交差判定かつ、そのうえで平面の法線方向の差分がnormalEpsilon以下の場合のみ交差と判定
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="plane"></param>
        /// <param name="normalEpsilon"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public static bool TrySegmentIntersectionBy2D(this LineSegment3D self, LineSegment3D other, AxisPlane plane,
            float normalEpsilon, out Vector3 intersection)
        {
            return self.TrySegmentIntersectionBy2D(other, plane, normalEpsilon, out intersection, out var _, out var _);
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
        /// Vector3.Lerp(self.Start, self.End, t)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Lerp(this LineSegment3D self, float t)
        {
            return Vector3.Lerp(self.Start, self.End, t);
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