﻿using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork
{
    public class PLATEAURoadNetwork : MonoBehaviour
    {
        [Serializable]
        public class Cell
        {
            // cell番号
            public Vector3Int cellNo;

            // このセルを参照するレーン一覧
            // #TODO : SortedListかHashSetを使いたいがシリアライズできないので仮
            public PLATEAURoadNetworkIndexMap lanes = new PLATEAURoadNetworkIndexMap();
        }

        [SerializeField] private float cellSize = 0.01f;

        [SerializeField] private List<Cell> cell2Groups = new List<Cell>();

        // レーンリスト
        [SerializeField] private List<PLATEAURoadNetworkLane> lanes = new List<PLATEAURoadNetworkLane>();

        public IEnumerable<PLATEAURoadNetworkLane> Lanes => lanes;

        private void BuildCellMap(IList<PLATEAUCityObjectGroup> cityObjectGroups)
        {
            // 頂点の一致判定のためにセル単位に切り捨て
            // #TODO : 近いけど隣のセルになる場合を考慮

            // レーンの初期化
            lanes = cityObjectGroups.Select((c, i) => new PLATEAURoadNetworkLane(i, c)).ToList();

            var cSize = Vector3.one * cellSize;
            cell2Groups.Clear();

            // レーンの頂点情報を構築
            foreach (var lane in lanes)
            {
                var comp = lane.cityObjectGroup.GetComponent<MeshCollider>();
                if (!comp)
                    continue;
                if (PolygonUtil.IsClockwise(comp.sharedMesh.vertices.Select(x => x.Xz())) == false)
                {
                    lane.vertices.AddRange(comp.sharedMesh.vertices);
                }
                else
                {
                    lane.vertices.AddRange(comp.sharedMesh.vertices.Reverse());
                }

                foreach (var v in lane.vertices)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var cell = cell2Groups.FirstOrDefault(x => x.cellNo == cellNo);
                    if (cell == null)
                    {
                        cell = new Cell
                        {
                            cellNo = cellNo
                        };
                        cell2Groups.Add(cell);
                    }
                    cell.lanes.Add(lane.LaneIndex);
                }
            }

            // laneのConnectedを構築
            foreach (var c in cell2Groups)
            {
                if (c.lanes.Count <= 1)
                    continue;
                foreach (var aIndex in c.lanes)
                {
                    foreach (var bIndex in c.lanes)
                    {
                        if (aIndex == bIndex)
                            continue;
                        var a = lanes[aIndex];
                        var b = lanes[bIndex];
                        a.connectedLaneIndices.Add(bIndex);
                        b.connectedLaneIndices.Add(aIndex);
                    }
                }
            }
        }

        public void CreateNetwork(IList<PLATEAUCityObjectGroup> targets)
        {
            BuildCellMap(targets);

            var cSize = Vector3.one * cellSize;

            foreach (var lane in lanes)
            {
                BuildLane(lane);
            }

        }
        public void BuildLane(PLATEAURoadNetworkLane lane)
        {
            var cSize = Vector3.one * cellSize;
            List<PLATEAURoadNetworkLane> GetNeighborLane(Vector3 v)
            {
                var cellNo = v.RevScaled(cSize).ToVector3Int();
                var c = cell2Groups.First(x => x.cellNo == cellNo);
                return c.lanes.Where(l => l != lane.LaneIndex).Select(l => lanes[l]).ToList();
            }
            // index番目の頂点と隣接しているレーン
            var vertex2Neighbors = lane.vertices.Select(GetNeighborLane).ToList();

            // 1頂点でしかつながっていないレーンは隣接していないので削除
            // 交差点で隣り合う道路道路は１頂点でつながっているが実際は中央の交差点
            // 以下のようにAとBは共通する1頂点があるが実際はそれぞれEとしかつながっていないので削除する
            //       A
            //       ||
            //   B = E = C
            //       ||  
            //       D
            lane.connectedLaneIndices
                .RemoveAll(c => vertex2Neighbors.Count(n => n.Any(l => l.LaneIndex == c)) <= 1);

            foreach (var n in vertex2Neighbors)
            {
                n.RemoveAll(a => lane.connectedLaneIndices.Contains(a.LaneIndex) == false);
            }

            // #TODO : 同じレーンが偶数回出てくる前提
            //       : 

            // 隣接レーンが一つもない場合は孤立
            if (vertex2Neighbors.Any(n => n.Any()) == false)
            {
                var way = new PLATEAURoadNetworkWay();
                way.vertices.AddRange(lane.vertices);
                lane.ways.Add(way);
                return;
            }

            var startIndex = Enumerable.Range(0, vertex2Neighbors.Count).First(i => vertex2Neighbors[i].Any());

            // wayを構成する頂点インデックスリスト
            List<int> wayVertexIndices = new List<int> { startIndex };
            foreach (var tmp in Enumerable.Range(1, vertex2Neighbors.Count))
            {
                var i = (tmp + startIndex) % vertex2Neighbors.Count;
                wayVertexIndices.Add(i);
                if (wayVertexIndices.Count <= 1)
                    continue;
                // 
                var toNeighbor = vertex2Neighbors[i];
                // 隣接している点があったら切り替え
                if (toNeighbor.Any() == false)
                    continue;

                var fromIndex = wayVertexIndices[0];
                var fromNeighbors = vertex2Neighbors[fromIndex];
                var neighbor = toNeighbor.FirstOrDefault(n => fromNeighbors.Contains(n));

                bool isEdge = false;
                if (neighbor != null)
                {
                    // 共通の隣接レーンがあるとエッジ
                    // ただし、行き止まり(1つのレーンとしか隣接していない)場合
                    //       全WayがEdge扱いになるので、道の外側が隣接レーンのポリゴンの範囲内かどうかもチェックする
                    var p0 = lane.vertices[wayVertexIndices[0]];
                    var p1 = lane.vertices[wayVertexIndices[1]];
                    var cp = (p0 + p1) * 0.5f;

                    var n = lane.GetOutsizeNormal(wayVertexIndices[0]).normalized;
                    // 微小にずらして確認する
                    var p = cp + n * 0.1f;
                    isEdge = PolygonUtil.Contains(neighbor.vertices.Select(x => x.Xz()), p.Xz());
                }

                if (isEdge)
                {
                    var edge = new PLATEAURoadNetworkEdge
                    {
                        neighborLaneIndex = neighbor.LaneIndex
                    };
                    edge.vertices.AddRange(wayVertexIndices.Select(a => lane.vertices[a]));
                    lane.edges.Add(edge);
                }
                else
                {

                    Assert.IsTrue(fromNeighbors.Count == 1, $"fromNeighborsCount {fromNeighbors.Count}");
                    Assert.IsTrue(toNeighbor.Count == 1, $"toNeighborCount {toNeighbor.Count}");
                    var way = new PLATEAURoadNetworkWay();
                    way.prevLaneIndex = fromNeighbors[0].LaneIndex;
                    way.nextLaneIndex = toNeighbor[0].LaneIndex;
                    way.vertices.AddRange(wayVertexIndices.Select(a => lane.vertices[a]));
                    lane.ways.Add(way);
                }
                wayVertexIndices = new List<int> { i };
            }

            // 左右のWayの方向をそろえる
            lane.AlignWayDirection();
            lane.BuildCenterLine();
        }
    }

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

    [Serializable]
    public class PLATEAURoadNetworkEdge
    {
        public List<Vector3> vertices = new List<Vector3>();

        public int neighborLaneIndex = -1;

        public bool TryGetEdgeCenter(out Vector3 midPoint)
        {
            return PolygonUtil.TryGetLineSegmentMidPoint(vertices, out midPoint);
        }
    }

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