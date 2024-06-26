﻿using PLATEAU.RoadNetwork.Data;
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
    public class RoadNetworkWay : ARoadNetworkParts<RoadNetworkTrack>, IReadOnlyList<Vector3>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // LineStringの向きが逆かどうか
        public bool IsReversed { get; set; } = false;

        // 法線計算用. 進行方向左側が道かどうか
        public bool IsRightSide { get; set; } = false;

        // 頂点
        public RoadNetworkLineString LineString { get; private set; }

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
        public IEnumerable<RoadNetworkPoint> Points
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    yield return GetPoint(i);
            }
        }

        public RoadNetworkPoint GetPoint(int index)
        {
            // 負数の時は逆からのインデックスに変換
            if (index < 0)
                index = Count + index;
            return LineString.Points[ToRawIndex(index)];
        }

        // 頂点数
        public int Count => LineString?.Count ?? 0;

        // 有効な道かどうか
        public bool IsValid => LineString?.IsValid ?? false;

        public RoadNetworkWay(RoadNetworkLineString lineString, bool isReversed = false, bool isRightSide = false)
        {
            LineString = lineString;
            IsReversed = isReversed;
            IsRightSide = isRightSide;
        }

        // デシリアライズのために必要
        public RoadNetworkWay() { }

        public RoadNetworkWay ReversedWay()
        {
            return new RoadNetworkWay(LineString, !IsReversed, !IsRightSide);
        }

        /// <summary>
        /// Reversedを考慮したインデックスへ変換する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int ToRawIndex(int index)
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
        }

        /// <summary>
        /// 頂点 vertexIndex -> vertexIndex, vertexIndex -> vertexIndex + 1の方向に対して
        /// 道の外側を向いている法線ベクトルの平均を返す.正規化はされていない
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertexIndex)
        {
            // 頂点数1の時は不正値を返す
            if (Count <= 1)
                return Vector3.zero;
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
            return IsRightSide ? -ret : ret;
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
        /// 線分の距離をp : (1-p)で分割した点をmidPointに入れて返す. 戻り値は midPointを含む線分のインデックス(i ~ i+1の線分上にmidPointがある) 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="midPoint"></param>
        /// <returns></returns>
        public int GetLerpPoint(float p, out Vector3 midPoint)
        {
            return LineUtil.GetLineSegmentLerpPoint(this, p, out midPoint);
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


    public static class RoadNetworkWayEx
    {
        public static IEnumerable<LineSegment2D> GetEdges2D(this RoadNetworkWay self)
        {
            if (self == null)
                yield break;
            foreach (var e in GeoGraphEx.GetEdges(self.Vertices.Select(x => x.Xz()), false))
                yield return new LineSegment2D(e.Item1, e.Item2);
        }
    }
}