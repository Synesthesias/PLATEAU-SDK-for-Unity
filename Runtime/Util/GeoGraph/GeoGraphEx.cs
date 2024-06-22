﻿using Codice.Client.BaseCommands.Differences;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Util.GeoGraph
{

    public static class GeoGraphEx
    {


        /// <summary>
        /// 頂点verticesで構成される多角形の辺を返す. isLoop=trueの時は最後の用途と最初の要素を繋ぐ辺も返す
        /// Item1 : 始点, Item2 : 終点
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<T, T>> GetEdges<T>(IEnumerable<T> vertices, bool isLoop) where T : struct
        {
            T? first = null;
            T? current = null;
            foreach (var v in vertices)
            {
                if (current == null)
                {
                    first = current = v;
                    continue;
                }
                yield return new Tuple<T, T>(current.Value, v);
                current = v;
            }

            if (isLoop && first.HasValue)
                yield return new Tuple<T, T>(current.Value, first.Value);
        }

        /// <summary>
        /// 点群verticesをセルサイズcellSizeでグリッド化し、頂点をまとめた結果を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        public static Dictionary<Vector3, Vector3> MergeVertices(IEnumerable<Vector3> vertices, float cellSize = 0.1f)
        {
            var len = cellSize * 0.5f;
            var cells = new Dictionary<Vector3Int, HashSet<Vector3>>();
            var min = Vector3Int.one * int.MaxValue;
            foreach (var v in vertices)
            {
                var c = (v / len).ToVector3Int();
                cells.GetValueOrCreate(c).Add(v);
                min = Vector3Int.Min(min, c);
            }

            int Pow3(int x) => x * x * x;

            Vector3Int[] Delta(int d)
            {
                var w = 2 * d + 1;
                var st = Pow3(w - 2);
                var en = Pow3(w);
                var half = w / 2;
                var w2 = w * w;
                var ret = new Vector3Int[en - st];
                for (var i = st; i < en; i++)
                {
                    var dx = i % w - half;
                    var dy = (i / w) % w - half;
                    var dz = (i / w2) - half;
                    ret[i - st] = new Vector3Int(dx, dy, dz);
                }

                return ret;
            }

            // 26近傍のセルの差分
            var delta3 = Delta(1);
#if false
            void Wfs(Vector3Int c)
            {
                List<Vector3Int> neighbor = new List<Vector3Int>(delta3.Length);
                foreach (var d in delta3)
                {
                    var n = c + d;
                    if (cells.ContainsKey(n))
                        neighbor.Add(n);
                }

                // 1マスの近傍にない場合はこのセルは独立している
                if (neighbor.Any() == false)
                {
                    cells.Remove(c);
                    return;
                }

                var exist2Neighbor = neighbor.Any(n => delta3.Any(d =>
                {
                    var n2 = n + d;
                    var dc = n2 - c;
                    // 近傍がcの1近傍のものは無視
                    if (dc.x is >= -1 and <= 1 && dc.y is >= -1 and <= 1 && dc.z is >= -1 and <= 1)
                        return false;
                    return cells.ContainsKey(n2);
                }));

                // 2マス近傍にいない場合は3*3セルはすべて同じとみなす
                if (exist2Neighbor == false)
                {
                    foreach (var n in neighbor)
                    {
                        cells[c].UnionWith(cells[n]);
                        cells.Remove(n);
                    }
                }
            }
#endif
            var keys = cells.Keys.ToList();
            keys.Sort((a, b) =>
            {
                var d = a.z - b.z;
                if (d != 0)
                    return d;
                d = a.y - b.y;
                if (d != 0)
                    return d;
                return a.x - b.x;
            });
            var del = Delta(1)
                .Concat(Delta(2))
                // zでソートしているのでzが負のものは無視してよい
                .Where(d => d.z >= 0)
                // マンハッタン距離で2のものをマージする
                .Where(d => d.Abs().Sum() <= 2)
                .ToList();
            foreach (var k in keys)
            {
                if (cells.ContainsKey(k) == false)
                    continue;
                foreach (var d in del)
                {
                    var n = k + d;
                    if (cells.ContainsKey(n) == false)
                        continue;
                    cells[k].UnionWith(cells[n]);
                    cells.Remove(n);
                }
            }

            var ret = new Dictionary<Vector3, Vector3>();

            foreach (var c in cells)
            {
                var center = c.Value.Aggregate(Vector3.zero, (v, a) => v + a) / c.Value.Count;

                foreach (var v in c.Value)
                    ret[v] = center;
            }

            return ret;
        }
    }
}