using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.Util.GeoGraph
{
    /// <summary>
    /// 3D空間上の平面タイプ
    /// </summary>
    public enum AxisPlane
    {
        // XY平面
        Xy,

        // XZ平面
        Xz,

        // YZ平面
        Yz,
    }

    public static class AxisPlaneEx
    {
        /// <summary>
        /// planeの平面成分を取り出す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector2 GetTangent(this Vector3 self, AxisPlane plane)
        {
            return plane switch
            {
                AxisPlane.Xy => self.Xy(),
                AxisPlane.Xz => self.Xz(),
                AxisPlane.Yz => self.Yz(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        /// <summary>
        /// planeの平面成分を取り出す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector2 ToVector2(this Vector3 self, AxisPlane plane)
        {
            return self.GetTangent(plane);
        }

        /// <summary>
        /// planeに指定した成分にselfを入れて、残りにaを入れたvector3を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector3 ToVector3(this Vector2 self, AxisPlane plane, float a = 0f)
        {
            return plane switch
            {
                AxisPlane.Xy => new Vector3(self.x, self.y, a),
                AxisPlane.Xz => new Vector3(self.x, a, self.y),
                AxisPlane.Yz => new Vector3(a, self.x, self.y),
                _ => throw new ArgumentOutOfRangeException(nameof(plane), plane, null)
            };
        }

        /// <summary>
        /// planeに対する直交成分を取り出す
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="self"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static float GetNormal(this Vector3 self, AxisPlane plane)
        {
            return plane switch
            {
                AxisPlane.Xy => self.z,
                AxisPlane.Xz => self.y,
                AxisPlane.Yz => self.x,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        /// <summary>
        /// planeに対する直交成分をnに変更する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector3 PutNormal(this Vector3 self, AxisPlane plane, float n)
        {
            return plane switch
            {
                AxisPlane.Xy => self.PutZ(n),
                AxisPlane.Xz => self.PutY(n),
                AxisPlane.Yz => self.PutX(n),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// selfのplane成分をaに変更したVector3を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="plane"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector3 Put(this Vector3 self, AxisPlane plane, Vector2 a)
        {
            return plane switch
            {
                AxisPlane.Xy => self.PutXy(a),
                AxisPlane.Xz => self.PutXz(a.x, a.y),
                AxisPlane.Yz => self.PutYz(a.x, a.y),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        /// <summary>
        /// planeの平行成分をv, 直交成分をnormalに変更したVector3を返す
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="v"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector3 Make(this AxisPlane plane, Vector2 v, float normal)
        {
            return Vector3.zero.Put(plane, v).PutNormal(plane, normal);
        }

        /// <summary>
        /// planeに直交する軸の単位ベクトルを返す
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Vector3 NormalVector(this AxisPlane plane)
        {
            return plane.Make(Vector2.zero, 1f);
        }
    }

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
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="mergeMap"></param>
        /// <param name="newVertices"></param>
        /// <param name="newVertexIndexMap"></param>
        /// <returns>頂点のマージ処理が走ったかどうか</returns>
        public static bool MergeMeshVertex(
            IList<Vector3> vertices,
            Dictionary<Vector3, Vector3> mergeMap,
            out List<Vector3> newVertices,
            // verticesインデックス -> newVerticesのインデックス
            out List<int> newVertexIndexMap)
        {
            newVertexIndexMap = Enumerable.Range(0, vertices.Count).ToList();
            newVertices = new List<Vector3>(vertices.Count);
            // 頂点 -> インデックス変換
            var indexMap = new Dictionary<Vector3, int>();
            bool found = false;
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                // 頂点の変換があるか確認する
                if (mergeMap.TryGetValue(v, out var afterVertex))
                {
                    v = afterVertex;
                    found = true;
                }

                // #NOTE : 重複頂点の削除
                var idx = 0;
                if (indexMap.TryGetValue(v, out idx) == false)
                {
                    // 新規の場合はnewVerticesに追加しインデックスマップに登録
                    idx = newVertices.Count;
                    newVertices.Add(v);
                    indexMap[v] = idx;
                    found = true;
                }

                newVertexIndexMap[i] = idx;
            }


            return found;
        }

        /// <summary>
        /// 3次元のセル空間における, d近傍の距離を返す.
        /// GetNeighborDistance3D(1)の場合は, 3*3*3の立方体の距離を返す( x,yzがそれぞれ -1~1の範囲)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector3Int[] GetNeighborDistance3D(int d)
        {
            // +d, -dの範囲で2d+1の立方体を返す
            var w = 2 * d + 1;
            var size = w * w * w;
            var half = d;
            var w2 = w * w;
            var ret = new Vector3Int[size];
            for (var i = 0; i < size; i++)
            {
                var dx = i % w - half;
                var dy = (i / w) % w - half;
                var dz = (i / w2) - half;
                ret[i] = new Vector3Int(dx, dy, dz);
            }

            return ret;
        }

        /// <summary>
        /// 2次元のセル空間における, d近傍の距離を返す.
        /// GetNeighborDistance3D(1)の場合は, 3*3の正方形の距離を返す( x,yzがそれぞれ -1~1の範囲)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Vector2Int[] GetNeighborDistance2D(int d)
        {
            // +d, -dの範囲で2d+1の立方体を返す
            var w = 2 * d + 1;
            var size = w * w;
            var half = d;
            var ret = new Vector2Int[size];
            for (var i = 0; i < size; i++)
            {
                var dx = i % w - half;
                var dy = (i / w) % w - half;
                ret[i] = new Vector2Int(dx, dy);
            }

            return ret;
        }

        /// <summary>
        /// 点群verticesをセルサイズcellSizeでグリッド化し、頂点をまとめた結果を返す
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="cellSize"></param>
        /// <param name="mergeCellLength"></param>
        /// <returns></returns>
        public static Dictionary<Vector3, Vector3> MergeVertices(IEnumerable<Vector3> vertices, float cellSize = 0.1f,
            int mergeCellLength = 2)
        {
            var len = cellSize;
            var mergeLen = mergeCellLength;

            // HashSetだと全く同じ値が来たときに消えないのでListにする
            var cells = new Dictionary<Vector3Int, List<Vector3>>();
            var min = Vector3Int.one * int.MaxValue;
            foreach (var v in vertices)
            {
                var c = (v / len).ToVector3Int();
                cells.GetValueOrCreate(c).Add(v);
                min = Vector3Int.Min(min, c);
            }

            // z,y,xの順でソート
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

            var del1 = GetNeighborDistance3D(1);
            foreach (var k in keys)
            {
                if (cells.ContainsKey(k) == false)
                    continue;

                var queue = new Queue<Vector3Int>();
                queue.Enqueue(k);

                while (queue.Any())
                {
                    var c = queue.Dequeue();
                    foreach (var d in del1)
                    {
                        var n = c + d;
                        if (cells.ContainsKey(n) == false)
                            continue;
                        if (n == k)
                            continue;

                        // 指定した距離以上は無視
                        if ((k - n).Abs().Sum() > mergeLen)
                            continue;

                        // 重複があった場合はそっちに近づけるため単純にAddRangeする
                        cells[k].AddRange(cells[n]);
                        cells.Remove(n);
                        queue.Enqueue(n);
                    }
                }
            }

            var ret = new Dictionary<Vector3, Vector3>();

            foreach (var c in cells)
            {
                // #NOTE : 1セルに1つの頂点しかない場合は無視でよい(メモリ最適化)
                if (c.Value.Count == 1)
                    continue;

                var center = c.Value.Aggregate(Vector3.zero, (v, a) => v + a) / c.Value.Count;
                foreach (var v in c.Value)
                    ret[v] = center;
            }

            return ret;
#if false
            // 26近傍のセルの差分
            var delta3 = Delta(1);
            void Search(Vector3Int c)
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
        }

#if false
        public class NearestPointInfo
        {
            // 線分のインデックス
            public int Index { get; set; }

            // 線分のT値(Startからの距離)
            public float T { get; set; }

            // 線分と点との距離
            public float Distance { get; set; }

            // 最近傍点
            public Vector3 NearestPoint { get; set; }

            public static int Compare(NearestPointInfo a, NearestPointInfo b)
            {
                var ret = Comparer<int>.Default.Compare(a.Index, b.Index);
                if (ret != 0)
                    return ret;
                return Comparer<float>.Default.Compare(a.T, b.T);
            }

            public override string ToString()
            {
                return $"{Index}/{T:F2}";
            }
        }

        public class InnerSegment
        {
            public LineSegment3D Segment { get; set; }

            public int LeftIndex { get; }

            public int RightIndex { get; }

            public NearestPointInfo LeftNearestStartInfo { get; set; }

            public NearestPointInfo LeftNearestEndInfo { get; set; }

            public NearestPointInfo RightNearestStartInfo { get; set; }

            public NearestPointInfo RightNearestEndInfo { get; set; }

            public bool IsStartLeft { get; set; }

            public bool IsEndLeft { get; set; }

            // Left:Right = P : (1-P)となるP
            public float P { get; set; }

            // 輪郭線と交差しているかどうか
            public bool IsCrossed { get; set; }

            public InnerSegment(LineSegment3D segment, int leftIndex, int rightIndex, bool isStartLeft, bool isEndLeft,
                float p)
            {
                Segment = segment;
                LeftIndex = leftIndex;
                RightIndex = rightIndex;
                P = p;
                IsStartLeft = isStartLeft;
                IsEndLeft = isEndLeft;
            }

            public override string ToString()
            {
                //return $"{LeftIndex}-{RightIndex}(L={LeftNearestStartInfo}-{LeftNearestEndInfo},R={RightNearestStartInfo}-{RightNearestEndInfo}";
                return $"{LeftIndex}-{RightIndex}";
            }

            public static int Compare(InnerSegment a, InnerSegment b)
            {
                var ret = NearestPointInfo.Compare(a.LeftNearestStartInfo, b.LeftNearestStartInfo);
                if (ret != 0)
                    return ret;
                return NearestPointInfo.Compare(a.RightNearestStartInfo, b.RightNearestStartInfo);
            }
        }

        /// <summary>
        /// selfの左右の道を横幅p : (1-p)で分割した線分を返す. p=[0,1]
        /// 例) 0.5だと中央線が返る, 0だとLeftが返る, 1だとRightが返る. 
        /// </summary>
        /// <param name="seg1"></param>
        /// <param name="plane"></param>
        /// <param name="p"></param>
        /// <param name="op"></param>
        /// <param name="seg0"></param>
        /// <returns></returns>
        public static List<InnerSegment> GetInnerLerpSegments(
            IReadOnlyList<Vector3> seg0,
            IReadOnlyList<Vector3> seg1,
            AxisPlane plane,
            float p,
            DebugOption op = null)
        {
            p = Mathf.Clamp01(p);

            var lefts = GetEdges(seg0, false).Select(v => new LineSegment3D(v.Item1, v.Item2)).ToList();
            var rights = GetEdges(seg1, false).Select(v => new LineSegment3D(v.Item1, v.Item2)).ToList();

            var floatComparer = Comparer<float>.Default;

            var innerSegments = new List<InnerSegment>();

            Vector2 ToVec2(Vector3 v) => v.GetTangent(plane);

            Vector3 Make(Vector2 v, float h) => plane.Make(v, h);

            InnerSegment AddSegment(LineSegment2D segment, int leftIndex, int rightIndex, bool isStartLeft,
                bool isEndLeft, float p)
            {
                var isHit = false;

                // 最も近い線分を求める
                NearestPointInfo FindNearestPoint(List<LineSegment3D> edges, Vector2 pos)
                {
                    Enumerable.Range(0, edges.Count)
                        .Select(i =>
                        {
                            var e = edges[i];
                            var near = e.GetNearestPoint(pos, out var t);
                            return new { near, dist = (ToVec2(near) - pos).sqrMagnitude, index = i, t };
                        }).TryFindMin(a => a.dist, out var x);
                    return new NearestPointInfo()
                    {
                        Index = x.index,
                        T = x.t,
                        Distance = (ToVec2(x.near) - pos).magnitude,
                        NearestPoint = x.near
                    };
                }

                var leftNearestStartInfo = FindNearestPoint(lefts, segment.Start);
                var leftNearestEndInfo = FindNearestPoint(lefts, segment.End);
                var rightNearestStartInfo = FindNearestPoint(rights, segment.Start);
                var rightNearestEndInfo = FindNearestPoint(rights, segment.End);

                float ToHeight(NearestPointInfo lNear, NearestPointInfo rNear)
                {
                    var sum = lNear.Distance + rNear.Distance;
                    if (sum <= 0f)
                        return lNear.NearestPoint.GetNormal(plane);
                    var rate = lNear.Distance / sum;
                    return Mathf.Lerp(lNear.NearestPoint.GetNormal(plane), rNear.NearestPoint.GetNormal(plane), rate);
                }

                var start = Make(segment.Start, ToHeight(leftNearestStartInfo, rightNearestStartInfo));
                var end = Make(segment.End, ToHeight(leftNearestEndInfo, rightNearestEndInfo));
                var ev =
                    new InnerSegment(new LineSegment3D(start, end), leftIndex, rightIndex, isStartLeft, isEndLeft, p)
                    {
                        LeftNearestStartInfo = FindNearestPoint(lefts, segment.Start),
                        LeftNearestEndInfo = FindNearestPoint(lefts, segment.End),
                        RightNearestStartInfo = FindNearestPoint(rights, segment.Start),
                        RightNearestEndInfo = FindNearestPoint(rights, segment.End),
                        IsCrossed = isHit
                    };
                innerSegments.Add(ev);
                return ev;
            }

            var leftIndex = 0;
            var rightIndex = 0;
            while (leftIndex < lefts.Count && rightIndex < rights.Count)
            {
                var l = lefts[leftIndex].To2D(ToVec2);
                var r = rights[rightIndex].To2D(ToVec2);
                var centerRay = LerpRay(l.Ray, r.Ray, p);

                // centerRayと[l.start, l.end, r.start, r.end]との交点を求めてソート
                // lとrがかぶらないと無視
                var dirL = new Vector2(l.Direction.y, -l.Direction.x);
                var dirR = new Vector2(r.Direction.y, -r.Direction.x);
                var points = new[]
                    {
                        new { ray = new Ray2D(l.Start, dirL), isLeft = true },
                        new { ray = new Ray2D(l.End, dirL), isLeft = true },
                        new { ray = new Ray2D(r.Start, dirR), isLeft = false },
                        new { ray = new Ray2D(r.End, dirR), isLeft = false }
                    }
                    .Select(x =>
                    {
                        var hit = LineUtil.LineIntersection(centerRay, x.ray, out var inter, out var t1, out var t2);
                        return new
                        {
                            isHit = hit,
                            inter,
                            tCenterRay = t1,
                            tRay = t2,
                            x.isLeft,
                            x.ray.origin
                        };
                    }).Where(x => x.isHit)
                    .ToList();

                points.Sort((a, b) => floatComparer.Compare(a.tCenterRay, b.tCenterRay));

                // ベースラインの切り替えが走ったかどうか
                if (points.Any() == false)
                    break;

                // 重ならない時は無視
                if (points.Count > 1 && points[0].isLeft != points[1].isLeft)
                {
                    // left-rightが重ならない時は無視
                    //if (baseRef.HasValue && points.Count == 4 && points[0].Item3 != points[1].Item3)
                    var begSeg = points[1];
                    var endSeg = points[2];
                    if (begSeg.tCenterRay >= 0 || endSeg.tCenterRay >= 0)
                    {
                        var segment = new LineSegment2D(points[1].inter, points[2].inter);
                        var ev = AddSegment(segment, leftIndex, rightIndex, begSeg.isLeft, endSeg.isLeft, p);
                        if (op?.showReturnSegments ?? false)
                        {
                            DebugEx.DrawString(ev.ToString(), ev.Segment.Start);
                        }
                    }
                }


                // より遠いのが左の場合右の線分を進める
                if (points.Last().isLeft)
                {
                    rightIndex++;
                }
                else
                {
                    leftIndex++;
                }
            }

            //innerSegments.Sort(InnerSegment.Compare);
            return innerSegments;
        }
#endif
        /// <summary>
        /// selfの左右の道を横幅p : (1-p)で分割した線分を返す. p=[0,1]
        /// 例) 0.5だと中央線が返る, 0だとLeftが返る, 1だとRightが返る. 
        /// </summary>
        /// <param name="leftVertices"></param>
        /// <param name="rightVertices"></param>
        /// <param name="plane"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<Vector3> GetInnerLerpSegments(
            IReadOnlyList<Vector3> leftVertices
            , IReadOnlyList<Vector3> rightVertices
            , AxisPlane plane,
            float p)
        {
            p = Mathf.Clamp01(p);
            var leftEdges = GetEdges(leftVertices, false).Select(v => new LineSegment3D(v.Item1, v.Item2)).ToList();
            var rightEdges = GetEdges(rightVertices, false).Select(v => new LineSegment3D(v.Item1, v.Item2)).ToList();

            var indices = new List<float>();

            indices.AddRange(Enumerable.Range(0, leftVertices.Count).Select(i => (float)i));

            bool IsInInnerSide(LineSegment3D? e, Vector3 d, bool reverse, bool isPrev)
            {
                if (e.HasValue == false)
                    return true;
                var ed2 = e.Value.Direction.ToVector2(plane);
                var d2 = d.ToVector2(plane);
                var cross = Vector2Ex.Cross(ed2, d2);
                if (reverse == false)
                    cross = -cross;
                if (cross > 0)
                    return false;
                if (cross == 0f)
                {
                    var dot = Vector2.Dot(ed2, d2);
                    if (isPrev)
                        dot = -dot;
                    return dot < 0f;
                }

                return true;
            }

            bool CheckCollision(Vector3 a, Vector3 b, IList<LineSegment3D> edges, float indexF)
            {
                var a2 = a.ToVector2(plane);
                var b2 = b.ToVector2(plane);
                var index = (int)indexF;
                var f = indexF - index;
                var prevIndex = f > 0 ? index : index - 1;
                for (var i = 0; i < edges.Count; ++i)
                {
                    if (i == index || i == prevIndex)
                        continue;
                    var e = edges[i];
                    var e2 = e.To2D(plane);
                    if (e2.TrySegmentIntersection(a2, b2, out var _, out var _, out var _))
                        return true;
                }

                return false;
            }

            for (var i = 0; i < rightVertices.Count; ++i)
            {
                var pos = rightVertices[i];

                var prevEdge = i > 0 ? rightEdges[i - 1] : (LineSegment3D?)null;
                var nextEdge = i < rightEdges.Count ? rightEdges[i] : (LineSegment3D?)null;

                float minIndexF = -1;
                float minDist = float.MaxValue;
                for (var edgeIndex = 0; edgeIndex < leftEdges.Count; ++edgeIndex)
                {
                    var e = leftEdges[edgeIndex];
                    var nearPos = e.GetNearestPoint(pos, out var distanceFromStart);
                    var d = nearPos - pos;

                    var dist = d.magnitude;
                    if (dist >= minDist)
                        continue;
                    if (IsInInnerSide(prevEdge, d, false, true) == false)
                        continue;
                    if (IsInInnerSide(nextEdge, d, false, false) == false)
                        continue;
                    if (CheckCollision(pos, nearPos, rightEdges, i))
                        continue;
                    minDist = dist;
                    minIndexF = edgeIndex + distanceFromStart / e.Magnitude;
                }

                if (minIndexF < 0)
                    continue;
                indices.Add(minIndexF);
            }

            indices.Sort();

            var searchRightIndex = 0;
            var ret = new List<Vector3>();
            foreach (var indexF in indices)
            {
                var i = Mathf.Clamp((int)indexF, 0, leftEdges.Count - 1);
                var e1 = leftEdges[i];
                var f = Mathf.Clamp01(indexF - i);
                var pos = Vector3.Lerp(e1.Start, e1.End, f);

                LineSegment3D? prevEdge = null;
                LineSegment3D? nextEdge = null;
                if (f is > 0f and < 1f)
                {
                    prevEdge = new LineSegment3D(e1.Start, pos);
                    nextEdge = new LineSegment3D(pos, e1.End);
                }
                else
                {
                    if (i > 0)
                        prevEdge = leftEdges[i - 1];
                    if (i < leftEdges.Count)
                        nextEdge = leftEdges[i];
                }

                float minIndexF = -1;
                float minDist = float.MaxValue;
                Vector3 minPos = Vector3.zero;
                for (var edgeIndex = searchRightIndex; edgeIndex < rightEdges.Count; ++edgeIndex)
                {
                    var e2 = rightEdges[edgeIndex];
                    var nearPos = e2.GetNearestPoint(pos, out var t);
                    var d = nearPos - pos;
                    var dist = d.magnitude;

                    if (dist >= minDist)
                        continue;
                    if (IsInInnerSide(prevEdge, d, true, true) == false)
                        continue;
                    if (IsInInnerSide(nextEdge, d, true, false) == false)
                        continue;

                    if (CheckCollision(pos, nearPos, leftEdges, indexF))
                        continue;
                    minDist = dist;
                    minIndexF = edgeIndex + t;
                    minPos = nearPos;
                }

                if (minIndexF < 0)
                    continue;
                // #TODO : やってみたらおかしくなったので毎回最初から探す
                // 高速化のため. 戻ることは無いはずなので見つかったindexから探索でよいはず
                //searchRightIndex = (int)minIndexF;

                ret.Add(Vector3.Lerp(pos, minPos, p));
            }

            return ret;
        }
    }
}