using PLATEAU.CityInfo;
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


}