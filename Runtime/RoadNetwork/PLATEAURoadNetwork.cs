using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
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
            public SerializableHashSet<RnLaneId> lanes = new SerializableHashSet<RnLaneId>();

        }

        [SerializeField] private float cellSize = 0.01f;

        [SerializeField] private List<Cell> cell2Groups = new List<Cell>();

        // レーンリスト
        [SerializeField] private List<RoadNetworkLane> lanes = new List<RoadNetworkLane>();


        public IEnumerable<RoadNetworkLane> Lanes => lanes;

        private void BuildCellMap(IList<PLATEAUCityObjectGroup> cityObjectGroups)
        {

            var cSize = Vector3.one * cellSize;

            Dictionary<Vector3Int, RnPointId> cell2PointId = new Dictionary<Vector3Int, RnPointId>();
            List<RoadNetworkPoint> vertices = new List<RoadNetworkPoint>();

            foreach (var g in cityObjectGroups
                         .Select(c => c.GetComponent<MeshCollider>())
                         .SelectMany(c => c.sharedMesh.vertices)
                         .GroupBy(v => v.RevScaled(cSize).ToVector3Int()))
            {
                var key = g.Key;
                // とりあえずマージした頂点の平均を代表頂点にする
                var vertex = g.Aggregate(Vector3.zero, (a, v) => a + v) / g.Count();
                cell2PointId[key] = new RnPointId(vertices.Count);
                vertices.Add(new RoadNetworkPoint(vertex));
            }




            // 頂点の一致判定のためにセル単位に切り捨て
            // #TODO : 近いけど隣のセルになる場合を考慮

            // レーンの初期化
            lanes = cityObjectGroups.Select((c, i) => new RoadNetworkLane(i, c)).ToList();

            cell2Groups.Clear();



            // レーンの頂点情報を構築
            foreach (var lane in lanes)
            {
                var comp = lane.cityObjectGroup.GetComponent<MeshCollider>();
                if (!comp)
                    continue;
                var cells = comp.sharedMesh.vertices.Select(v => v.RevScaled(cSize).ToVector3Int())
                    .Select(v => cell2PointId[v])
                    .ToList();
                if (GeoGraph2d.IsClockwise(comp.sharedMesh.vertices.Select(x => x.Xz())) == false)
                {
                    lane.vertices.AddRange(cells);
                }
                else
                {
                    lane.vertices.AddRange(((IEnumerable<RnPointId>)cells).Reverse());
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
        public void BuildLane(RoadNetworkLane lane)
        {
            var cSize = Vector3.one * cellSize;
            List<RoadNetworkLane> GetNeighborLane(Vector3 v)
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
                var way = new RoadNetworkWay();
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

                bool isBorder = false;
                if (neighbor != null)
                {
                    // 共通の隣接レーンがあるとエッジ
                    // ただし、行き止まり(1つのレーンとしか隣接していない)場合
                    //       全WayがBorder扱いになるので、道の外側が隣接レーンのポリゴンの範囲内かどうかもチェックする
                    var p0 = lane.vertices[wayVertexIndices[0]];
                    var p1 = lane.vertices[wayVertexIndices[1]];
                    var cp = (p0 + p1) * 0.5f;

                    var n = lane.GetEdgeNormal(wayVertexIndices[0]).normalized;
                    // 微小にずらして確認する
                    var p = cp + n * 0.1f;
                    isBorder = GeoGraph2d.Contains(neighbor.vertices.Select(x => x.Xz()), p.Xz());
                }

                if (isBorder)
                {
                    var border = new RoadNetworkBorder
                    {
                        neighborLaneIndex = neighbor.LaneIndex
                    };
                    border.vertices.AddRange(wayVertexIndices.Select(a => lane.vertices[a]));
                    lane.borders.Add(border);
                }
                else
                {
                    // 孤立した状態だとneighborsが0の時もあり得る
                    Assert.IsTrue(fromNeighbors.Count <= 1, $"fromNeighborsCount {fromNeighbors.Count}");
                    Assert.IsTrue(toNeighbor.Count <= 1, $"toNeighborCount {toNeighbor.Count}");
                    var way = new RoadNetworkWay();
                    way.prevLaneIndex = fromNeighbors.FirstOrDefault()?.LaneIndex ?? -1;
                    way.nextLaneIndex = toNeighbor.FirstOrDefault()?.LaneIndex ?? -1;
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