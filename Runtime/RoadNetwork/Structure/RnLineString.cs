﻿using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// 線分群クラス. 頂点のリストを持つ
    /// </summary>
    [Serializable]
    public class RnLineString : ARnParts<RnLineString>, IReadOnlyList<Vector3>
    {

        public class AddPointOption
        {
            public float DistanceEpsilon { get; set; } = -1;

            public float DegEpsilon { get; set; } = -1;

            public float MidPointTolerance { get; set; } = -1f;
        }

        //----------------------------------
        // start: フィールド
        //----------------------------------
        public List<RnPoint> Points { get; } = new List<RnPoint>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public int Count => Points.Count;

        // 頂点が2つ以上ある有効な線分かどうか
        public bool IsValid => Count >= 2;

        public RnLineString()
        {

        }

        public RnLineString(IEnumerable<RnPoint> points)
        {
            Points = points.ToList();
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// rateSelector : 分割線分の長さを取得する関数.numでちょうど1になるようにする必要がある.  nullの場合は等分割. rateSel
        /// </summary>
        /// <param name="num"></param>
        /// <param name="insertNewPoint"></param>
        /// <param name="rateSelector"></param>
        /// <returns></returns>
        public List<RnLineString> Split(int num, bool insertNewPoint, Func<int, float> rateSelector = null)
        {
            if (Points.Count <= 1)
                return new List<RnLineString>();

            if (num <= 1)
                return new List<RnLineString> { Clone() };

            if (rateSelector == null)
                rateSelector = i => 1f / num;
            var ret = new List<List<RnPoint>>();
            var totalLength = LineUtil.GetLineSegmentLength(this);
            var len = 0f;
            var subVertices = new List<RnPoint> { Points[0] };

            float GetLength(int i) => totalLength * rateSelector(i);
            for (var i = 1; i < Points.Count; ++i)
            {
                var p0 = subVertices.Last();
                var p1 = Points[i];
                var l = (p1.Vertex - p0.Vertex).magnitude;
                len += l;

                var length = GetLength(ret.Count);
                // lenがlengthを超えたら分割線分を追加
                // ただし、最後の線は全部追加する
                while (len >= length && l >= GeoGraph2D.Epsilon && ret.Count < (num - 1))
                {
                    // #TODO : マジックナンバー
                    //       : 分割点が隣り合う点とこれ以下の場合は新規で作らず使いまわす
                    var mergeLength = Mathf.Min(1e-1f, length * 0.5f);
                    var threshold = mergeLength * mergeLength;
                    var f = 1f - (len - length) / l;
                    var end = new RnPoint(Vector3.Lerp(p0, p1, f));

                    // もし,p0/p1とほぼ同じ点ならそっちを使う
                    // ただし、その結果subVerticesが線分にならない場合は無視する
                    if (subVertices.Count > 1)
                    {
                        if ((p1.Vertex - end.Vertex).sqrMagnitude < threshold)
                        {
                            end = p1;
                        }
                        else if ((p0.Vertex - end.Vertex).sqrMagnitude < threshold)
                        {
                            end = p0;
                        }
                    }

                    // 同一頂点が複数あった場合は無視する
                    if (f >= float.Epsilon)
                    {
                        subVertices.Add(end);
                        // 自分自身にも追加する場合
                        if (insertNewPoint && p1 != end && p0 != end)
                        {
                            Points.Insert(i, end);
                            i += 1;
                        }
                    }

                    ret.Add(subVertices);
                    subVertices = new List<RnPoint> { end };
                    len -= length;
                    // 次の長さを更新
                    length = GetLength(ret.Count);
                }
                if (subVertices.LastOrDefault() != p1)
                    subVertices.Add(p1);
            }

            // 最後の要素は無条件で返す
            if (ret.Count < num && subVertices.Any())
            {
                if (subVertices.Last() != Points.Last())
                    subVertices.Add(Points.Last());
                if (subVertices.Count > 1)
                    ret.Add(subVertices);
            }

            // 分割できない時は空を返す
            return ret.Select(a => Create(a)).ToList();
        }

        /// <summary>
        /// index指定で前半/後半に分割したLineStringを返す(非破壊)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="front"></param>
        /// <param name="back"></param>
        /// <returns></returns>
        public bool SplitByIndex(float index, out RnLineString front, out RnLineString back)
        {
            // indexが整数の時で処理を変える
            var isInt = Mathf.Abs(index - Mathf.RoundToInt(index)) < 1e-5f;
            // 桁落ちを考えて, isInt時にはRoundを取る
            var i = isInt ? Mathf.RoundToInt(index) : (int)index;


            // points = [v0, v1, v2, v3]の時
            // index = 1で区切るときは, front = [v0, v1], back = [v1, v2, v3]
            // -> i = 1, frontはTake(1), backはskip(2)にして、v1をお互いに追加
            // index = 1.5で区切るときは,front = [v0, v1, v1.5], back = [v1.5, v2, v3]
            // -> i = 1, frontはTake(2), backはskip(2)にして、v1.5を追加

            // まずはiで前半と後半で分ける
            var frontPoints = Points.Take(i + 1).ToList();
            var backPoints = Points.Skip(i + 1).ToList();

            if (isInt)
            {
                // 整数の時はfrontの最後をbackの最初に追加
                backPoints.Insert(0, Points[i]);
            }
            else
            {
                // 少数の時は中間点をfontの最後とbackの最初に追加
                var v = Vector3.Lerp(Points[i].Vertex, Points[i + 1].Vertex, index - i);
                var mid = new RnPoint(v);
                frontPoints.Add(mid);
                backPoints.Insert(0, mid);
            }

            front = Create(frontPoints);
            back = Create(backPoints);
            return true;
        }

        /// <summary>
        /// 後ろに点を追加する
        /// 1. 後ろの点が同じなら追加しない
        /// 2. 追加した結果, 最後3点による二つの直線がほぼ同じ直線になる場合は中間点を削除する
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distanceEpsilon">距離誤差</param>
        /// <param name="degEpsilon">角度誤差(p0 -> p1 -> p2の角度が180±この値以内になるときp1を削除する</param>
        /// <param name="midPointTolerance">p0 -> p1 -> p2の３点があったときに、p0->p2の直線とp1の距離がこれ以下ならp1を削除する</param>
        public void AddPointOrSkip(RnPoint p, float distanceEpsilon = DefaultDistanceEpsilon, float degEpsilon = DefaultDegEpsilon, float midPointTolerance = DefaultMidPointTolerance)
        {
            if (p == null)
                return;
            if (Points.Count > 0 && RnPoint.Equals(Points.Last(), p, distanceEpsilon < 0 ? -1f : distanceEpsilon * distanceEpsilon))
                return;
            if (Points.Count > 1 && GeoGraphEx.IsCollinear(Points[^2], Points[^1], p.Vertex, degEpsilon, midPointTolerance))
                Points.RemoveAt(Points.Count - 1);
            Points.Add(p);
        }

        /// <summary>
        /// 先頭に点を追加する.
        /// 1. 前の点が同じなら追加しない
        /// 2. 追加した結果, 最初3点による二つの直線がほぼ同じ直線になる場合は中間点を削除する
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distanceEpsilon">距離誤差</param>
        /// <param name="degEpsilon">角度誤差(p0 -> p1 -> p2の角度が180±この値以内になるときp1を削除する</param>
        /// <param name="midPointTolerance">p0 -> p1 -> p2の３点があったときに、p0->p2の直線とp1の距離がこれ以下ならp1を削除する</param>
        public void AddPointFrontOrSkip(RnPoint p, float distanceEpsilon = DefaultDistanceEpsilon, float degEpsilon = DefaultDegEpsilon, float midPointTolerance = DefaultMidPointTolerance)
        {
            if (p == null)
                return;
            if (Points.Count > 0 && RnPoint.Equals(Points.First(), p, distanceEpsilon < 0 ? -1f : distanceEpsilon * distanceEpsilon))
                return;
            if (Points.Count > 1 && GeoGraphEx.IsCollinear(Points[1], Points[0], p.Vertex, degEpsilon, midPointTolerance))
                Points.RemoveAt(0);
            Points.Insert(0, p);
        }

        /// <summary>
        /// 同じ点かどうかのチェック無しに追加する
        /// </summary>
        /// <param name="p"></param>
        public void AddPoint(RnPoint p)
        {
            if (p == null)
                return;
            Points.Add(p);
        }

        /// <summary>
        /// 同じ点かどうかのチェック無しに追加する
        /// </summary>
        /// <param name="p"></param>
        public void AddFrontPoint(RnPoint p)
        {
            if (p == null)
                return;
            Points.Insert(0, p);
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return Points.Select(v => v.Vertex).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Vector3 this[int index] => Points[index].Vertex;

        /// <summary>
        /// 線分の長さを計算する
        /// </summary>
        /// <returns></returns>
        public float CalcLength()
        {
            if (IsValid == false)
                return 0f;
            return GeoGraphEx.GetEdges(Points.Select(x => x.Vertex), false).Sum(e => (e.Item1 - e.Item2).magnitude);
        }

        /// <summary>
        /// 頂点 vertexIndex -> vertexIndex, vertexIndex -> vertexIndex + 1の方向に対して
        /// 道の外側を向いている法線ベクトルの平均を返す.正規化済み.
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertexIndex)
        {
            // 頂点数1の時は不正値を返す
            if (IsValid == false)
                return Vector3.zero;
            if (vertexIndex == 0)
                return GetEdgeNormal(0).normalized;
            if (vertexIndex == Count - 1)
                return GetEdgeNormal(Count - 2).normalized;

            return (GetEdgeNormal(vertexIndex - 1).normalized + GetEdgeNormal(vertexIndex).normalized).normalized;
        }

        /// <summary>
        /// 頂点 startVertexIndex, startVertexIndex + 1で構成される辺の法線ベクトルを返す
        /// 上(Vector3.up)から見て半時計回りを向いている. 正規化済み
        /// </summary>
        /// <param name="startVertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetEdgeNormal(int startVertexIndex)
        {
            var p0 = this[startVertexIndex];
            var p1 = this[startVertexIndex + 1];
            // Vector3.Crossは左手系なので逆
            return (-Vector3.Cross(Vector3.up, p1 - p0)).normalized;
        }

        /// <summary>
        /// 自身のコピーを作成する.
        /// clonePoint : 頂点もコピーするかどうか
        /// </summary>
        /// <returns></returns>
        public RnLineString Clone(bool cloneVertex = true)
        {
            if (cloneVertex)
                return Create(Points.Select(p => p.Clone()), false);
            return Create(Points, false);
        }

        /// <summary>
        /// ポイントの入れ替え
        /// </summary>
        /// <param name="oldPoint"></param>
        /// <param name="newPoint"></param>
        /// <returns></returns>
        public int ReplacePoint(RnPoint oldPoint, RnPoint newPoint)
        {
            var ret = 0;
            for (var i = 0; i < Count; ++i)
            {
                if (Points[i] == oldPoint)
                {
                    Points[i] = newPoint;
                    ret++;
                }
            }

            return ret;
        }

        // ---------------
        // Static Methods
        // ---------------

        private const float DefaultDistanceEpsilon = 0f;
        private const float DefaultDegEpsilon = 0.5f;
        private const float DefaultMidPointTolerance = 0.3f;


        /// <summary>
        /// 頂点リストから線分を生成する
        /// distanceEpsilon : 頂点間の距離がこの値以下の場合は同じ点とみなす
        /// degEpsilon : 3点が同一直線上にあるときの角度誤差. 180±degEpsilon以内の場合は中間点を削除する
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="distanceEpsilon"></param>
        /// <param name="degEpsilon"></param>
        /// <param name="midPointTolerance"></param>
        /// <returns></returns>
        public static RnLineString Create(IEnumerable<RnPoint> vertices, float distanceEpsilon, float degEpsilon, float midPointTolerance)
        {
            var ret = new RnLineString();
            foreach (var v in vertices)
                ret.AddPointOrSkip(v, distanceEpsilon, degEpsilon, midPointTolerance);
            return ret;
        }

        public static RnLineString Create(IEnumerable<RnPoint> vertices, bool removeDuplicate = true)
        {
            if (removeDuplicate)
                return Create(vertices, DefaultDistanceEpsilon, DefaultDegEpsilon, DefaultMidPointTolerance);
            return Create(vertices, -1, -1, -1f);
        }

        public static RnLineString Create(IEnumerable<Vector3> vertices, bool removeDuplicate = true)
        {
            return Create(vertices.Select(v => new RnPoint(v)), removeDuplicate);
        }

        /// <summary>
        /// x/yが同じかどうか
        /// 参照一致だけでなく値一致でもtrue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Equals(RnLineString x, RnLineString y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            if (x.Count != y.Count)
                return false;

            return x.Points.SequenceEqual(y.Points);
        }
    }

    public static class RnLineStringEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RnLineString self, AxisPlane axis = AxisPlane.Xz)
        {
            foreach (var e in GeoGraphEx.GetEdges(self.Points.Select(x => x.Vertex.ToVector2(axis)), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }

        /// <summary>
        /// 線分をLineSegment3Dにして返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<LineSegment3D> GetEdges(this RnLineString self)
        {
            foreach (var e in GeoGraphEx.GetEdges(self.Points.Select(x => x.Vertex), false))
                yield return new LineSegment3D(e.Item1, e.Item2);
        }

        /// <summary>
        /// selfをlineの交点をすべて返す. ただしaxis辺面に射影↓状態で交差判定を行う.
        /// 実際に返る交点はself上の点とその時のインデックス(float)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="line"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static IEnumerable<(Vector3 v, float index)> GetIntersectionBy2D(this RnLineString self, LineSegment3D line, AxisPlane axis = AxisPlane.Xz)
        {
            foreach (var item in self.GetEdges().Select((edge, i) => new { edge, i }))
            {
                if (item.edge.TrySegmentIntersectionBy2D(line, axis, -1f, out var p, out var t1, out var t2))
                {
                    var v = item.edge.Lerp(t1);
                    yield return (v, item.i + t1);
                }
            }
        }

        /// <summary>
        /// 点vに最も近いself上の点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v"></param>
        /// <param name="nearest"></param>
        /// <param name="pointIndex"></param>
        /// <param name="distance"></param>
        public static void GetNearestPoint(this RnLineString self, Vector3 v, out Vector3 nearest, out float pointIndex, out float distance)
        {
            nearest = Vector3.zero;
            var sqrDistance = float.MaxValue;
            pointIndex = -1f;
            for (var i = 0; i < self.Count - 1; ++i)
            {
                var segment = new LineSegment3D(self[i], self[i + 1]);
                var p = segment.GetNearestPoint(v, out var distanceFromStart);
                var d = (p - v).sqrMagnitude;
                if (d < sqrDistance)
                {
                    nearest = p;
                    sqrDistance = d;
                    pointIndex = i + distanceFromStart / segment.Magnitude;
                }
            }

            distance = Mathf.Sqrt(sqrDistance);
        }

        /// <summary>
        /// floatのindexを指定して線分上の点を取得する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Vector3 GetLerpPoint(this RnLineString self, float index)
        {
            var i1 = (int)index;
            var i2 = i1 + 1;
            if (i2 >= self.Count)
            {
                return self[^1];
            }
            var t = index - i1;
            return Vector3.Lerp(self[i1], self[i2], t);
        }

        /// <summary>
        /// selfの各線分がintervalより長い場合に間に点を置いていく
        /// </summary>
        /// <param name="self"></param>
        /// <param name="interval"></param>
        public static void Refine(this RnLineString self, float interval)
        {
            // 余りに小さい場合は何もしない
            if (interval <= 1e-3f)
                return;

            for (var i = 0; i < self.Count - 1; ++i)
            {
                var p0 = self[i];
                var p1 = self[i + 1];
                var len = (p1 - p0).magnitude;

                var num = len / interval;
                var newPoints = new List<RnPoint>();
                for (var j = 1; j < num; ++j)
                {
                    var t = j / num;
                    newPoints.Add(new RnPoint(Vector3.Lerp(p0, p1, t)));
                }

                if (newPoints.Count > 0)
                    self.Points.InsertRange(i + 1, newPoints);
                i += newPoints.Count;
            }
        }

        /// <summary>
        /// selfの各線分がintervalより長い場合に間に点を置いて細分化したものを返す.非破壊
        /// </summary>
        /// <param name="self"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static RnLineString Refined(this RnLineString self, float interval)
        {
            var ret = self.Clone(false);
            ret.Refine(interval);
            return ret;
        }

        /// <summary>
        /// LineStringの線分群が成す角度の合計を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcTotalAngle2D(this RnLineString self)
        {
            var ret = 0f;
            LineSegment2D? last = null;
            foreach (var e in self.GetEdges2D())
            {
                if (last != null)
                {
                    var ang = Vector2.SignedAngle(-last.Value.Direction, e.Direction);
                    if (ang < 0)
                        ang += 360f;
                    ret += ang;
                }

                last = e;
            }

            return ret;
        }
    }
}