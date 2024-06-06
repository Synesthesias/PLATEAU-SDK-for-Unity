using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Factory
{
    [Serializable]
    public class RoadNetworkFactory
    {
        // --------------------
        // start:フィールド
        // --------------------

        // 同一頂点扱いにするセルサイズ
        [SerializeField] private float cellSize = 0.01f;

        // 中心線で分離するかどうか
        [SerializeField] private int splitLaneNum = 1;

        // 道路サイズ
        [SerializeField] private float roadSize = 3f;

        // 行き止まり検出判定時に同一直線と判断する角度の総和
        [SerializeField] private float terminateAllowEdgeAngle = 20f;

        // --------------------
        // end:フィールド
        // --------------------

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

        private enum RoadType
        {
            // 通常の道
            Road,
            // 交差点
            Intersection,
            // 行き止まり
            Terminate,
            // 孤島
            Isolated,
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

            // 対応するLink
            RoadNetworkLink Link { get; set; }
            // 対応するNode
            RoadNetworkNode Node { get; set; }

            public List<WayWork> Borders { get; } = new List<WayWork>();

            public List<WayWork> Ways { get; } = new List<WayWork>();

            public RoadType RoadType
            {
                get
                {
                    switch (Connected.Count())
                    {
                        case 0: return RoadType.Isolated;
                        case 1: return RoadType.Terminate;
                        case 2: return RoadType.Road;
                        default: return RoadType.Intersection;
                    }
                }
            }

            public void Bind(RoadNetworkLink link)
            {
                Link = link;
            }

            public void Bind(RoadNetworkNode node)
            {
                Node = node;
            }

            public void BuildConnection()
            {
                if (Link != null)
                {
                    var nextTrans = Ways.Select(w => w.NextBorder).Where(b => b != null).SelectMany(w => w.BothConnectedTrans).Distinct().ToList();
                    var prevTrans = Ways.Select(w => w.PrevBorder).Where(b => b != null).SelectMany(w => w.BothConnectedTrans).Distinct().ToList();
                    Link.NextNode = nextTrans.FirstOrDefault(t => t.Node != null)?.Node;
                    Link.PrevNode = prevTrans.FirstOrDefault(t => t.Node != null)?.Node;
                }
                else if (Node != null)
                {
                    foreach (var b in Borders)
                    {
                        foreach (var l in b.BothConnectedTrans)
                        {
                            if (l.Link == null)
                                continue;
                            Node.Neighbors.Add(new RoadNetworkNeighbor
                            {
                                Link = l.Link,
                                Border = b.Way
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Way
        /// </summary>
        private class WayWork
        {
            public RoadNetworkWay Way { get; private set; }

            /// <summary>
            /// 開始地点と隣接しているレーン
            /// </summary>
            public List<TranWork> PrevTranWorks { get; private set; }

            /// <summary>
            /// 終了地点と隣接しているレーン
            /// </summary>
            public List<TranWork> NextLaneWorks { get; private set; }

            /// <summary>
            /// 通常の道用. Wayの開始地点と接続している境界線
            /// </summary>
            public WayWork PrevBorder { get; set; }

            /// <summary>
            /// 通常の道用. Wayの終了地点と接続している境界線
            /// </summary>
            public WayWork NextBorder { get; set; }

            /// <summary>
            /// From/Toどっちにも含まれているレーン(接しているレーン)
            /// </summary>
            public IEnumerable<TranWork> BothConnectedTrans
            {
                get
                {
                    foreach (var l in PrevTranWorks)
                    {
                        if (NextLaneWorks.Contains(l))
                            yield return l;
                    }
                }
            }

            /// <summary>
            /// From/Toどっちかに含まれているレーン
            /// </summary>
            public IEnumerable<TranWork> AnyConnectedTrans => PrevTranWorks.Concat(NextLaneWorks).Distinct();


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
                        var neighbor = BothConnectedTrans.FirstOrDefault();
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
                PrevTranWorks = fromLaneWorks.ToList();
                NextLaneWorks = toLaneWorks.ToList();
            }

            /// <summary>
            /// 道を逆転させる
            /// </summary>
            public void Reverse()
            {
                Way = Way.ReversedWay();
                (PrevTranWorks, NextLaneWorks) = (NextLaneWorks, PrevTranWorks);
                (PrevBorder, NextBorder) = (NextBorder, PrevBorder);
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
            var vertex2Points = new Vertex2PointTable(cellSize, targets.SelectMany(v => v.Item2));
            var tranWorks = CreateTranWorks(targets, vertex2Points, out var cell2Groups);

            foreach (var tranWork in tranWorks)
            {
                Build(tranWork, ret);
            }
            foreach (var tranWork in tranWorks)
                tranWork.BuildConnection();
            //ret.DebugIdentify();
            return ret;
        }

        private void Build(TranWork tranWork, RoadNetworkModel ret)
        {
            var roadType = tranWork.RoadType;
            // 隣接レーンが一つもない場合は孤立
            if (roadType == RoadType.Isolated)
            {
                ret.AddLink(RoadNetworkLink.CreateIsolatedLink(tranWork.TargetTran, tranWork.LineString));
                return;
            }

            List<RoadNetworkWay> SplitWay()
            {
                var points = tranWork.LineString.Points;
                var ret = new List<List<RoadNetworkPoint>>();
                var startIndex = Enumerable.Range(0, points.Count).First(i => tranWork.Vertex2Connected[points[i]].Any());
                var wayVertexIndices = new List<RoadNetworkPoint> { points[startIndex] };
                foreach (var tmp in Enumerable.Range(1, points.Count))
                {
                    var i = (tmp + startIndex) % points.Count;
                    var p = points[i];
                    wayVertexIndices.Add(p);
                    if (wayVertexIndices.Count <= 1)
                        continue;
                    // 
                    var neighbors = tranWork.Vertex2Connected[points[i]];
                    // 隣接している点があったら切り替え
                    if (neighbors.Any() == false)
                        continue;

                    ret.Add(wayVertexIndices);
                    wayVertexIndices = new List<RoadNetworkPoint> { p };
                }
                return ret.Select(indices =>
                {
                    var lineString = RoadNetworkLineString.Create(indices);
                    return new RoadNetworkWay(lineString);
                }).ToList();
            }

            // wayを構成する頂点インデックスリスト
            var splitWays = SplitWay();

            foreach (var way in splitWays)
            {
                //var lineString = RoadNetworkLineString.Create(wayVertexIndices.Select(v => tranWork.LineString.Points[v]));
                //var way = new RoadNetworkWay(lineString);
                var toNeighbor = tranWork.Vertex2Connected[way.Points.Last()];
                var fromNeighbors = tranWork.Vertex2Connected[way.Points.First()];
                var wayWork = new WayWork(way, fromNeighbors, toNeighbor);
                if (wayWork.IsBorder)
                {
                    tranWork.Borders.Add(wayWork);
                }
                else
                {
                    tranWork.Ways.Add(wayWork);
                }
            }

            {
                var visitedWays = new HashSet<WayWork>();
                // 境界線の接続レーンとつながっているwayの方向をそろえる
                foreach (var l in tranWork.Borders.SelectMany(b => b.BothConnectedTrans).Distinct())
                {
                    // lとつながっているレーンを見に行く
                    foreach (var w in tranWork.Ways.Where(w => w.AnyConnectedTrans.Contains(l)))
                    {
                        if (visitedWays.Contains(w))
                            continue;

                        // From側につながっているものを一律で逆転させることで左右の方向をそろえる
                        if (w.PrevTranWorks.Contains(l) == false)
                        {
                            w.Reverse();
                        }

                        visitedWays.Add(w);
                    }
                }
            }

            // 境界情報を入れておく
            {
                foreach (var b in tranWork.Borders)
                {
                    foreach (var l in b.BothConnectedTrans)
                    {
                        foreach (var w in tranWork.Ways)
                        {
                            if (w.PrevTranWorks.Contains(l))
                                w.PrevBorder = b;
                            if (w.NextLaneWorks.Contains(l))
                                w.NextBorder = b;
                        }
                    }
                }
            }

            // もともと頂点が時計回りになっているのでIsReversed == trueの者が右になる
            var leftWay = tranWork.Ways.FirstOrDefault(w => w.Way.IsReversed == false);
            var rightWay = tranWork.Ways.FirstOrDefault(w => w.Way.IsReversed);

            // 通常の道
            if (leftWay != null && rightWay != null && leftWay?.PrevBorder == rightWay?.PrevBorder &&
                leftWay.NextBorder == rightWay?.NextBorder)
            {
                var startBorderWay = leftWay?.PrevBorder?.Way;
                var endBorderWay = leftWay?.NextBorder?.Way;
                var l = new RoadNetworkLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                var link = new RoadNetworkLink(tranWork.TargetTran);
                var startBorderLength = GeoGraphEx.GetEdges(startBorderWay?.Vertices ?? new List<Vector3>(), false)
                    .Sum(e => (e.Item2 - e.Item1).magnitude);
                var endBorderLength = GeoGraphEx.GetEdges(endBorderWay?.Vertices ?? new List<Vector3>(), false)
                    .Sum(e => (e.Item2 - e.Item1).magnitude);
                var num = (int)(Mathf.Min(startBorderLength, endBorderLength) / roadSize);
                num = Mathf.Min(2, num);
                if (l.IsValidWay && num > 1)
                {
                    var lanes = l.SplitLane(num);
                    foreach (var lane in lanes)
                        link.AddMainLane(lane);
                }
                else
                {
                    link.AddMainLane(l);
                }

                tranWork.Bind(link);
                ret.AddLink(link);
            }
            // 行き止まり
            else if (tranWork.Connected.Count() == 1)
            {
                var startBorderWay = leftWay?.PrevBorder?.Way;
                var endBorderWay = leftWay?.NextBorder?.Way;

                // どっちかはnot nullなはず
                var lWay = leftWay?.Way;
                var rWay = rightWay?.Way;
                var way = lWay ?? rWay;
                if (way != null)
                {
                    var vertices = way.Vertices.Select(x => x.Xz()).ToList();
                    var edgeIndices = GeoGraph2D.FindMidEdge(vertices, terminateAllowEdgeAngle);

                    RoadNetworkWay AsWay(IEnumerable<int> ind, bool isReverse, bool isRightSide)
                    {
                        var ls = RoadNetworkLineString.Create(ind.Select(x => way.GetPoint(x)));
                        return new RoadNetworkWay(ls, isReverse, isRightSide);
                    }

                    lWay = AsWay(Enumerable.Range(0, edgeIndices[0] + 1), false, false);
                    rWay = AsWay(Enumerable.Range(edgeIndices.Last(), way.Count - edgeIndices.Last()), true, true);
                    var borderWay = AsWay(edgeIndices, false, false);
                    if (startBorderWay == null)
                    {
                        startBorderWay = borderWay;
                    }
                    else
                    {
                        endBorderWay = borderWay;
                        (lWay, rWay) = (rWay.ReversedWay(), lWay.ReversedWay());
                    }
                }

                var l = new RoadNetworkLane(lWay, rWay, startBorderWay, endBorderWay);
                var link = RoadNetworkLink.CreateOneLaneLink(tranWork.TargetTran, l);
                tranWork.Bind(link);
                ret.AddLink(link);
            }
            // 交差点
            else if (tranWork.Connected.Count() >= 3)
            {
                var node = new RoadNetworkNode(tranWork.TargetTran);
                node.AddTracks(tranWork.Ways.Select(w => new RoadNetworkTrack(w.Way, null)));
                tranWork.Bind(node);
                ret.AddNode(node);
            }
        }

        private static List<TranWork> CreateTranWorks(IList<Tuple<PLATEAUCityObjectGroup, IList<Vector3>>> targets, Vertex2PointTable vertex2Points, out Dictionary<RoadNetworkPoint, Cell> cell2Groups)
        {
            cell2Groups = new Dictionary<RoadNetworkPoint, Cell>();
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
                    // 自分自身を除く
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

            return tranWorks;
        }
    }
}