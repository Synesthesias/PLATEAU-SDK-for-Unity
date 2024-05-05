using Codice.Client.BaseCommands.Differences;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
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
        private class Cell
        {
            // このセルを参照するレーン一覧
            public HashSet<LaneWork> lanes = new HashSet<LaneWork>();
        }

        /// <summary>
        /// レーン情報
        /// </summary>
        private class LaneWork
        {
            public int id;
            public RoadNetworkLineString LineString { get; } = new RoadNetworkLineString();

            public HashSet<LaneWork> ConnectedLanes { get; } = new HashSet<LaneWork>();
        }

        private class WayWork
        {
            public RoadNetworkWay Way { get; set; }

            /// <summary>
            /// 開始地点と隣接しているレーン
            /// </summary>
            public List<LaneWork> FromLaneWorks { get; private set; }

            /// <summary>
            /// 終了地点と隣接しているレーン
            /// </summary>
            public List<LaneWork> ToLaneWorks { get; private set; }

            public WayWork FromBorder { get; set; }

            public WayWork ToBorder { get; set; }

            /// <summary>
            /// From/Toどっちにも含まれているレーン(接しているレーン)
            /// </summary>
            public IEnumerable<LaneWork> BothConnectedLanes
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
            public IEnumerable<LaneWork> AnyConnectedLanes => FromLaneWorks.Concat(ToLaneWorks).Distinct();


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
                            cachedIsBorder = GeoGraph2d.Contains(neighbor.LineString.Vertices.Select(x => x.Xz()), p.Xz());
                        }
                    }

                    return cachedIsBorder.Value;
                }
            }

            public WayWork(RoadNetworkWay way, List<LaneWork> fromLaneWorks, List<LaneWork> toLaneWorks)
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
                Way.IsReversed = !Way.IsReversed;
                (FromLaneWorks, ToLaneWorks) = (ToLaneWorks, FromLaneWorks);
                (FromBorder, ToBorder) = (ToBorder, FromBorder);
            }
        }

        [SerializeField] private float cellSize = 0.01f;

        [SerializeField] private bool splitCenterLine = false;


        public RoadNetworkModel CreateNetwork(IList<PLATEAUCityObjectGroup> targets)
        {
            var meshes = targets
                .Select(c => c.GetComponent<MeshCollider>())
                .Where(c => c)
                .Select(c => c.sharedMesh.vertices)
                .Cast<IList<Vector3>>()
                .ToList();
            return CreateNetwork(meshes);
        }

        /// <summary>
        /// targets -> ポリゴンのリスト
        /// </summary>
        /// <param name="targets"></param>
        /// <returns></returns>
        public RoadNetworkModel CreateNetwork(IList<IList<Vector3>> targets)
        {
            var cSize = Vector3.one * cellSize;
            // 頂点の一致判定のためにセル単位に切り捨て
            // #TODO : 近いけど隣のセルになる場合を考慮

            // レーンの初期化

            var ret = new RoadNetworkModel();
            var cell2Groups = new Dictionary<Vector3Int, Cell>();

            // レーンの頂点情報を構築
            var laneWorks = new List<LaneWork>();
            foreach (var polygon in targets)
            {
                var laneWork = new LaneWork { id = laneWorks.Count, };

                var vertices = laneWork.LineString.Vertices;
                // 時計回りになるように順番チェック
                if (GeoGraph2d.IsClockwise(polygon.Select(x => x.Xz())) == false)
                {
                    vertices.AddRange(polygon);
                }
                else
                {
                    vertices.AddRange(polygon.Reverse());
                }

                foreach (var v in vertices)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var cell = cell2Groups.GetValueOrCreate(cellNo, v => new Cell { });
                    cell.lanes.Add(laneWork);
                }

                laneWorks.Add(laneWork);
            }

            // laneのConnectedを構築
            foreach (var c in cell2Groups.Values)
            {
                if (c.lanes.Count <= 1)
                    continue;
                foreach (var a in c.lanes)
                {
                    foreach (var b in c.lanes)
                    {
                        if (a == b)
                            continue;
                        a.ConnectedLanes.Add(b);
                        b.ConnectedLanes.Add(a);
                    }
                }
            }

            foreach (var laneWork in laneWorks)
            {
                List<LaneWork> GetNeighborLane(Vector3 v)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var c = cell2Groups[cellNo];
                    return c.lanes.Where(l => l != laneWork).ToList();
                }

                // index番目の頂点と隣接しているレーン
                var vertex2Neighbors = laneWork.LineString.Vertices.Select(GetNeighborLane).ToList();

                // 1頂点でしかつながっていないレーンは隣接していないので削除
                // 交差点で隣り合う道路道路は１頂点でつながっているが実際は中央の交差点
                // 以下のようにAとBは共通する1頂点があるが実際はそれぞれEとしかつながっていないので削除する
                //        A
                //       | |
                //       | |
                //    ---o o ---
                //   B    E   C
                //    ---o o ---
                //       | |
                //       | |
                //        D
                laneWork.ConnectedLanes
                    .RemoveWhere(c => vertex2Neighbors.Count(n => n.Any(l => l == c)) <= 1);

                foreach (var n in vertex2Neighbors)
                {
                    n.RemoveAll(a => laneWork.ConnectedLanes.Contains(a) == false);
                }
                // #TODO : 同じレーンが偶数回出てくる前提
                //       : 

                // 隣接レーンが一つもない場合は孤立
                if (vertex2Neighbors.Any(n => n.Any()) == false)
                {
                    ret.Links.Add(RoadNetworkLink.CreateIsolatedLink(laneWork.LineString));
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
                // List<int> wayVertexIndices = new List<int> { startIndex };
                var splitWays = SplitWay();

                // 境界線
                var borderWorks = new List<WayWork>();
                // 車線端線
                var wayWorks = new List<WayWork>();
                foreach (var wayVertexIndices in splitWays)
                {
                    var lineString = RoadNetworkLineString.Create(wayVertexIndices.Select(v => laneWork.LineString.Vertices[v]));
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
                    var link = new RoadNetworkLink();
                    if (l.IsBothConnectedLane && splitCenterLine)
                    {
                        link.MainLanes.AddRange(l.SplitLane(2));
                    }
                    else
                    {
                        link.MainLanes.Add(l);
                    }
                    ret.Links.Add(link);
                }
                // 行き止まり
                else if (laneWork.ConnectedLanes.Count == 1)
                {
                    var startBorderWay = leftWay?.FromBorder?.Way;
                    var endBorderWay = leftWay?.ToBorder?.Way;
                    var l = new RoadNetworkLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                    ret.Links.Add(RoadNetworkLink.CreateOneLaneLink(l));
                }
                // 交差点
                else if (laneWork.ConnectedLanes.Count >= 3)
                {
                    var node = new RoadNetworkNode();
                    ret.Nodes.Add(node);
                }
            }

            return ret;
        }

        ///// <summary>
        ///// 中央線の構築
        ///// </summary>
        //private List<Vector3> ComputeCenterLine(List<Vector3> vertices, List<RoadNetworkBorder> borders, List<RoadNetworkWay> ways, out bool isPartial)
        //{
        //    isPartial = false;
        //    var ret = new List<Vector3>();
        //    var visitedBorder = new HashSet<RoadNetworkBorder>();
        //    foreach (var border in borders)
        //    {
        //        if (visitedBorder.Contains(border))
        //            continue;
        //        visitedBorder.Add(border);
        //        // Borderの中心を開始点とする
        //        if (border.TryGetCenterVertex(out Vector3 startPoint) == false)
        //            continue;
        //        ret.Add(startPoint);

        //        // このBorderから出ていくWayすべてを使って中心線を書く
        //        var border1 = border;
        //        var targetWays = ways.Where(w => w.prevLane == border1.NeighborLane).ToList();

        //        // #TODO : 一旦prev/nextが一致しているものだけが対象. (枝分かれしているような交差点のレーンは対象外)
        //        if (targetWays.Count != 2 || targetWays[0].nextLane != targetWays[1].nextLane)
        //            continue;

        //        // Borderに対してWayが2つないと中心線が取れないので無視する
        //        // #TODO : 3本以上ある場合が想像できないが要対応
        //        if (targetWays.Count != 2)
        //            continue;

        //        var candidates = new List<Vector3>();
        //        foreach (var way in targetWays)
        //        {
        //            for (var i = 1; i < way.vertices.Count - 1; ++i)
        //            {
        //                var v = way.vertices[i];
        //                var n = -way.GetVertexNormal(i).normalized;
        //                var ray = new Ray(v + n * 0.01f, n);
        //                if (GeoGraph2d.PolygonHalfLineIntersectionXZ(vertices, ray, out var inter, out var t))
        //                    candidates.Add(Vector3.Lerp(v, inter, 0.5f));
        //            }
        //        }

        //        while (candidates.Count > 0)
        //        {
        //            var before = ret.Last();
        //            var found = candidates
        //                // LaneがものすごいUターンしていたりする時の対応
        //                // beforeから直接いけないものは無視
        //                .Where(c => targetWays.All(w => w.SegmentIntersectionXz(before, c, out var _) == false))
        //                .TryFindMin(x => (x - before).sqrMagnitude, out var nearPoint);
        //            if (found == false)
        //            {
        //                //Assert.IsTrue(found, "center point not found");
        //                DebugUtil.DrawArrow(before, before + Vector3.up * 2, arrowSize: 1f, duration: 30f, bodyColor: Color.blue);

        //                foreach (var c in candidates)
        //                    DebugUtil.DrawArrow(c, c + Vector3.up * 100, arrowSize: 1f, duration: 30f, bodyColor: Color.red);
        //                isPartial = true;
        //                break;
        //            }

        //            ret.Add(nearPoint);
        //            candidates.RemoveAll(x => (x - nearPoint).sqrMagnitude <= float.Epsilon);
        //        }

        //        // 構築するwayがすべて目的地が同じ場合、中心線に目的地のBorderを追加する
        //        if (targetWays.All(t => t.nextLane == targetWays[0].nextLane))
        //        {
        //            var lastBorder = borders.FirstOrDefault(e => e.NeighborLane == targetWays[0].nextLane);
        //            if (lastBorder != null)
        //            {
        //                if (lastBorder.TryGetCenterVertex(out var end))
        //                    ret.Add(end);
        //            }
        //        }
        //    }
        //    // 自己交差があれば削除する
        //    GeoGraph2d.RemoveSelfCrossing(ret, t => t.Xz(), (p1, p2, p3, p4, inter, f1, f2) => Vector3.Lerp(p1, p2, f1));
        //    return ret;
        //}
    }
}