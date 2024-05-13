using UnityEngine;
namespace PLATEAU.Util
{
    public static class MathUtil
    {
        public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t)
        {
            return Lerp(a, b, t.x, t.y);
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float xt, float yt)
        {
            return new Vector2(Mathf.Lerp(a.x, b.x, xt), Mathf.Lerp(a.y, b.y, yt));
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Lerp(a, b, t, t);
        }

        public static double Lerp(double a, double b, float t)
        {
            // a*(1-t) * b*t だとiOSで桁落ちするので修正
            return a + (b - a) * t;
        }

        /// <summary>
        /// nを法とするLerp
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float t, float n)
        {
            a = a % n;
            b = b % n;

            if (Mathf.Abs(b - a) < n / 2)
                return Mathf.Lerp(a, b, t);

            return (a < b ? Mathf.Lerp(a + n, b, t) : Mathf.Lerp(a, b + n, t)) % n;
        }

        /// <summary> new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y)) </summary>
        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
        }

        /// <summary> new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z)) </summary>
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
        }

        /// <summary> 引数の10進数における桁数を取得 </summary>
        public static int GetDigit(int num)
        {
            return (num == 0) ? 1 : (int)Mathf.Log10(num) + 1;
        }

        /// <summary>引数の10進数における桁数を取得(long型)</summary>
        public static int GetDigit(long num)
        {
            return num == 0 ? 1 : (int)Mathf.Log10(num) + 1;
        }

        /// <summary>
        /// 正規化した(a - min) / (max - min) を0~1でClampした結果を返す
        /// </summary>
        public static float NormalizedClamp(float a, float min, float max)
        {
            return Mathf.Clamp((a - min) / (max - min), 0, 1);
        }

        /// <summary> 3引数のMathf.Min(可変長引数だとGCが発生するのでオーバーロードで定義) </summary>
        public static int Min(int a, int b, int c)
        {
            return a < b ? (a < c ? a : c) : (b < c ? b : c);
        }

        /// <summary> 3引数のMathf.Max(可変長引数だとGCが発生するのでオーバーロードで定義) </summary>
        public static int Max(int a, int b, int c)
        {
            return a > b ? (a > c ? a : c) : (b > c ? b : c);
        }

        /// <summary> 4引数のMathf.Min(可変長引数だとGCが発生するのでオーバーロードで定義) </summary>
        public static int Min(int a, int b, int c, int d)
        {
            return Mathf.Min(d, Min(a, b, c));
        }

        /// <summary> 4引数のMathf.Max(可変長引数だとGCが発生するのでオーバーロードで定義) </summary>
        public static int Max(int a, int b, int c, int d)
        {
            return Mathf.Max(d, Max(a, b, c));
        }

        /// <summary> 要素ごとのMin結果を取得 </summary>
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        /// <summary> 要素ごとのMin結果を取得 </summary>
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        /// <summary> f(0) = y0, f(0.5) = y1, f(1) = y2 を通る2次曲線. y(t) = a t^2 + b t^2 + c のa,b,cを返す </summary>
        public static void Quad(float y0, float y1, float y2, out float a, out float b, out float c)
        {
            c = y0;
            b = y1 * 4.0f - y2 - (y0 * 3f);
            a = 2 * (y0 + y2) - 4 * y1;
        }

        /// <summary> f(0) = y0, f(0.5) = y1, f(1) = y2 を通る2次曲線. y(t) = a t^2 + b t^2 + c のa,b,cを返す </summary>
        public static void Quad(Vector2 y0, Vector2 y1, Vector2 y2, out Vector2 a, out Vector2 b, out Vector2 c)
        {
            c = y0;
            b = y1 * 4.0f - y2 - (y0 * 3f);
            a = 2 * (y0 + y2) - 4 * y1;
        }

        /// <summary> f(0) = y0, f(0.5) = y1, f(1) = y2 を通る2次曲線. y(t) = a t^2 + b t^2 + c のa,b,cを返す </summary>
        public static void Quad(Vector3 y0, Vector3 y1, Vector3 y2, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            c = y0;
            b = 4 * y1 - y2 - 3 * y0;
            a = 2 * (y0 + y2) - 4 * y1;
        }


        /// <summary> f(0) = y0, f(0.5) = y1, f(1) = y2 を通る2次曲線. y(t) = a t^2 + b t^2 + cの値を返す </summary>
        public static float Quad(float y0, float y1, float y2, float t)
        {
            float a, b, c;
            Quad(y0, y1, y2, out a, out b, out c);

            return a * t * t + b * t + c;
        }

        /// <summary> f(0) = yv0, f(0.5) = v1, f(1) = v2 を通る2次曲線. y(t) = a t^2 + b t^2 + cの値を返す </summary>
        public static Vector2 Quad(Vector2 y0, Vector2 y1, Vector2 y2, float t)
        {
            Vector2 a, b, c;
            Quad(y0, y1, y2, out a, out b, out c);

            return a * t * t + b * t + c;
        }
    }
}