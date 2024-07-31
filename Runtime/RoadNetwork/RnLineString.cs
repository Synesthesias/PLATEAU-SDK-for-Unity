using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 線分群クラス. 頂点のリストを持つ
    /// </summary>
    [Serializable]
    public class RnLineString : ARnParts<RnLineString>, IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        public List<RnPoint> Points { get; } = new List<RnPoint>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 頂点法線のキャッシュ
        private Dictionary<RnPoint, Vector3> CachedVertexNormal { get; set; } = new Dictionary<RnPoint, Vector3>();

        // 頂点法線ベクトルのキャッシュをクリアする
        public bool DirtyFlag { get; set; }

        public int Count => Points.Count;

        // 頂点が2つ以上ある有効な線分かどうか
        public bool IsValid => Count >= 2;

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// getSubLength : 分割線分の長さを取得する関数. nullの場合は等分割
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

                // lenがlengthを超えたら分割線分を追加
                var length = GetLength(subVertices.Count);
                while (len >= length && l >= GeoGraph2D.Epsilon)
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
                    length = GetLength(subVertices.Count);
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
        /// 後ろの点が同じなら追加しない
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distanceEpsilon">距離誤差</param>
        /// <param name="degEpsilon">角度誤差(p0 -> p1 -> p2の角度が180±degEpsilon以内になるときp1を削除する</param>
        public void AddPointOrSkip(RnPoint p, float distanceEpsilon = DefaultDistanceEpsilon, float degEpsilon = DefaultDegEpsilon)
        {
            if (Points.Count > 0 && RnPoint.Equals(Points.Last(), p, distanceEpsilon))
                return;
            if (Points.Count > 1)
            {
                var deg = Vector3.Angle(Points[^2].Vertex - Points[^1].Vertex, p.Vertex - Points[^1].Vertex);
                if (Mathf.Abs(180f - deg) <= degEpsilon)
                    Points.RemoveAt(Points.Count - 1);
            }
            Points.Add(p);
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
        /// 頂点の法線ベクトルを返す. キャッシュ化されており, DirtyFlagをtrueにすると再計算される
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertexIndex, bool useCache = true)
        {
            // 頂点数1の時は不正値を返す
            if (IsValid == false)
                return Vector3.zero;

            // キャッシュを使わない場合はその場で計算
            if (useCache == false)
            {
                if (vertexIndex == 0)
                    return GetEdgeNormal(0).normalized;
                if (vertexIndex == Count - 1)
                    return GetEdgeNormal(Count - 2).normalized;

                return (GetEdgeNormal(vertexIndex - 1).normalized + GetEdgeNormal(vertexIndex).normalized).normalized;
            }

            var p = Points[vertexIndex];
            if (DirtyFlag || CachedVertexNormal.ContainsKey(p) == false)
                CalcVertexNormal();

            return CachedVertexNormal[p];
        }

        /// <summary>
        /// 頂点法線ベクトルを計算してキャッシュ化する
        /// </summary>
        public void CalcVertexNormal()
        {
            CachedVertexNormal.Clear();
            var n1 = GetEdgeNormal(0).normalized;
            CachedVertexNormal[Points[0]] = n1;
            CachedVertexNormal[Points[Count - 1]] = GetEdgeNormal(Count - 2).normalized;
            for (var i = 1; i < Count - 1; ++i)
            {
                var p = Points[i];
                var n2 = GetEdgeNormal(i).normalized;
                CachedVertexNormal[p] = (n1 + n2).normalized;
                n1 = n2;
            }
            DirtyFlag = false;
        }


        /// <summary>
        /// 頂点 startVertexIndex, startVertexIndex + 1で構成される辺の法線ベクトルを返す
        /// 道の外側を向いている. 正規化はされていない
        /// </summary>
        /// <param name="startVertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetEdgeNormal(int startVertexIndex)
        {
            var p0 = this[startVertexIndex];
            var p1 = this[startVertexIndex + 1];
            // Vector3.Crossは左手系なので逆
            return -Vector3.Cross(Vector3.up, p1 - p0);
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

        public const float DefaultDistanceEpsilon = 0f;
        public const float DefaultDegEpsilon = 0.5f;

        /// <summary>
        /// 頂点リストから線分を生成する
        /// distanceEpsilon : 頂点間の距離がこの値以下の場合は同じ点とみなす
        /// degEpsilon : 3点が同一直線上にあるときの角度誤差. 180±degEpsilon以内の場合は中間点を削除する
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="distanceEpsilon"></param>
        /// <param name="degEpsilon"></param>
        /// <returns></returns>
        public static RnLineString Create(IEnumerable<RnPoint> vertices, float distanceEpsilon, float degEpsilon)
        {
            var ret = new RnLineString();
            foreach (var v in vertices)
                ret.AddPointOrSkip(v, distanceEpsilon, degEpsilon);
            return ret;
        }

        public static RnLineString Create(IEnumerable<RnPoint> vertices, bool removeDuplicate = true)
        {
            if (removeDuplicate)
                return Create(vertices, DefaultDistanceEpsilon, DefaultDegEpsilon);
            return Create(vertices, -1, -1);
        }

        public static RnLineString Create(IEnumerable<Vector3> vertices)
        {
            return Create(vertices.Select(v => new RnPoint(v)));
        }

        /// <summary>
        /// x/yが同じかどうか
        /// 参照一致だけでなく値一致でもtrue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="shallowEqual"></param>
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

    public static class RoadNetworkLineStringEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RnLineString self)
        {
            foreach (var e in GeoGraphEx.GetEdges(self.Points.Select(x => x.Vertex.Xz()), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }

        /// <summary>
        /// 線分の長さを取得
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcLength(this RnLineString self)
        {
            return LineUtil.GetLineSegmentLength(self);
        }
    }
}