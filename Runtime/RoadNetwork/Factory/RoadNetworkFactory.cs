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

namespace PLATEAU.RoadNetwork.Factory
{
    [Serializable]
    public class RoadNetworkFactory
    {
        [Serializable]
        private class Cell : IReadOnlyList<TranWork>
        {
            // このセルを参照するレーン一覧
            private List<TranWork> trans = new List<TranWork>();

            public void Add(TranWork tran)
            {
                if (trans.Contains(tran))
                    return;
                trans.Add(tran);
            }

            public TranWork this[int index]
            {
                get
                {
                    return trans[index];
                }
            }

            public IEnumerator<TranWork> GetEnumerator()
            {
                return trans.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => trans.Count;
        }

        /// <summary>
        /// tranオブジェクトに紐づく Link or Node情報
        /// </summary>
        private class TranWork
        {
            public PLATEAUCityObjectGroup TargetTran { get; set; }

            public RoadNetworkLineString LineString { get; set; } = new RoadNetworkLineString();

            public IEnumerable<TranWork> Connected => Vertex2Connected.SelectMany(v => v.Value).Distinct();

            // 頂点 -> 接続TranWork情報
            public Dictionary<RoadNetworkPoint, List<TranWork>> Vertex2Connected { get; } =
                new Dictionary<RoadNetworkPoint, List<TranWork>>();
        }

        private class WayWork
        {
            public RoadNetworkWay Way { get; set; }

            /// <summary>
            /// 開始地点と隣接しているレーン
            /// </summary>
            public List<TranWork> FromLaneWorks { get; private set; }

            /// <summary>
            /// 終了地点と隣接しているレーン
            /// </summary>
            public List<TranWork> ToLaneWorks { get; private set; }

            public WayWork FromBorder { get; set; }

            public WayWork ToBorder { get; set; }

            /// <summary>
            /// From/Toどっちにも含まれているレーン(接しているレーン)
            /// </summary>
            public IEnumerable<TranWork> BothConnectedLanes
            {
                get
                {
                    foreach (var l in FromLaneWorks)
                    {
                        if (ToLaneWorks.Contains(l))
                            yield return l;
                    }
                }
            }

            /// <summary>
            /// From/Toどっちかに含まれているレーン
            /// </summary>
            public IEnumerable<TranWork> AnyConnectedLanes => FromLaneWorks.Concat(ToLaneWorks).Distinct();


            private bool? cachedIsBorder = null;
            /// <summary>
            /// 境界線かどうか
            /// </summary>
            public bool IsBorder
            {
                get
                {
                    if (cachedIsBorder.HasValue == false)
                    {
                        cachedIsBorder = false;
                        var neighbor = BothConnectedLanes.FirstOrDefault();
                        if (neighbor != null)
                        {
                            // 共通の隣接レーンがあると境界線
                            // ただし、行き止まり(1つのレーンとしか隣接していない)場合
                            //       全WayがBorder扱いになるので、道の外側が隣接レーンのポリゴンの範囲内かどうかもチェックする
                            var p0 = Way[0];
                            var p1 = Way[1];
                            var cp = (p0 + p1) * 0.5f;

                            var n = Way.GetEdgeNormal(0);
                            // 微小にずらして確認する
                            var p = cp + n * 0.1f;
                            cachedIsBorder = GeoGraph2D.Contains(neighbor.LineString.Select(x => x.Xz()), p.Xz());
                        }
                    }

                    return cachedIsBorder.Value;
                }
            }

            public WayWork(RoadNetworkWay way, List<TranWork> fromLaneWorks, List<TranWork> toLaneWorks)
            {
                Way = way;
                FromLaneWorks = fromLaneWorks.ToList();
                ToLaneWorks = toLaneWorks.ToList();
            }

            /// <summary>
            /// 道を逆転させる
            /// </summary>
            public void Reverse()
            {
                Way = Way.ReversedWay();
                (FromLaneWorks, ToLaneWorks) = (ToLaneWorks, FromLaneWorks);
                (FromBorder, ToBorder) = (ToBorder, FromBorder);
            }
        }


        /// <summary>
        /// 頂点 -> ポイント変換を行う
        /// </summary>
        private class Vertex2PointTable
        {
            public float CellSize { get; }

            private Dictionary<Vector3, RoadNetworkPoint> Table { get; } = new Dictionary<Vector3, RoadNetworkPoint>();

