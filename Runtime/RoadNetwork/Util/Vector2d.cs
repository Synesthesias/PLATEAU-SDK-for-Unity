using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Util
{
    /// <summary>
    /// floatの精度が足りないのでdoubleで計算する
    /// </summary>
    public struct Vector2d
    {
        public double x;

        public double y;

        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2d(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        /// <summary>
        /// ベクトル長さ(Unity.Vector2に名前合わせる)
        /// </summary>
        public double magnitude => Math.Sqrt(x * x + y * y);

        /// <summary>
        /// 正規化されたベクトル(Unity.Vector2に名前合わせる)
        /// </summary>
        public Vector2d normalized
        {
            get
            {
                var m = magnitude;
                return new Vector2d(x / m, y / m);
            }
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {

            return new Vector2d(a.x - b.x, a.y - b.y);
        }

        public static Vector2d operator *(Vector2d a, double x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator *(Vector2d a, int x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator *(Vector2d a, float x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator -(Vector2d a)
        {
            return new Vector2d(-a.x, -a.y);
        }

        public static bool operator ==(Vector2d a, Vector2d b)
        {
            return a.x.Equals(b.x) && a.y.Equals(b.y);
        }

        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return !(a == b);
        }

        public bool Equals(Vector2d other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2d other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public Vector2 ToVector2() => new Vector2((float)x, (float)y);

        /// <summary>
        /// Vector2の外積. ret = a.x * b.y - a.y * b.x
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Cross(Vector2d a, Vector2d b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Vector2d LerpUnclamped(Vector2d a, Vector2d b, double t) => new Vector2d(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }

}