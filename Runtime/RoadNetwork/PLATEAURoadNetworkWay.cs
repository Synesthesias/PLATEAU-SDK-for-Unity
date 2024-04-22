using PlasticPipe.PlasticProtocol.Messages;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// レーンを構成する左右の道の一つ
    /// </summary>
    [Serializable]
    public class PLATEAURoadNetworkWay
    {
        public List<Vector3> vertices = new List<Vector3>();
        public int nextLaneIndex = -1;
        public int prevLaneIndex = -1;
        // レーンの右サイドの道
        public bool isRightSide = false;

        /// <summary>
        /// 頂点 vertexIndex -> vertexIndex, vertexIndex -> vertexIndex + 1の方向に対して
        /// 道の外側を向いている法線ベクトルの平均を返す.正規化はされていない
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertexIndex)
        {
            var next = Math.Min(vertexIndex + 1, vertices.Count - 1);
            var prev = Math.Max(vertexIndex - 1, 0);
            // Vector3.Crossは左手系
            var n1 = Vector3.Cross(Vector3.up, vertices[next] - vertices[vertexIndex]).normalized;
            var n2 = Vector3.Cross(Vector3.up, vertices[vertexIndex] - vertices[prev]).normalized;

            // 境界地の時はそのままの値を使うようにする. vertexIndex自体が範囲外の時は例外にする
            if (vertexIndex == next)
                return n2;
            if (vertexIndex == prev)
                return n1;
            var ret = (n1 + n2) / 2;
            return isRightSide ? ret : -ret;
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
            for (var i = 0; i < vertices.Count - 1; ++i)
            {
                var p1 = vertices[i];
                var p2 = vertices[i + 1];
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
            for (var i = 0; i < vertices.Count - 1; ++i)
            {
                var p1 = vertices[i];
                var p2 = vertices[i + 1];
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
    }
}