            public Vertex2PointTable(float cellSize, IEnumerable<Vector3> vertices)
            {
                // 頂点の一致判定のためにセル単位に切り捨て
                // #TODO : 近いけど隣のセルになる場合を考慮
                CellSize = cellSize;
                var cSize = Vector3.one * cellSize;

                var table = new Dictionary<Vector3Int, RoadNetworkPoint>();
                foreach (var v in vertices)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var point = table.GetValueOrCreate(cellNo, c => new RoadNetworkPoint(v));
                    Table[v] = point;
                }
            }

            public RoadNetworkPoint this[Vector3 v]
            {
                get
                {
                    return Table[v];
                }
            }
        }

        // 同一頂点扱いにするセルサイズ
        [SerializeField] private float cellSize = 0.01f;

        // 中心線で分離するかどうか
        [SerializeField] private bool splitCenterLine = false;


        public RoadNetworkModel CreateNetwork(IList<PLATEAUCityObjectGroup> targets)
        {
            var meshes = targets
                .Select(c => new { c = c, col = c.GetComponent<MeshCollider>() })
                .Where(c => c.col)
                .Select(c => new Tuple<PLATEAUCityObjectGroup, IList<Vector3>>(c.c, c.col.sharedMesh.vertices))
                .ToList();
            return CreateNetwork(meshes);
        }

