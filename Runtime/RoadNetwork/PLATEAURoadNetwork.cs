using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;

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
                    var c = cell2Groups.FirstOrDefault(x => x.cellNo == cellNo);
                    if (c == null)
                    {
                        c = new Cell
                        {
                            cellNo = cellNo
                        };
                        cell2Groups.Add(c);
                    }
                    c.lanes.Add(lane.LaneIndex);
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
            var neighbors = lane.vertices.Select(GetNeighborLane).ToList();

            // 1頂点でしかつながっていないレーンは隣接していないので削除
            // 交差点で隣り合う道路道路は１頂点でつながっているが実際は中央の交差点
            // 以下のようにAとBは共通する1頂点があるが実際はそれぞれEとしかつながっていない
            //       A
            //       ||
            //   B = E = C
            //       ||  
            //       D
            lane.connectedLaneIndices
                .RemoveAll(c => neighbors.Count(n => n.Any(l => l.LaneIndex == c)) <= 1);

            foreach (var n in neighbors)
            {
                n.RemoveAll(a => lane.connectedLaneIndices.Contains(a.LaneIndex) == false);
            }

            // #TODO : 同じレーンが偶数回出てくる前提
            //       : 

            // 隣接レーンが一つもない場合は孤立
            if (neighbors.Any(n => n.Any()) == false)
            {
                var way = new PLATEAURoadNetworkWay();
                way.vertices.AddRange(lane.vertices);
                lane.ways.Add(way);
                return;
            }

            var startIndex = Enumerable.Range(0, neighbors.Count).First(i => neighbors[i].Any());

            List<int> indices = new List<int> { startIndex };
            foreach (var tmp in Enumerable.Range(1, neighbors.Count))
            {
                var i = (tmp + startIndex) % neighbors.Count;
                indices.Add(i);

                if (indices.Count <= 1)
                    continue;

                var toNeighbor = neighbors[i];
                // 隣接している点があったら切り替え
                if (toNeighbor.Any() == false)
                    continue;

                var fromIndex = indices[0];
                var fromNeighbors = neighbors[fromIndex];
                var neighbor = toNeighbor.FirstOrDefault(n => fromNeighbors.Contains(n));

                bool isEdge = false;
                if (neighbor != null)
                {
                    // 共通の隣接レーンがあるとエッジ
                    // ただし、行き止まり(1つのレーンとしか隣接していない)場合
                    //       全WayがEdge扱いになるので、道の外側が隣接レーンのポリゴンの範囲内かどうかもチェックする

                    var p1 = lane.vertices[indices[0]];
                    var p2 = lane.vertices[indices[1]];
                    var dir = (p2 - p1).PutY(0).normalized;
                    var cp = (p1 + p2) * 0.5f;

                    var n = -Vector3.Cross(Vector3.up, dir);
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
                    edge.vertices.AddRange(indices.Select(a => lane.vertices[a]));
                    lane.edges.Add(edge);
                }
                else
                {
                    var way = new PLATEAURoadNetworkWay();
                    way.prevLanes.AddRange(fromNeighbors.Select(n => n.LaneIndex));
                    way.nextLanes.AddRange(toNeighbor.Select(n => n.LaneIndex));
                    way.vertices.AddRange(indices.Select(a => lane.vertices[a]));
                    lane.ways.Add(way);
                }
                indices = new List<int> { i };
            }
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

        public List<PLATEAURoadNetworkWay> ways = new List<PLATEAURoadNetworkWay>();

        public List<PLATEAURoadNetworkEdge> edges = new List<PLATEAURoadNetworkEdge>();

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
    }

    [Serializable]
    public class PLATEAURoadNetworkEdge
    {
        public List<Vector3> vertices = new List<Vector3>();

        public int neighborLaneIndex = -1;
    }

    /// <summary>
    /// レーンを構成する１車線を表す
    /// </summary>
    [Serializable]
    public class PLATEAURoadNetworkWay
    {
        public List<Vector3> vertices = new List<Vector3>();

        public PLATEAURoadNetworkIndexMap nextLanes = new PLATEAURoadNetworkIndexMap();

        public PLATEAURoadNetworkIndexMap prevLanes = new PLATEAURoadNetworkIndexMap();
    }

}