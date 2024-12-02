using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// WayのPointsをReadOnlyで返すラッパー
    /// </summary>
    public class RnWayPoints : IReadOnlyList<RnPoint>
    {
        private readonly RnWay way;

        public RnWayPoints(RnWay way)
        {
            this.way = way;
        }

        public IEnumerator<RnPoint> GetEnumerator()
        {
            return way.Points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => way.Count;

        public RnPoint this[int index] => way.GetPoint(index);
    }

    public class RnWayEqualityComparer : IEqualityComparer<RnWay>
    {
        // 同じLineStringであれば同一判定とする
        public bool SameLineIsEqual { get; set; } = true;

        /// <summary>
        /// sameLineIsEqual : 同じLineStringであれば同一判定とする
        /// </summary>
        /// <param name="sameLineIsEqual"></param>
        public RnWayEqualityComparer(bool sameLineIsEqual) { SameLineIsEqual = sameLineIsEqual; }

        public bool Equals(RnWay x, RnWay y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (SameLineIsEqual)
            {
                return x.IsSameLine(y);
            }

            return x.IsReversed == y.IsReversed && x.IsReverseNormal == y.IsReverseNormal && Equals(x.LineString, y.LineString);
        }

        public int GetHashCode(RnWay obj)
        {
            if (SameLineIsEqual)
                return obj.LineString.GetHashCode();
            return HashCode.Combine(obj.IsReversed, obj.IsReverseNormal, obj.LineString);
        }
    }

    /// <summary>
    /// 方向を持つ線分クラス. 車線を構成する1ライン
    /// 同じ線分だけど向きが逆ということが多々あるのでメモリ削減 & 比較を楽にするため
    /// </summary>
    [Serializable]
    public class RnWay : ARnParts<RnWay>, IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // LineStringの向きが逆かどうか
        public bool IsReversed { get; set; } = false;

        // 法線が進行方向に対して左側か右側か. trueなら右側
        public bool IsReverseNormal { get; set; } = false;

        // 頂点群
        public RnLineString LineString { get; private set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// 頂点情報を返す
        /// </summary>
        public IEnumerable<Vector3> Vertices
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// 頂点情報をPoint型で返す(頂点変更できるように)
        /// </summary>
        public IEnumerable<RnPoint> Points
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    yield return GetPoint(i);
            }

            set
            {
                var points = value.ToArray();
                LineString = RnLineString.Create(value, false);
            }
        }

        /// <summary>
        /// 頂点取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RnPoint GetPoint(int index)
        {
            // 負数の時は逆からのインデックスに変換
            var i = ToRawIndex(index, true);
            return LineString.Points[i];
        }

        /// <summary>
        /// 頂点書き換え. 戻り値は変更前の値
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        public RnPoint SetPoint(int index, RnPoint p)
        {
            var i = ToRawIndex(index, true);
            var ret = LineString.Points[i];
            LineString.Points[i] = p;
            return ret;
        }

        // 頂点数
        public int Count => LineString?.Count ?? 0;

        // 2頂点以上ある有効な道かどうか
        public bool IsValid => LineString?.IsValid ?? false;

        /// <summary>
        /// RnWay生成
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="isReversed">LineStringの向きが逆かどうか</param>
        /// <param name="isReverseNormal">法線が進行方向に対して左側か右側か. trueなら右側</param>
        public RnWay(RnLineString lineString, bool isReversed = false, bool isReverseNormal = false)
        {
            LineString = lineString;
            IsReversed = isReversed;
            IsReverseNormal = isReverseNormal;
        }

        // デシリアライズのために必要
        public RnWay() { }

        /// <summary>
        /// 反転させたWayを返す(非破壊)
        /// </summary>
        /// <returns></returns>
        public RnWay ReversedWay()
        {
            return new RnWay(LineString, !IsReversed, !IsReverseNormal);
        }

        /// <summary>
        /// 線の向きを反転させる
        /// </summary>
        /// <param name="keepNormalDir">法線の向きは保持する</param>
        public void Reverse(bool keepNormalDir)
        {
            IsReversed = !IsReversed;
            // #NOTE : 反転させた段階で法線も逆になるのでkeepするときにIsReverseNormalを反転させる
            if (keepNormalDir)
                IsReverseNormal = !IsReverseNormal;
        }

        /// <summary>
        /// Reversedを考慮したインデックスへ変換する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="allowMinus">負数の場合は逆から検索</param>
        /// <returns></returns>
        private int ToRawIndex(int index, bool allowMinus = false)
        {
            if (allowMinus && index < 0)
                index = Count + index;
            return IsReversed ? Count - 1 - index : index;
        }

        /// <summary>
        /// IsReversed ? Count - 1 - index : index
        /// LineStringとWayのインデックスの相互変換
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int SwitchIndex(int index)
        {
            return IsReversed ? Count - 1 - index : index;
        }

        /// <summary>
        /// IsReversed ? Count - 1 - index : index
        /// LineStringとWayのインデックスの相互変換(float版)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float SwitchIndex(float index)
        {
            return IsReversed ? Count - 1 - index : index;
        }

        /// <summary>
        /// 頂点アクセス
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 this[int index]
        {
            get
            {
                return LineString[ToRawIndex(index)];
            }
            set
            {
                LineString[ToRawIndex(index)] = value;
            }
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
            if (Count <= 1)
                return Vector3.zero;

            var ret = LineString.GetVertexNormal(ToRawIndex(vertexIndex));
            if (IsReversed != IsReverseNormal)
                ret *= -1;
            return ret;
        }

        /// <summary>
        /// 頂点の法線ベクトルをリストで返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> GetVertexNormals()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return GetVertexNormal(i);
            }
        }

        /// <summary>
        /// 頂点 startVertexIndex, startVertexIndex + 1で構成される辺の法線ベクトルを返す
        /// 道の外側を向いている. 正規化済み
        /// </summary>
        /// <param name="startVertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetEdgeNormal(int startVertexIndex)
        {
            var index = ToRawIndex(startVertexIndex);
            // LineStringのGetEdgeNormalはindex, index+1で見るようになっているので
            // 逆方向の時は-1する必要がある
            if (IsReversed)
                index -= 1;
            var ret = LineString.GetEdgeNormal(index);
            if (IsReversed != IsReverseNormal)
                ret *= -1;
            return ret;
        }

        /// <summary>
        /// 辺の法線ベクトルをリストで返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> GetEdgeNormals()
        {
            for (var i = 0; i < Count - 1; i++)
            {
                yield return GetEdgeNormal(i);
            }
        }

        /// <summary>
        /// Xz平面だけで見たときの, 半直線rayの最も近い交点を返す
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool HalfLineIntersectionXz(Ray ray, out Vector3 intersection)
        {
            var ray2d = new Ray2D { direction = ray.direction.Xz(), origin = ray.origin.Xz() };

            intersection = Vector3.zero;
            var minLen = float.MaxValue;
            for (var i = 0; i < Count - 1; ++i)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                if (LineUtil.HalfLineSegmentIntersection(ray2d, p1.Xz(), p2.Xz(), out Vector2 _, out var t1, out var t2))
                {
                    var inter3d = Vector3.Lerp(p1, p2, t2);
                    var len = (inter3d - ray.origin).sqrMagnitude;
                    if (len < minLen)
                    {
                        minLen = len;
                        intersection = inter3d;
                    }
                }
            }
            return minLen < float.MaxValue;
        }

        /// <summary>
        /// Xz平面だけで見たときの, 線分(st, en)との最も近い交点を返す
        /// </summary>
        /// <param name="st"></param>
        /// <param name="en"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public bool SegmentIntersectionXz(Vector3 st, Vector3 en, out Vector3 intersection)
        {
            var st2d = st.Xz();
            var en2d = en.Xz();

            intersection = Vector3.zero;
            var minLen = float.MaxValue;
            for (var i = 0; i < Count - 1; ++i)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                if (LineUtil.SegmentIntersection(st2d, en2d, p1.Xz(), p2.Xz(), out Vector2 _, out var t1, out var t2))
                {
                    var inter3d = Vector3.Lerp(p1, p2, t2);
                    var len = (inter3d - st).sqrMagnitude;
                    if (len < minLen)
                    {
                        minLen = len;
                        intersection = inter3d;
                    }
                }
            }
            return minLen < float.MaxValue;
        }

        /// <summary>
        /// 線分の距離をp : (1-p)で分割した点をmidPointに入れて返す. 戻り値は midPointを含む線分のインデックス(i ~ i+1の線分上にmidPointがある) 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public float GetLerpPoint(float p, out Vector3 midPoint)
        {
            return LineUtil.GetLineSegmentLerpPoint(this, p, out midPoint);
        }

        public Vector3 GetLerpPoint(float p)
        {
            GetLerpPoint(p, out var midPoint);
            return midPoint;
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す.
        /// insertNewPoint=trueの時はselfにも新しい点を追加する
        /// </summary>
        /// <returns></returns>
        public List<RnWay> Split(int num, bool insertNewPoint, Func<int, float> rateSelector = null)
        {
            var ret = LineString.Split(num, insertNewPoint, rateSelector).Select(s => new RnWay(s, IsReversed, IsReverseNormal)).ToList();
            if (IsReversed)
                ret.Reverse();
            return ret;
        }

        /// <summary>
        /// 法線に沿って移動する
        /// </summary>
        /// <param name="offset"></param>
        public void MoveAlongNormal(float offset)
        {
            if (IsValid == false)
                return;

            // 頂点数が2の時は特殊処理
            if (Count == 2)
            {
                var n = GetEdgeNormal(0);
                foreach (var p in Points)
                    p.Vertex += n * offset;

                return;
            }

            var index = 0;
            // 現在見る点と次の点の辺/頂点の法線を保存しておく
            // 線分の法線
            var edgeNormal = new[] { GetEdgeNormal(0), GetEdgeNormal(Mathf.Min(Count - 1, 1)) };
            // 頂点の法線
            var vertexNormal = new[] { GetVertexNormal(0), GetVertexNormal(1) };
            var delta = offset;
            for (var i = 0; i < Count; ++i)
            {
                var en0 = edgeNormal[index];
                var en1 = edgeNormal[(index + 1) & 1];
                var vn = vertexNormal[index];

                // 形状維持するためにオフセット距離を変える
                // en0成分の移動量がdeltaになるように, vnの移動量を求める
                var m = Vector3.Dot(vn, en0);
                // p0->p1->p2でp0 == p2だったりした場合に0除算が発生するのでチェック
                // #TODO : 移動した結果頂点が重なる場合があるのでその対策が必要
                var d = delta;
                bool isZero = Mathf.Abs(m) < 1e-5f;
                if (isZero == false)
                    d /= m;
                var o = vn * d;
                if (i < Count - 1)
                {
                    vertexNormal[index] = GetVertexNormal(i + 1);
                    edgeNormal[index] = GetEdgeNormal(Mathf.Min(Count - 2, i + 1));
                    index = (index + 1) & 1;
                }
                GetPoint(i).Vertex += o;
                // 次の頂点計算のためにen1線分の移動量を入れる
                // #TODO : vnが0ベクトルの時の対応
                delta = d * Vector3.Dot(vn, en1);
            }
        }

        /// <summary>
        /// Way全体を動かす
        /// </summary>
        /// <param name="offset"></param>
        public void Move(Vector3 offset)
        {
            for (var i = 0; i < Count; ++i)
            {
                GetPoint(i).Vertex += offset;
            }
        }


        /// <summary>
        /// 自身のクローンを作成する.
        /// cloneVertexがtrueの時は頂点もクローンする
        /// </summary>
        /// <returns></returns>
        public RnWay Clone(bool cloneVertex)
        {
            return new RnWay(LineString.Clone(cloneVertex), IsReversed, IsReverseNormal);
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 同じ線分かどうか
        /// </summary>
        /// <param name="other"></param>
        /// <param name="onlyReferenceEqual"></param>
        /// <returns></returns>
        public bool IsSameLine(RnWay other, bool onlyReferenceEqual = true)
        {
            if (other == null)
                return false;
            if (onlyReferenceEqual)
                return LineString == other.LineString;
            return RnLineString.Equals(LineString, other.LineString);
        }
    }


    public static class RnWayEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RnWay self)
        {
            if (self == null)
                yield break;
            foreach (var e in GeoGraphEx.GetEdges(self.Vertices.Select(x => x.Xz()), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }

        public static int FindPoint(this RnWay self, RnPoint point)
        {
            return self.LineString.Points.FindIndex(p => p == point);
        }

        /// <summary>
        /// self.GetPoint(index) == pointとなるindexを返す. 見つからない場合は-1が返る.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static int FindPointIndex(this RnWay self, RnPoint point)
        {
            var index = self.LineString.Points.IndexOf(point);
            if (index < 0)
                return index;
            return self.SwitchIndex(index);
        }

        /// <summary>
        /// posからRnWay上の最も近い点を探す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        /// <param name="nearest"></param>
        /// <param name="pointIndex"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static void GetNearestPoint(this RnWay self, Vector3 pos, out Vector3 nearest, out float pointIndex, out float distance)
        {
            nearest = Vector3.zero;
            self.LineString.GetNearestPoint(pos, out nearest, out pointIndex, out distance);
            pointIndex = self.SwitchIndex(pointIndex);
        }


        /// <summary>
        /// nullチェック込みのIsValid
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsValidOrDefault(this RnWay self)
        {
            return self?.IsValid ?? false;
        }

        /// <summary>
        /// 線分の長さを取得
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float CalcLength(this RnWay self)
        {
            return self.LineString.CalcLength();
        }

        public static float CalcLength(this RnWay self, float startIndex, float endIndex)
        {
            if (self.IsReversed)
            {
                return self.LineString.CalcLength(self.SwitchIndex(endIndex), self.SwitchIndex(startIndex));
            }
            else
            {
                return self.LineString.CalcLength(startIndex, endIndex);
            }
        }

        /// <summary>
        /// selfの内部のLineStringにbackのLineStringを追加する
        /// self.Points ... back.Pointsの順になるようにIsReverseを考慮して追加する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="back"></param>
        public static void AppendBack2LineString(this RnWay self, RnWay back)
        {
            if (back == null)
                return;
            // 自己挿入は禁止
            if (self.IsSameLine(back))
                return;
            if (self.IsReversed)
            {
                foreach (var p in back.Points)
                    self.LineString.AddPointFrontOrSkip(p, 0f, 0f, 0f);
            }
            else
            {
                // IsReversedがfalseの時はそのまま追加
                foreach (var p in back.Points)
                    self.LineString.AddPointOrSkip(p, 0f, 0f, 0f);
            }
        }
        /// <summary>
        /// selfの内部のLineStringにfrontのLineStringを追加する
        /// back.Points... self.Pointsの順になるようにIsReverseを考慮して追加する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="front"></param>
        public static void AppendFront2LineString(this RnWay self, RnWay front)
        {
            if (front == null)
                return;
            // 自己挿入は禁止
            if (self.IsSameLine(front))
                return;
            if (self.IsReversed)
            {
                // IsReversedの時は逆順に後ろに追加
                for (var i = 0; i < front.Count; ++i)
                    self.LineString.AddPointOrSkip(front.GetPoint(front.Count - 1 - i), 0f, 0, 0);
            }
            else
            {
                // falseの時は逆順に前に追加
                for (var i = 0; i < front.Count; ++i)
                    self.LineString.AddPointFrontOrSkip(front.GetPoint(front.Count - 1 - i), 0f, 0f, 0f);
            }
        }

        /// <summary>
        /// a,bのPointsを結合した新しいWayを作成する(a, bは非破壊)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RnWay CreateMergedWay(RnWay a, RnWay b)
        {
            var ls = RnLineString.Create(a.Points.Concat(b.Points));
            return new RnWay(ls);
        }

        /// <summary>
        /// selfの方向に対してvが外側(法線と同じ側)かどうか
        /// </summary>
        /// <param name="self"></param>
        /// <param name="v"></param>
        /// <param name="nearest"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsOutSide(this RnWay self, Vector3 v, out Vector3 nearest, out float distance)
        {
            self.GetNearestPoint(v, out nearest, out var pointIndex, out distance);

            var st = Mathf.Clamp((int)pointIndex, 0, self.Count - 2);
            var en = Mathf.Clamp(Mathf.CeilToInt(pointIndex - 1), 0, self.Count - 2);

            HashSet<int> set = new HashSet<int> { st, en };

            var d = v - nearest;
            return set.Any(i => Vector2.Dot(self.GetEdgeNormal(i).Xz(), d.Xz()) >= 0f);
        }

        /// <summary>
        /// Wayを法線方向に沿って各頂点を補間しながら移動させる。
        /// 最初の頂点はstartOffset分だけ、最後の頂点はendOffset分だけ移動され、
        /// 間の頂点は線形補間されたオフセットとWayの頂点法線をつかってなるべく元の形状を維持するように移動される。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="startOffset"></param>
        /// <param name="endOffset"></param>
        public static void MoveLerpAlongNormal(this RnWay self, Vector3 startOffset, Vector3 endOffset)
        {
            if (self.IsValid == false)
                return;

            if (self.Count == 2)
            {
                self.GetPoint(0).Vertex += startOffset;
                self.GetPoint(1).Vertex += endOffset;
                return;
            }

            var index = 0;


            // 現在見る点と次の点の辺/頂点の法線を保存しておく
            // 線分の法線

            Vector3 EdgeNormal(int i)
            {
                return self.GetEdgeNormal(i);
            }

            var sLen = startOffset.magnitude;
            var eLen = endOffset.magnitude;
            var sDir = startOffset.normalized;
            var eDir = endOffset.normalized;
            // 始点と終点でベースラインをまたぐ場合があるので法線からの方向を記録しておく
            var sSign = Mathf.Sign(Vector3.Dot(startOffset, EdgeNormal(0)));
            var eSign = Mathf.Sign(Vector3.Dot(endOffset, EdgeNormal(self.Count - 2)));

            var edgeNormal = new[] { sDir * sSign, EdgeNormal(1) };
            // 頂点の法線
            var vertexNormal = new[] { edgeNormal[0], (edgeNormal[0] + edgeNormal[1]).normalized };
            var delta = 1f;


            var totalLength = self.CalcLength();
            var nowLength = 0f;

            var pointOffset = new Vector3[self.Count];
            pointOffset[0] = startOffset;
            pointOffset[self.Count - 1] = endOffset;
            for (var i = 0; i < self.Count - 1; ++i)
            {
                var en0 = edgeNormal[index];
                var en1 = edgeNormal[(index + 1) & 1];
                var vn = vertexNormal[index];

                // 形状維持するためにオフセット距離を変える
                // en0成分の移動量がdeltaになるように, vnの移動量を求める
                var m = Vector3.Dot(vn, en0);
                // p0->p1->p2でp0 == p2だったりした場合に0除算が発生するのでチェック
                var d = delta;
                bool isZero = Mathf.Abs(m) < 1e-5f;
                if (isZero == false)
                    d /= m;

                if (i < self.Count - 2)
                {
                    edgeNormal[index] = EdgeNormal(Mathf.Min(self.Count - 2, i + 1));
                    vertexNormal[index] = (edgeNormal[0] + edgeNormal[1]).normalized;
                    index = (index + 1) & 1;
                }

                if (i != 0)
                {
                    var p = nowLength / totalLength;
                    var l = Mathf.Lerp(sSign * sLen, eSign * eLen, p) * Mathf.Lerp(d, 1f, p);
                    pointOffset[i] = vn * l;
                }
                // 次の頂点計算のためにen1線分の移動量を入れる
                delta = d * Vector3.Dot(vn, en1);
                nowLength += (self[i + 1] - self[i]).magnitude;

            }

            for (var i = 0; i < self.Count; ++i)
            {
                self.GetPoint(i).Vertex += pointOffset[i];
            }
        }

        /// <summary>
        /// selfの先頭から線分に沿ってoffsetだけ進んだ点を返す.
        /// 線分の長さがoffsetより短い場合は最後の点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector3 GetAdvancedPointFromFront(this RnWay self, float offset, out int startIndex, out int endIndex)
        {
            if (self.IsReversed)
            {
                var ret = self.LineString.GetAdvancedPointFromBack(offset, out startIndex, out endIndex);
                startIndex = self.SwitchIndex(startIndex);
                endIndex = self.SwitchIndex(endIndex);
                return ret;
            }
            else
            {
                var ret = self.LineString.GetAdvancedPointFromFront(offset, out startIndex, out endIndex);
                startIndex = self.SwitchIndex(startIndex);
                return ret;
            }
        }

        /// <summary>
        /// selfの最後から線分に沿ってoffsetだけ進んだ点を返す.
        /// 線分の長さがoffsetより短い場合は先頭の点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offset"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static Vector3 GetAdvancedPointFromBack(this RnWay self, float offset, out int startIndex, out int endIndex)
        {
            if (self.IsReversed)
            {
                var ret = self.LineString.GetAdvancedPointFromFront(offset, out startIndex, out endIndex);
                startIndex = self.SwitchIndex(startIndex);
                endIndex = self.SwitchIndex(endIndex);
                return ret;
            }
            else
            {
                var ret = self.LineString.GetAdvancedPointFromBack(offset, out startIndex, out endIndex);
                startIndex = self.SwitchIndex(startIndex);
                endIndex = self.SwitchIndex(endIndex);
                return ret;
            }
        }

        /// <summary>
        /// selfの開始点(reverse=trueの時は終了点)から線分に沿ってoffsetだけ進んだ点を返す.
        /// 線分の長さがoffsetより短い場合は先頭の点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offset"></param>
        /// <param name="reverse"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static Vector3 GetAdvancedPoint(this RnWay self, float offset, bool reverse, out int startIndex, out int endIndex)
        {
            if (reverse)
                return self.GetAdvancedPointFromBack(offset, out startIndex, out endIndex);
            else
                return self.GetAdvancedPointFromFront(offset, out startIndex, out endIndex);
        }

        /// <summary>
        /// selfの開始点(reverse=trueの時は終了点)から線分に沿ってoffsetだけ進んだ点を返す.
        /// 線分の長さがoffsetより短い場合は先頭の点を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="offset"></param>
        /// <param name="reverse"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static Vector3 GetAdvancedPoint(this RnWay self, float offset, bool reverse)
        {
            return GetAdvancedPoint(self, offset, reverse, out _, out _);
        }

        /// <summary>
        /// 2D平面におけるRnway同士の距離を返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static float GetDistance2D(this RnWay self, RnWay other, AxisPlane plane = AxisPlane.Xz)
        {
            return self?.LineString?.GetDistance2D(other?.LineString, plane) ?? float.MaxValue;
        }
    }
}