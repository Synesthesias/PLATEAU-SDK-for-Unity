using PlasticPipe.PlasticProtocol.Messages;
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
    /// レーンを構成する左右の道の一つ
    /// </summary>
    [Serializable]
    public class RoadNetworkWay : IReadOnlyList<Vector3>
    {
        // LineStringの向きが逆かどうか
        public bool IsReversed { get; set; } = false;

        // 頂点
        public RoadNetworkLineString LineString { get; private set; }

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    yield return this[i];
            }
        }

        // 頂点数
        public int Count => LineString?.Count ?? 0;

        public RoadNetworkWay(RoadNetworkLineString lineString, bool isReversed = false)
        {
            LineString = lineString;
            IsReversed = isReversed;
        }

        public RoadNetworkWay ReversedWay()
        {
            return new RoadNetworkWay(LineString, !IsReversed);
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
                if (IsReversed == false)
                    return LineString.Vertices[index];
                // 0 <= index < LineString.Vertices.Count前提なのでmodとったりしない
                return LineString.Vertices[LineString.Vertices.Count - 1 - index];
            }
        }

        /// <summary>
        /// 頂点 vertexIndex -> vertexIndex, vertexIndex -> vertexIndex + 1の方向に対して
        /// 道の外側を向いている法線ベクトルの平均を返す.正規化はされていない
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertexIndex)
        {
            var n1 = GetEdgeNormal(Math.Min(vertexIndex, Count - 2)).normalized;
            var n2 = GetEdgeNormal(Math.Max(vertexIndex - 1, 0)).normalized;

            // 境界地の時はそのままの値を使うようにする. vertexIndex自体が範囲外の時は例外にする
            Vector3 ret;
            if (vertexIndex == Count - 1)
                ret = n2;
            else if (vertexIndex == 0)
                ret = n1;
            else
                ret = (n1 + n2) / 2;
            return ret;
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
            var p1 = this[(startVertexIndex + 1) % Count];
            // Vector3.Crossは左手系なので逆
            return -Vector3.Cross(Vector3.up, p1 - p0);
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
        /// 境界線の中央の点を返す
        /// </summary>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public int TryGetCenterVertex(out Vector3 midPoint)
        {
            return LineUtil.TryGetLineSegmentMidPoint(LineString?.Vertices ?? new List<Vector3>(), out midPoint);
        }

        /// <summary>
        /// 自身をnum分割して返す. 分割できない(頂点空）の時は空リストを返す
        /// </summary>
        /// <returns></returns>
        public List<RoadNetworkWay> Split(int num)
        {
            return LineString.Split(num).Select(s => new RoadNetworkWay(s, IsReversed)).ToList();
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}