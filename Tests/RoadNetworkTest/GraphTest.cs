using NUnit.Framework;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEngine;

namespace Tests.RoadNetworkTest
{
    public class GraphTest
    {
        private static void CheckLerpPointInLine(int index, Ray2D a, Ray2D b, float p)
        {
            var res = GeoGraph2D.CalcLerpPointInLine(a, b, p, out var v);
            // そもそも解無しの場合は無視(a,bが並行で p=1.0の場合等)
            if (!res)
                return;
            var t = (v - a.origin).magnitude;
            var near = b.GetNearestPoint(v);
            var bLen = (v - near).magnitude;

            // t : bLen = p : (1-p)になるか確認
            var lhs = Mathf.Abs(t * (1 - p));
            var rhs = Mathf.Abs(bLen * p);
            var del = Mathf.Abs(lhs - rhs);
            var label = $"[{index}] {a.ToLogString()}/{b.ToLogString()}[{p}].  |{lhs}| != {rhs}({p} & {bLen})";
            Assert.IsTrue(del < 1e-1f, label);
        }

        [Test()]
        public void CalcLerpPointInLineTest()
        {
            var triCount = 1000;
            var posRange = 1000f;
            var pCount = 100;

            float RandPos()
            {
                return Random.Range(-posRange, posRange);
            }
            Ray2D CreateRandomRay()
            {
                var dir = Vector2Ex.Polar2Cart(Random.Range(0, 360));
                return new Ray2D(new Vector2(RandPos(), RandPos()), dir);
            }

            foreach (var i in Enumerable.Range(0, triCount))
            {
                var a = CreateRandomRay();
                var b = CreateRandomRay();
                foreach (var p in Enumerable.Range(0, pCount + 1).Select(x => 1f * x / pCount))
                {
                    GeoGraph2D.CalcLerpPointInLine(a, b, p, out var v);
                    CheckLerpPointInLine(i, a, b, p);
                }
            }

            CheckLerpPointInLine(
                -1
                , new Ray2D(new Vector2(457.00f, 244.00f), new Vector2(-0.39f, -0.92f))
                , new Ray2D(new Vector2(-579f, -137f), new Vector2(-0.45f, -0.89f))
                , 0f
            );
            CheckLerpPointInLine(
                -1
                , new Ray2D(new Vector2(457.00f, 244.00f), new Vector2(-0.39f, -0.92f))
                , new Ray2D(new Vector2(-579f, -137f), new Vector2(-0.45f, -0.89f))
                , 1f
                );
        }
    }
}