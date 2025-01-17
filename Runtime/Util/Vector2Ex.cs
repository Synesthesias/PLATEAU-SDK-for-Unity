using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class Vector2Ex
    {
        /// <summary>
		/// new Vector2(Mathf.Abs(self.x), Mathf.Abs(self.y))
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static Vector2 Abs(this Vector2 self)
        {
            self.x = Mathf.Abs(self.x);
            self.y = Mathf.Abs(self.y);
            return self;
        }

        /// <summary> 各要素をmin, maxでクランプ </summary>
        public static Vector2 Clamp(this Vector2 x, float min, float max)
        {
            return new Vector2(Mathf.Clamp(x.x, min, max), Mathf.Clamp(x.y, min, max));
        }

        /// <summary> 各要素をmin,maxの各要素でクランプ </summary>
        public static Vector2 Clamp(this Vector2 x, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(x.x, min.x, max.x), Mathf.Clamp(x.y, min.y, max.y));
        }

        /// <summary> Vector2.Angleの360°版. fromをtoに回転させるために必要か角度を返す(-180 ~ 180)(反時計回りが正) </summary>
        public static float Angle360(Vector2 from, Vector2 to)
        {
            return Vector2.Angle(from, to) * Mathf.Sign(Cross(from, to));
        }

        /// <summary>
        /// Vector2の外積. ret = a.x * b.y - a.y * b.x
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// from -> toに最大maxRad回転させる
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxRad"></param>
        /// <returns></returns>
        public static Vector2 RotateTo(Vector2 from, Vector2 to, float maxRad)
        {
            var angle = Vector2.Angle(from, to);
            var maxDeg = Mathf.Rad2Deg * maxRad;
            if (angle < maxDeg)
                return to;

            var cross = from.x * to.y - from.y * to.x;

            var rad = cross > 0 ? maxRad : -maxRad;
            var s = Mathf.Sin(rad);
            var c = Mathf.Cos(rad);

            return new Vector2(from.x * c - from.y * s, from.x * s + from.y * c);
        }

        /// <summary> 時計回りに回転した結果を返す </summary>
        public static Vector2 Rotate(this Vector2 self, float degree)
        {
            var rad = Mathf.Deg2Rad * degree;
            var si = Mathf.Sin(rad);
            var co = Mathf.Cos(rad);

            var x = self.x * co - self.y * si;
            var y = self.y * co + self.x * si;

            return new Vector2(x, y);
        }

        /// <summary> xを差し替えたVector2返す。非破壊 </summary>
        public static Vector2 PutX(this Vector2 self, float x)
        {
            self.x = x;
            return self;
        }

        /// <summary> yを差し替えたVector2返す。非破壊 </summary>
        public static Vector2 PutY(this Vector2 self, float y)
        {
            self.y = y;
            return self;
        }

        /// <summary>
        /// yを指定してVector3にする
        /// return new Vector3(self.x, y, self.y);
        /// </summary>
        /// <param name="self"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Xay(this Vector2 self, float y = 0f)
        {
            return new Vector3(self.x, y, self.y);
        }

        /// <summary>
        /// zを指定してVector3にする
        /// return new Vector3(self.x, self.y, z);
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 Xya(this Vector2 self, float z = 0f)
        {
            return new Vector3(self.x, self.y, z);
        }

        /// <summary>
        /// Mathf.Min(self.x, self.y)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Min(this Vector2 self)
        {
            return Mathf.Min(self.x, self.y);
        }

        /// <summary>
        /// Mathf.Min(self.x, self.y)
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Max(this Vector2 self)
        {
            return Mathf.Max(self.x, self.y);
        }

        /// <summary>
        /// self.x + self.y
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Sum(this Vector2 self)
        {
            return self.x + self.y;
        }

        /// <summary>
        /// (self.x + self.y) / 2
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Ave(this Vector2 self)
        {
            return (self.x + self.y) / 2;
        }

        /// <summary>
        /// self.y / self.magnitude
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Sin(this Vector2 self)
        {
            return self.y / self.magnitude;
        }

        /// <summary>
        /// self.x / self.magnitude
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Cos(this Vector2 self)
        {
            return self.x / self.magnitude;
        }

        /// <summary>
        /// self.y / self.x
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float Tan(this Vector2 self)
        {
            return self.y / self.x;
        }

        /// <summary>
        /// new Vector2Int(self.x * mul.x, self.y * mul.y)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="mul"></param>
        /// <returns></returns>
        public static Vector2 Scaled(this Vector2 self, Vector2 mul)
        {
            self.x *= mul.x;
            self.y *= mul.y;
            return self;
        }

        /// <summary>
        /// new Vector2Int(self.x / div.x, self.y / div.y)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static Vector2 RevScaled(this Vector2 self, Vector2 div)
        {
            self.x /= div.x;
            self.y /= div.y;
            return self;
        }

        /// <summary> Vector2.Scaleの逆数版 a / b </summary>
        public static Vector2 RevScale(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        /// <summary> 角度からVector2を生成. return new Vector2(Cosθ, Sinθ)を返す </summary>
        public static Vector2 Deg2Vector(float degree)
        {
            return Rad2Vector(degree * Mathf.Deg2Rad);
        }

        /// <summary> 角度からVector2を生成. return new Vector2(Cosθ, Sinθ)を返す </summary>
        public static Vector2 Rad2Vector(float rad)
        {
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        /// <summary>
        /// 極座標の角度から直交座標系の値  new Vector2(Cos(degree * Deg2Rad) , Sin(degree * Deg2Rad)) を返す.
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static Vector2 Polar2Cart(float degree, float radius = 1.0f)
        {
            var rad = Mathf.Deg2Rad * degree;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        public static bool IsNaNOrInfinity(this Vector3 self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsNaN(self.z) || float.IsInfinity(self.x) || float.IsInfinity(self.y) || float.IsInfinity(self.z);
        }

        public static bool IsNaNOrInfinity(this Vector2 self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsInfinity(self.x) || float.IsInfinity(self.y);
        }


        public static Vector2Int ToVector2Int(this Vector2 self)
        {
            return new Vector2Int((int)self.x, (int)self.y);
        }

        /// <summary>
        /// 各成分に対してCeilToIntを適用してVector2Int型で返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2Int CeilToInt(this Vector2 self)
        {
            return new Vector2Int(Mathf.CeilToInt(self.x), Mathf.CeilToInt(self.y));
        }

        /// <summary>
        /// 各成分に対してFloorToIntを適用してVector2Int型で返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2Int FloorToInt(this Vector2 self)
        {
            return new Vector2Int(Mathf.FloorToInt(self.x), Mathf.FloorToInt(self.y));
        }

        /// <summary>
        /// 幾何中心を求める
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2 Centroid(IEnumerable<Vector2> points)
        {
            var sum = Vector2.zero;
            var n = 0;
            foreach (var p in points)
            {
                sum += p;
                n++;
            }

            if (n == 0)
                return sum;
            return sum / n;
        }
    }
}