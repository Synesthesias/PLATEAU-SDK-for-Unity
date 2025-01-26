using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Util
{
    /// <summary>
    /// 線が自分自身と交差している場合、交差して輪になった部分をなかったことにして、交差しない線に修正します。
    /// 交差は2次元(x,z)で判定します。
    /// </summary>
    public class LineIntersectionRemover
    {
        public List<Vector3> Calc(IEnumerable<Vector3> lineArg)
        {
            var srcLine = lineArg.ToList();
            
            // 点の数が2以下なら交差しようがない
            if (srcLine.Count < 2) return srcLine;

            // 最初の線分を追加
            var dstLine = new List<Vector3> { srcLine[0], srcLine[1] };

            // 点を追加しながら、既存線分と交差していないかチェック
            for (int srcI = 2; srcI < srcLine.Count; srcI++)
            {
                var p1 = dstLine[^2];
                var p2 = dstLine[^1];
                
                // いったん追加
                dstLine.Add(srcLine[srcI]);
                
                
                // 今までの線分と交差が見つかったら輪になった部分を切り取る
                for (int dstI = 0; dstI < dstLine.Count - 3; dstI++)
                {
                    var p3 = dstLine[dstI];
                    var p4 = dstLine[dstI + 1];
                    if (DoIntersect(p1, p2, p3, p4, out Vector3 intersection))
                    {
                        // 交差が見つかったので、ループ部分を削除する。
                        // 具体的にはdstI+1から最後までを削除し、交差点を挿入する。
                        dstLine.RemoveRange(dstI+1, dstLine.Count - (dstI + 1));
                        
                        // 交差点を追加して最後の点とする
                        dstLine.Add(intersection);
                        break; // 切り取り完了
                    }
                }
            }

            return dstLine;
        }

        /// <summary>
        /// 線分p1-p2と線分p3-p4が交差する場合にtrueを返し、交差座標を返します。交差は2次元(x,z)で判定します。
        /// </summary>
        private bool DoIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
        {
            intersection = Vector2.zero;
            
            // ChatGPT o1に感謝
            // 線分をパラメータ形式で表したときの係数を求める
            // (p1 + t*(p2-p1)) と (p3 + u*(p4-p3)) が同じ点になる t,u を求める
            
            // 並行判定。denomが0なら並行
            float denom = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);
            if (Mathf.Abs(denom) < 1e-5f) return false;
            
            float t = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denom;
            float u = ((p2.x - p1.x) * (p1.z - p3.z) - (p2.z - p1.z) * (p1.x - p3.x)) / denom;
            
            // t,uが両方[0,1]の範囲にあれば、線分同士が交差
            if(t>0f && t<1f && u>0f && u<1f)
            {
                intersection = (p1 + t * (p2 - p1));
                return true;
            }

            return false;
        }
    }
}