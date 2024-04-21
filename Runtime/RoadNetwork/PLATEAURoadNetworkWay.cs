using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// レーンを構成する１車線を表す
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
        /// 頂点 vertexIndex, vertexIndex + 1の方向に対して
        /// 道の外側を向いている法線ベクトルを返す.正規化はされていない
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetOutsizeNormal(int vertexIndex)
        {
            var dir = vertices[vertexIndex + 1] - vertices[vertexIndex];
            // Vector3.Crossは左手系
            var ret = Vector3.Cross(Vector3.up, dir);
            return isRightSide ? ret : -ret;
        }

        /// <summary>
        /// Xz平面だけで見たときの最も近い交点を返す
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
    }
}