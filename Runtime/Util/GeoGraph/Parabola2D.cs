using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{
    public struct Parabola2D
    {
        // 放物線の原点
        public Vector2 Origin { get; set; }

        // 放物線の係数
        public float A { get; set; }

        public Parabola2D(Vector2 origin, float a)
        {
            Origin = origin;
            A = a;
        }

        public float GetY(float x)
        {
            return A * Mathf.Pow((x - Origin.x), 2) + Origin.y;
        }

        public static Parabola2D Create(Vector2 origin, Vector2 p0)
        {
            var d = p0 - origin;
            return new Parabola2D(origin, d.y / (d.x * d.x));
        }
    }
}