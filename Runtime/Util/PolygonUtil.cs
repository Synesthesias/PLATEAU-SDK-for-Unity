using PLATEAU.CityGML;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class PolygonUtil
    {
        /// <summary>
        /// ポリゴンを構成する頂点配列を渡すと, そのポリゴンが時計回りなのか反時計回りなのかを返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static bool IsClockwise(IEnumerable<Vector2> vertices)
        {
            var total = 0f;
            Vector2? first = null;
            Vector2? current = null;
            foreach (var v in vertices)
            {
                if (current == null)
                {
                    first = v;
                    current = v;
                    continue;
                }

                total += Vector2Util.Cross(v, current.Value);
                current = v;
            }

            if (first != null)
            {
                total += Vector2Util.Cross(first.Value, current.Value);
            }

            return total < 0;
        }

        public static bool Contains(IEnumerable<Vector2> vertices, Vector2 p)
        {
            Vector2? first = null;
            Vector2? current = null;
            bool? isClockwise = null;
            var cn = 0;

            bool Check(Vector2 c, Vector2 v)
            {

                // 上向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、終点は含まない。(ルール1)
                // 下向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、始点は含まない。(ルール2)
                if (((c.y <= p.y) && (v.y > p.y)) || ((c.y > p.y) && (v.y <= p.y)))
                {
                    // ルール1,ルール2を確認することで、ルール3も確認できている。
                    // 辺は点pよりも右側にある。ただし、重ならない。(ルール4)
                    // 辺が点pと同じ高さになる位置を特定し、その時のxの値と点pのxの値を比較する。
                    var vt = (p.y - c.y) / (v.y - c.y);
                    if (p.x < (c.x + (vt * (v.x - c.x))))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var v in vertices)
            {
                if (current == null)
                {
                    first = v;
                    current = v;
                    continue;
                }

                if (Check(current.Value, v))
                    cn++;

                current = v;
            }

            if (first != null)
            {
                if (Check(current.Value, first.Value))
                    cn++;
            }
            return (cn % 2) == 1;
        }

        // 線分の交差判定
        private static bool IsCrossing(Vector2 startPoint1, Vector2 endPoint1, Vector2 startPoint2, Vector2 endPoint2)
        {
            // ベクトルP1Q1
            var vector1 = endPoint1 - startPoint1;
            // ベクトルP2Q2
            var vector2 = endPoint2 - startPoint2;
            //
            // 以下条件をすべて満たすときが交差となる
            //
            //    P1Q1 x P1P2 と P1Q1 x P1Q2 が異符号
            //                かつ
            //    P2Q2 x P2P1 と P2Q2 x P2Q1 が異符号
            //
            return Vector2Util.Cross(vector1, startPoint2 - startPoint1) * Vector2Util.Cross(vector1, endPoint2 - startPoint1) < 0 &&
                   Vector2Util.Cross(vector2, startPoint1 - startPoint2) * Vector2Util.Cross(vector2, endPoint1 - startPoint2) < 0;
        }
    }
}