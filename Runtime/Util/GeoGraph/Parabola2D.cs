using UnityEngine;
using static UnityEngine.UI.Image;

namespace PLATEAU.Util.GeoGraph
{
    /// <summary>
    /// Originを通り傾きAの放物線
    /// </summary>
    public struct Parabola2D
    {
        // 放物線の原点
        public Vector2 Origin { get; set; }

        // 回転[deg]
        public float Rotation { get; set; }

        // 放物線の係数
        public float A { get; set; }

        public Parabola2D(Vector2 origin, float a, float rotation = 0)
        {
            Origin = origin;
            Rotation = rotation;
            A = a;
        }

        /// <summary>
        /// localX -> 放物線の原点からの横軸の相対距離
        /// </summary>
        /// <param name="localX"></param>
        /// <returns></returns>
        public Vector2 GetPoint(float localX)
        {
            var p = new Vector2(localX, A * Mathf.Pow(localX, 2));
            return p.Rotate(Rotation) + Origin;
        }

        public static Parabola2D Create(Vector2 origin, Vector2 p0, float rotation = 0)
        {
            var d = p0 - origin;
            var ld = d.Rotate(-rotation);
            return new Parabola2D(origin, ld.y / (ld.x * ld.x), rotation);
        }
    }

    public struct ParabolaSegment2D
    {
        public Parabola2D Parabola { get; set; }

        public Vector2 Min => GetPoint(MinT);

        public Vector2 Max => GetPoint(MaxT);

        public float MinT { get; set; }

        public float MaxT { get; set; }

        public Vector2 GetPoint(float t)
        {
            return Parabola.GetPoint(t);
        }

        public ParabolaSegment2D(Parabola2D parabola, float minT, float maxT)
        {
            this.Parabola = parabola;
            this.MinT = minT;
            this.MaxT = maxT;
        }

        public static ParabolaSegment2D Create(Vector2 origin, Vector2 p0, Vector2? up = null)
        {
            var axisY = up ?? Vector2.up;
            var rot = Vector2.SignedAngle(Vector2.up, axisY);
            var parabola = Parabola2D.Create(origin, p0, rot);
            var d = (p0 - origin).Rotate(-rot);
            return new ParabolaSegment2D(parabola, minT: 0, maxT: d.x);
        }
    }
}