        /// <summary>
        /// targets -> ポリゴンのリスト
        /// </summary>
        /// <param name="targets"></param>
        /// <returns></returns>
        public RoadNetworkModel CreateNetwork(IList<Tuple<PLATEAUCityObjectGroup, IList<Vector3>>> targets)
        {
            // レーンの初期化
            var ret = new RoadNetworkModel();
            var cell2Groups = new Dictionary<RoadNetworkPoint, Cell>();
            var vertex2Points = new Vertex2PointTable(cellSize, targets.SelectMany(v => v.Item2));
            // レーンの頂点情報を構築
            var tranWorks = new List<TranWork>();
            foreach (var item in targets)
            {
                var cityObject = item.Item1;
                var linkOrNodeWork = new TranWork { TargetTran = cityObject };

                //var vertices = laneWork.LineString.Vertices;
                var vertices = item.Item2.Select(v => vertex2Points[v]).ToList();
                // 時計回りになるように順番チェック
                if (GeoGraph2D.IsClockwise(vertices.Select(x => x.Vertex.Xz())) == false)
                    vertices.Reverse();

                foreach (var v in vertices)
                {
                    var cell = cell2Groups.GetValueOrCreate(v, v => new Cell { });
                    cell.Add(linkOrNodeWork);
                }
                linkOrNodeWork.LineString = RoadNetworkLineString.Create(vertices);
                tranWorks.Add(linkOrNodeWork);
            }

            // laneのConnectedを構築
            // 特定頂点につながっているLinkNodeは接続情報を付与する
            foreach (var tranWork in tranWorks)
            {
                // 頂点に対してつながっているTranを追加
                foreach (var p in tranWork.LineString.Points)
                {
                    var connected = cell2Groups[p];
                    tranWork.Vertex2Connected[p] = connected.Where(x => x != tranWork).ToList();
                }

                // 1頂点でしかつながっていないレーンは隣接していないので削除
                // 交差点で隣り合う道路道路は１頂点でつながっているが実際は中央の交差点
                // 以下のようにAとBは共通する1頂点があるが実際はそれぞれEとしかつながっていない
                //        A
                //       | |
                //       | |
                //    ---o o ---
                //   B    E   C
                //    ---o o ---
                //       | |
                //       | |
                //        D

                // 1頂点以下でしか接していないtranは削除
                var removeNeighbor = tranWork.Vertex2Connected.SelectMany(v => v.Value)
                    .GroupBy(v => v)
                    .Where(v => v.Count() <= 1)
                    .Select(v => v.Key)
                    .ToList();
                foreach (var n in tranWork.Vertex2Connected)
                    n.Value.RemoveAll(x => removeNeighbor.Contains(x));
            }

            foreach (var tranWork in tranWorks)
            {
                // index番目の頂点と隣接しているレーン
                var vertex2Neighbors = tranWork
                    .LineString
                    .Points
                    .Select(p => tranWork.Vertex2Connected[p])
                    .ToList();

                // 隣接レーンが一つもない場合は孤立
                if (vertex2Neighbors.Any(n => n.Any()) == false)
                {
                    ret.Links.Add(RoadNetworkLink.CreateIsolatedLink(tranWork.TargetTran, tranWork.LineString));
                    continue;
                }

                List<List<int>> SplitWay()
                {
                    var ret = new List<List<int>>();
                    var startIndex = Enumerable.Range(0, vertex2Neighbors.Count).First(i => vertex2Neighbors[i].Any());
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

                        ret.Add(wayVertexIndices);
                        wayVertexIndices = new List<int> { i };
                    }

                    return ret;
                }

                // wayを構成する頂点インデックスリスト
                var splitWays = SplitWay();

                // 境界線
                var borderWorks = new List<WayWork>();
                // 車線端線
                var wayWorks = new List<WayWork>();
                foreach (var wayVertexIndices in splitWays)
                {
                    var lineString = RoadNetworkLineString.Create(wayVertexIndices.Select(v => tranWork.LineString.Points[v]));
                    var way = new RoadNetworkWay(lineString);
                    var toNeighbor = vertex2Neighbors[wayVertexIndices.Last()];
                    var fromNeighbors = vertex2Neighbors[wayVertexIndices.First()];
                    var wayWork = new WayWork(way, fromNeighbors, toNeighbor);
                    if (wayWork.IsBorder)
                    {
                        borderWorks.Add(wayWork);
                    }
                    else
                    {
                        wayWorks.Add(wayWork);
                    }
                }

                {
                    var visitedWays = new HashSet<WayWork>();
                    // 境界線の接続レーンとつながっているwayの方向をそろえる
                    foreach (var l in borderWorks.SelectMany(b => b.BothConnectedLanes).Distinct())
                    {
                        // lとつながっているレーンを見に行く
                        foreach (var w in wayWorks.Where(w => w.AnyConnectedLanes.Contains(l)))
                        {
                            if (visitedWays.Contains(w))
                                continue;

                            // From側につながっているものを一律で逆転させることで左右の方向をそろえる
                            if (w.FromLaneWorks.Contains(l) == false)
                            {
                                w.Reverse();
                            }
                            visitedWays.Add(w);
                        }
                    }
                }

                // 境界情報を入れておく
                {
                    foreach (var b in borderWorks)
                    {
                        foreach (var l in b.BothConnectedLanes)
                        {
                            foreach (var w in wayWorks)
                            {
                                if (w.FromLaneWorks.Contains(l))
                                    w.FromBorder = b;
                                if (w.ToLaneWorks.Contains(l))
                                    w.ToBorder = b;
                            }
                        }
                    }
                }

                // もともと頂点が時計回りになっているのでIsReversed == trueの者が右になる
                var leftWay = wayWorks.FirstOrDefault(w => w.Way.IsReversed == false);
                var rightWay = wayWorks.FirstOrDefault(w => w.Way.IsReversed);

                // 通常の道
                if (leftWay != null && rightWay != null && leftWay?.FromBorder == rightWay?.FromBorder && leftWay.ToBorder == rightWay?.ToBorder)
                {

                    var startBorderWay = leftWay?.FromBorder?.Way;
                    var endBorderWay = leftWay?.ToBorder?.Way;
                    var l = new RoadNetworkLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                    var link = new RoadNetworkLink(tranWork.TargetTran);
                    if (l.IsBothConnectedLane && splitCenterLine)
                    {
                        var lanes = l.SplitLane(2);
                        if (lanes.Count > 1)
                        {
                            lanes[1].Reverse();
                        }
                        link.MainLanes.AddRange(lanes);
                    }
                    else
                    {
                        link.MainLanes.Add(l);
                    }
                    ret.Links.Add(link);
                }
                // 行き止まり
                else if (tranWork.Connected.Count() == 1)
                {
                    var startBorderWay = leftWay?.FromBorder?.Way;
                    var endBorderWay = leftWay?.ToBorder?.Way;
                    var l = new RoadNetworkLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                    ret.Links.Add(RoadNetworkLink.CreateOneLaneLink(tranWork.TargetTran, l));
                }
                // 交差点
                else if (tranWork.Connected.Count() >= 3)
                {
                    var node = new RoadNetworkNode(tranWork.TargetTran);
                    ret.Nodes.Add(node);
                }
            }

            //ret.DebugIdentify();
            return ret;
        }

    }
}