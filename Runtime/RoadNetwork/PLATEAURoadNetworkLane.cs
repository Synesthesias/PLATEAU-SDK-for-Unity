using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork
{

    [Serializable]
    public class PLATEAURoadNetworkLane
    {
        // 自分自身を表すインデックス
        [SerializeField] private int laneIndex = -1;
        public int LaneIndex => laneIndex;

        [SerializeField] public PLATEAUCityObjectGroup cityObjectGroup;

        // 構成する頂点
        // ポリゴン的に時計回りの順に格納されている
        [SerializeField]
        public List<Vector3> vertices = new List<Vector3>();

        // 連結しているレーン
        [SerializeField]
        public PLATEAURoadNetworkIndexMap connectedLaneIndices = new PLATEAURoadNetworkIndexMap();

        [SerializeField]
        public List<PLATEAURoadNetworkWay> ways = new List<PLATEAURoadNetworkWay>();

        [SerializeField]
        public List<PLATEAURoadNetworkEdge> edges = new List<PLATEAURoadNetworkEdge>();

        // 中央線
        [SerializeField]
        public List<Vector3> centerLine = new List<Vector3>();

        // 交差点かどうか
        public bool IsIntersection => edges.Count > 2;

        // #TODO : 左側/右側の判断ができるのか
        // 左側Way
        public PLATEAURoadNetworkWay Left { get; set; }
        // 右側Way
        public PLATEAURoadNetworkWay Right { get; set; }

        // #TODO : 連結先/連結元の判断ができるのか？

        // 連結先レーン
        public List<PLATEAURoadNetworkLane> NextLanes { get; } = new List<PLATEAURoadNetworkLane>();

        // 連結元レーン
        public List<PLATEAURoadNetworkLane> PrevLanes { get; } = new List<PLATEAURoadNetworkLane>();

        public PLATEAURoadNetworkLane(int index, PLATEAUCityObjectGroup src)
        {
            laneIndex = index;
            cityObjectGroup = src;
        }

        public Vector3 GetDrawCenterPoint()
        {
            return vertices[0];
        }

        public void AddConnectedLane(int laneIndex)
        {
            connectedLaneIndices.Add(laneIndex);
        }

        // 頂点 startVertexIndex, startVertexIndex + 1の方向に対して
        // 道の外側を向いている法線ベクトルを返す. 正規化はされていない
        public Vector3 GetOutsizeNormal(int startVertexIndex)
        {
            var p0 = vertices[startVertexIndex];
            var p1 = vertices[(startVertexIndex + 1) % vertices.Count];
            // Vector3.Crossは左手系なので逆
            return -Vector3.Cross(Vector3.up, p1 - p0);
        }



        /// <summary>
        /// 道路の左右で方向をそろえる
        /// </summary>
        public void AlignWayDirection()
        {
            // Edgeを基準にそこを開始点にする
            // ただし、1度変更したWayは無視する(next/prevで必ず2回反転してしまうので)
            // #TODO : 通常の道だと2つのwayはprevLaneIndex/nextLaneIndexは同じ組だが交差点などwayが3つ以上ある場合に正しく同じ方向を向くかは検証できていない
            HashSet<int> visitedEdge = new HashSet<int>();
            foreach (var edge in edges)
            {
                // edgeを開始地点にする
                Assert.IsTrue(edge.neighborLaneIndex >= 0, $"edge.neighborLaneIndex {edge.neighborLaneIndex} >= 0");
                if (edge.neighborLaneIndex < 0)
                    continue;

                // edgeが次になっているWayを反転させる
                foreach (var way in ways
                             .Where(w => w.nextLaneIndex == edge.neighborLaneIndex && !visitedEdge
                                 .Contains(w.prevLaneIndex))
                        )
                {
                    way.vertices.Reverse();
                    (way.nextLaneIndex, way.prevLaneIndex) = (way.prevLaneIndex, way.nextLaneIndex);
                    way.isRightSide = !way.isRightSide;
                }
                visitedEdge.Add(edge.neighborLaneIndex);
            }
        }

        /// <summary>
        /// 中央線の構築
        /// </summary>
        public void BuildCenterLine()
        {
            centerLine.Clear();
            HashSet<int> visitedEdge = new HashSet<int>();
            for (var edgeIndex = 0; edgeIndex < 1; edgeIndex++)
            {
                if (visitedEdge.Contains(edgeIndex))
                    continue;
                visitedEdge.Add(edgeIndex);
                var edge = edges[edgeIndex];
                // edgeを開始地点にする
                Assert.IsTrue(edge.neighborLaneIndex >= 0, $"edge.neighborLaneIndex {edge.neighborLaneIndex} >= 0");
                if (edge.neighborLaneIndex < 0)
                    continue;

                // 
                if (edge.TryGetEdgeCenter(out Vector3 srcPoint))
                {
                    centerLine.Add(srcPoint);
                    var targetWays = ways.Where(w => w.prevLaneIndex == edge.neighborLaneIndex).ToList();

                    // #TODO : 一旦prev/nextが一致しているものだけが対象. (枝分かれしているような交差点のレーンは対象外)
                    if (targetWays.Count != 2 || targetWays[0].nextLaneIndex != targetWays[1].nextLaneIndex)
                        continue;

                    visitedEdge.Add(targetWays[0].nextLaneIndex);

                    // 0とCount-1はedge上なので1番からスタート
                    //var indices = Enumerable.Repeat(1, targetWays.Count).ToList();

                    bool TryGetNearestIntersection(PLATEAURoadNetworkWay way, int vertexIndex, out Vector3 intersection)
                    {
                        var v = way.vertices[vertexIndex];
                        var n1 = -way.GetOutsizeNormal(vertexIndex - 1).normalized;
                        var n2 = -way.GetOutsizeNormal(vertexIndex).normalized;
                        var n = (n1 + n2) / 2;
                        var ray = new Ray(v, n);
                        var found = targetWays
                            .Where(w => w != way)
                            .Select(w =>
                            {
                                var ret = w.HalfLineIntersectionXz(ray, out Vector3 inter);
                                return new { success = ret, inter = inter };
                            })
                            .Where(a => a.success)
                            .TryFindMin(a => (a.inter - ray.origin).sqrMagnitude, out var item);
                        intersection = Vector3.Lerp(v, item.inter, 0.5f);
                        return found;
                    }

                    var candidates = new List<Vector3>();
                    foreach (var way in targetWays)
                    {
                        for (var i = 1; i < way.vertices.Count - 1; ++i)
                        {
                            if (TryGetNearestIntersection(way, i, out var inter))
                                candidates.Add(inter);
                        }
                    }

                    while (candidates.Count > 0)
                    {
                        var before = centerLine.Last();
                        candidates.TryFindMin(x => (x - before).sqrMagnitude, out var nearPoint);
                        centerLine.Add(nearPoint);
                        candidates.Remove(nearPoint);
                    }

                    var lastEdge = edges.FirstOrDefault(e => e.neighborLaneIndex == targetWays[0].nextLaneIndex);
                    if (lastEdge != null)
                    {
                        if (lastEdge.TryGetEdgeCenter(out var end))
                            centerLine.Add(end);
                    }
                    //while (wayIndices
                    //       // Count-1は次のEdge上なので無視
                    //       .Where(i => indices[i] < targetWays[i].vertices.Count - 1)
                    //       .TryFindMin(i => (targetWays[i].vertices[indices[i]] - srcPoint).sqrMagnitude, out int wayIndex))
                    //{
                    //    var vertexIndex = indices[wayIndex]++;
                    //    List<Vector3> interSections = new List<Vector3>();


                    //    if (TryGetNearestIntersection(wayIndex, vertexIndex,  out var prevInter))
                    //    {
                    //        interSections.Add(prevInter);
                    //    }

                    //    if (interSections.Any() == false)
                    //        continue;
                    //    interSections.Sort((v1, v2) => Comparer<float>.Default.Compare((v1 - srcPoint).sqrMagnitude,
                    //        (v2 - srcPoint).sqrMagnitude));
                    //    centerLine.AddRange(interSections);
                    //    srcPoint = interSections.Last();
                    //}


                }
            }
        }
    }

}