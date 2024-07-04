using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using CityObjectList = PLATEAU.CityInfo.CityObjectList;

namespace PLATEAU.RoadNetwork.Factory
{
    [Serializable]
    public class RoadNetworkFactory
    {
        // --------------------
        // start:フィールド
        // --------------------

        // 同一頂点扱いにするセルサイズ
        [SerializeField] public float cellSize = 0.01f;

        // 道路サイズ
        [SerializeField] public float roadSize = 3f;

        // 行き止まり検出判定時に同一直線と判断する角度の総和
        [SerializeField] public float terminateAllowEdgeAngle = 20f;
        // 行き止まり検出判定時に開始線分と同一と判定する角度の許容量
        [SerializeField] public float terminateSkipEdgeAngle = 20f;
        // Lod1の道の歩道サイズ
        [SerializeField] public float lod1SideWalkSize = 3f;

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


        /// <summary>
        /// 頂点 -> ポイント変換を行う
        /// </summary>
        private class Vertex2PointTable
        {
            public float CellSize { get; }

            private Dictionary<Vector3, RnPoint> Table { get; } = new Dictionary<Vector3, RnPoint>();

            private Dictionary<Vector3Int, RnPoint> CellTable { get; } = new Dictionary<Vector3Int, RnPoint>();


            public Vertex2PointTable(float cellSize, IEnumerable<Vector3> vertices)
            {
                // 頂点の一致判定のためにセル単位に切り捨て
                CellSize = cellSize;
                foreach (var v in vertices)
                {
                    Table[v] = Create(v);
                }
            }

            public Vector3Int GetCell(Vector3 v)
            {
                return v.RevScaled(Vector3.one * CellSize).ToVector3Int();
            }

            public RnPoint Create(Vector3 v)
            {
                var cellNo = GetCell(v);
                return CellTable.GetValueOrCreate(cellNo, c => new RnPoint(v));
            }

            public void ChangeVertex(RnPoint v, Vector3 newVertex)
            {
                var cellNo = GetCell(v.Vertex);
                CellTable.Remove(cellNo);
                v.Vertex = newVertex;
                CellTable[cellNo] = v;
            }

            public RnPoint this[Vector3 v]
            {
                get
                {
                    return Table[v];
                }
            }
        }

        private class LineStringTable
        {
            private List<RnLineString> LineStrings { get; } = new List<RnLineString>();

            public RnLineString Create(IEnumerable<RnPoint> v, out bool isReverse)
            {
                isReverse = false;
                var vertices = v.ToList();
#if true
                foreach (var l in LineStrings)
                {
                    if (l.Count != vertices.Count())
                        continue;
                    if (Enumerable.Range(0, l.Count).All(i => l[i] == vertices[i]))
                        return l;
                    // 逆順もチェックする
                    if (Enumerable.Range(0, l.Count).All(i => l[i] == vertices[vertices.Count() - i - 1]))
                    {
                        isReverse = true;
                        return l;
                    }
                }
#endif
                var ret = RnLineString.Create(vertices);
                LineStrings.Add(ret);
                return ret;
            }
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
            public PLATEAUCityObjectGroup TargetTran { get; }

            public int LodLevel { get; }

            // 時計回りに格納されている
            public RnWay Way { get; set; } = new RnWay();

            // 接続しているすべてのTranWork
            public IEnumerable<TranWork> ConnectedTranWorks => Vertex2Connected.SelectMany(v => v.Value).Distinct();

            // 頂点 -> 接続TranWork情報
            public Dictionary<RnPoint, List<TranWork>> Vertex2Connected { get; } =
                new Dictionary<RnPoint, List<TranWork>>();

            // 対応するLink
            private RnLink Link { get; set; }

            // 対応するNode
            private RnNode Node { get; set; }

            // 境界線
            public IEnumerable<WayWork> Borders => Ways.Where(w => w.IsBorder);

            // レーン
            public IEnumerable<WayWork> Lanes => Ways.Where(w => w.IsBorder == false);

            // 構成するWay. Wayは端点以外では他のTranと接続していないことが保証されている
            public List<WayWork> Ways { get; } = new List<WayWork>();

            public RoadType RoadType
            {
                get
                {
                    // 1つ以下の場合は孤立状態
                    if (Ways.Count <= 1)
                    {
                        return RoadType.Isolated;
                    }

                    var count = ConnectedTranWorks.Count();
                    switch (count)
                    {
                        case 0: return RoadType.Isolated;
                        case 1: return RoadType.Terminate;
                        case 2: return RoadType.Road;
                        default: return RoadType.Intersection;
                    }
                }
            }

            public TranWork(PLATEAUCityObjectGroup target, int lodLevel)
            {
                TargetTran = target;
                LodLevel = lodLevel;
            }

            public void Bind(RnLink link)
            {
                Link = link;
            }

            public void Bind(RnNode node)
            {
                Node = node;
            }

            public void Split2Way(LineStringTable lineStringTable)
            {
                var points = new RoadNetworkWayPoints(Way);
                var startIndex = Enumerable.Range(0, points.Count)
                    .Where(i => Vertex2Connected[points[i]].Any())
                    .DefaultIfEmpty(-1)
                    .First();
                // 分割できない場合は無視
                if (startIndex < 0)
                {
                    if (points.Any())
                    {
                        var wayWork = new WayWork(Way, this);
                        Ways.Add(wayWork);
                    }
                    return;
                }

                var wayVertexIndices = new List<RnPoint> { points[startIndex] };
                foreach (var tmp in Enumerable.Range(1, points.Count))
                {
                    var i = (tmp + startIndex) % points.Count;
                    var p = points[i];
                    wayVertexIndices.Add(p);
                    if (wayVertexIndices.Count <= 1)
                        continue;
                    // 
                    var neighbors = Vertex2Connected[points[i]];
                    // 隣接している点があったら切り替え
                    if (neighbors.Any() == false)
                        continue;

                    var lineString = lineStringTable.Create(wayVertexIndices, out bool isReverse);
                    var way = new RnWay(lineString, isReverse);
                    var wayWork = new WayWork(way, this);
                    Ways.Add(wayWork);
                    wayVertexIndices = new List<RnPoint> { p };
                }

                // 全く同じTranWorkへの境界が二つ以上あるとき、それらはマージする

                {
                    var visitedWays = new HashSet<WayWork>();
                    // 境界線の接続レーンとつながっているwayの方向をそろえる
                    foreach (var l in Borders.SelectMany(b => b.BothConnectedTrans).Distinct())
                    {
                        // lとつながっているレーンを見に行く
                        foreach (var w in Lanes.Where(w => w.AnyConnectedTrans.Contains(l)))
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
            }

            public void BuildConnection(Vertex2PointTable vertexTable, float lod1RoadSize, HashSet<RnPoint> visited, out List<RnLineString> sideWalkLineStrings)
            {
                sideWalkLineStrings = new List<RnLineString>();
                if (Link != null)
                {
                    var nextTrans = Lanes.Select(w => w.NextBorder).Where(b => b != null).SelectMany(w => w.BothConnectedTrans).Distinct().ToList();
                    var prevTrans = Lanes.Select(w => w.PrevBorder).Where(b => b != null).SelectMany(w => w.BothConnectedTrans).Distinct().ToList();
                    Link.Next = nextTrans.FirstOrDefault(t => t.Node != null)?.Node;
                    Link.Prev = prevTrans.FirstOrDefault(t => t.Node != null)?.Node;
                    if (LodLevel == 1)
                    {
                        var leftLane = Link.MainLanes.FirstOrDefault();
                        var rightLane = Link.MainLanes.LastOrDefault();

                        RnLineString MoveWay(RnWay way)
                        {
                            if (lod1RoadSize <= 0f)
                                return null;

                            var origVertices = way.Vertices.ToList();

                            var normals = Enumerable.Range(0, way.Count)
                                .Select(i => way.GetVertexNormal(i).normalized)
                                .ToList();

                            // 元の線分 & 新しい線分の始点と終点を加えた線分が歩道分
                            var points = origVertices.Select(v => new RnPoint(v)).ToList();


                            // 始点/終点は除いて法線と逆方向に動かす
                            var isAddFirst = visited.Contains(way.GetPoint(0));
                            var isAddLast = visited.Contains(way.GetPoint(-1));
                            for (var i = 0; i < way.Count; ++i)
                            {
                                var n = normals[i];
                                var p = way.GetPoint(i);
                                if (visited.Contains(p) == false)
                                {
                                    p.Vertex -= n * lod1RoadSize;
                                }
                                else
                                {
                                    var copy = p.Vertex + n * lod1RoadSize;
                                    var newP = new RnPoint(copy);
                                    if (i == 0)
                                        points.Insert(1, newP);
                                    else if (i == way.Count - 1)
                                        points.Insert(points.Count - 1, newP);
                                }
                                visited.Add(p);
                            }

                            if (isAddFirst == false)
                            {
                                points.Insert(0, way.GetPoint(0));
                            }
                            else
                            {
                                //var copy = origVertices[0] + normals[0] * lod1RoadSize;
                                //var newP = vertexTable.Create(copy);
                                //points.Insert(1, newP);
                            }

                            if (isAddLast == false)
                            {
                                points.Add(way.GetPoint(-1));
                            }
                            else
                            {
                                //var copy = origVertices[^1] + normals[^1] * lod1RoadSize;
                                //var newP = vertexTable.Create(copy);
                                //points.Insert(points.Count - 2, newP);

                            }
                            return RnLineString.Create(points);
                        }
                        if (leftLane?.LeftWay != null)
                        {
                            var way = leftLane?.LeftWay;
                            var ls = MoveWay(way);
                            if (ls != null)
                                sideWalkLineStrings.Add(ls);
                        }
                        if (rightLane?.RightWay != null)
                        {
                            var way = rightLane?.RightWay;
                            var ls = MoveWay(way);
                            if (ls != null)
                                sideWalkLineStrings.Add(ls);
                        }
                    }
                }
                else if (Node != null)
                {
                    foreach (var b in Borders)
                    {
                        foreach (var l in b.BothConnectedTrans)
                        {
                            if (l.Link == null)
                                continue;
                            Node.Neighbors.Add(new RnNeighbor
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
            public TranWork Parent { get; }

            public RnWay Way { get; private set; }

            /// <summary>
            /// 開始地点と隣接しているレーン
            /// </summary>
            public IReadOnlyList<TranWork> PrevTranWorks => Parent.Vertex2Connected[Way.GetPoint(0)];

            /// <summary>
            /// 終了地点と隣接しているレーン
            /// </summary>
            public List<TranWork> NextTranWorks => Parent.Vertex2Connected[Way.GetPoint(-1)];

            /// <summary>
            /// 通常の道用. Wayの開始地点と接続している境界線
            /// </summary>
            public WayWork PrevBorder
            {
                get
                {
                    foreach (var b in Parent.Borders)
                    {
                        foreach (var l in b.BothConnectedTrans)
                        {
                            if (PrevTranWorks.Contains(l))
                                return b;
                        }
                    }

                    return null;
                }
            }

            /// <summary>
            /// 通常の道用. Wayの終了地点と接続している境界線
            /// </summary>
            public WayWork NextBorder
            {
                get
                {
                    foreach (var b in Parent.Borders)
                    {
                        foreach (var l in b.BothConnectedTrans)
                        {
                            if (NextTranWorks.Contains(l))
                                return b;
                        }
                    }

                    return null;
                }
            }

            /// <summary>
            /// From/Toどっちにも含まれているレーン(接しているレーン)
            /// </summary>
            public IEnumerable<TranWork> BothConnectedTrans
            {
                get
                {
                    foreach (var l in PrevTranWorks)
                    {
                        if (NextTranWorks.Contains(l))
                            yield return l;
                    }
                }
            }

            /// <summary>
            /// From/Toどっちかに含まれているレーン
            /// </summary>
            public IEnumerable<TranWork> AnyConnectedTrans => PrevTranWorks.Concat(NextTranWorks).Distinct();


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
                        if (Way.Count <= 1)
                            return cachedIsBorder.Value;

                        var isTerminated = Parent.RoadType == RoadType.Terminate;
                        foreach (var neighbor in BothConnectedTrans)
                        {
                            // 共通の隣接レーンがあると境界線
                            // ただし、行き止まり(1つのレーンとしか隣接していない)場合
                            // 全WayがBorder扱いになるので別途チェックが必要

                            // 共通のLineStringがある場合は境界線
                            if (neighbor.Ways.Any(w => w.Way.LineString == Way.LineString))
                            {
                                cachedIsBorder = true;
                                break;
                            }

                            var p0 = Way[0];
                            var p1 = Way[1];
                            var cp = (p0 + p1) * 0.5f;
                            var n = Way.GetEdgeNormal(0);
                            // 微小にずらして確認する
                            var p = cp + n * 0.01f;
                            cachedIsBorder = GeoGraph2D.Contains(neighbor.Way.Select(x => x.Xz()), p.Xz());
                            if (cachedIsBorder.Value)
                                break;
                        }
                    }

                    return cachedIsBorder.Value;
                }
            }

            public WayWork(RnWay way, TranWork parent)
            {
                Way = way;
                Parent = parent;
            }

            /// <summary>
            /// 道を逆転させる
            /// </summary>
            public void Reverse()
            {
                Way = Way.ReversedWay();
            }
        }

        public Task<RnModel> CreateNetworkAsync(IList<RoadNetworkTranMesh> targets)
        {
            // レーンの初期化
            try
            {
                var ret = new RnModel();
                var vertex2Points = new Vertex2PointTable(cellSize, targets.SelectMany(v => v.Vertices));
                var lineStringTable = new LineStringTable();
                var tranWorks = CreateTranWorks(targets, vertex2Points, lineStringTable, out var cell2Groups);

                foreach (var tranWork in tranWorks)
                    Build(lineStringTable, tranWork, ret);

                var visited = new HashSet<RnPoint>();
                foreach (var tranWork in tranWorks)
                {
                    tranWork.BuildConnection(vertex2Points, lod1SideWalkSize, visited, out var ls);
                    foreach (var l in ls)
                        ret.AddWalkRoad(l);
                }

                //ret.DebugIdentify();
                return Task.FromResult(ret);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private void Build(LineStringTable lineStringTable, TranWork tranWork, RnModel ret)
        {
            var roadType = tranWork.RoadType;
            // 隣接レーンが一つもない場合は孤立
            if (roadType == RoadType.Isolated)
            {
                ret.AddLink(RnLink.CreateIsolatedLink(tranWork.TargetTran, tranWork.Way));
                return;
            }

            // 行き止まり
            if (roadType == RoadType.Terminate)
            {
                // もともと頂点が時計回りになっているのでIsReversed == trueの者が右になる
                var wayWork = tranWork.Lanes.FirstOrDefault();
                var way = wayWork?.Way;
                // どっちかはnot nullなはず
                if (way != null)
                {
                    var vertices = way.Vertices.Select(x => x.Xz()).ToList();
                    var edgeIndices = GeoGraph2D.FindMidEdge(vertices, terminateAllowEdgeAngle);

                    RnWay AsWay(IEnumerable<int> ind, bool isReverse, bool isRightSide)
                    {
                        var ls = lineStringTable.Create(ind.Select(x => way.GetPoint(x)), out bool isRev);
                        return new RnWay(ls, isRev ? !isReverse : isReverse, isRightSide);
                    }

                    var rWay = AsWay(Enumerable.Range(0, edgeIndices[0] + 1), true, true);
                    var lWay = AsWay(Enumerable.Range(edgeIndices.Last(), way.Count - edgeIndices.Last()), false, false);
                    var startBorderWay = AsWay(edgeIndices, true, false);
                    var endBorderWay = tranWork.Borders.FirstOrDefault()?.Way;
                    var l = new RnLane(lWay, rWay, startBorderWay, endBorderWay);
                    var link = RnLink.CreateOneLaneLink(tranWork.TargetTran, l);
                    tranWork.Bind(link);
                    ret.AddLink(link);
                }
                else
                {
                    Debug.LogError("不正なレーン構成{Terminate)");
                }
            }
            // 交差点
            else if (roadType == RoadType.Intersection)
            {
                var node = new RnNode(tranWork.TargetTran);
                foreach (var l in tranWork.Lanes)
                {
                    var prevBorder = l.PrevBorder?.Way;
                    var nextBorder = l.NextBorder?.Way;
                    var lane = new RnLane(l.Way, null, prevBorder, nextBorder);
                    node.AddLane(lane);
                }
                tranWork.Bind(node);
                ret.AddNode(node);
            }
            // 通常の道
            else if (roadType == RoadType.Road)
            {
                foreach (var lw in tranWork.Lanes.GroupBy(a =>
                         {
                             // Prev/Nextの組み合わせが同じなら良いのでHashCodeでソートしたうえで返す
                             if ((a.PrevBorder?.GetHashCode() ?? 0) > (a.NextBorder?.GetHashCode() ?? 0))
                                 return new Tuple<WayWork, WayWork>(a.NextBorder, a.PrevBorder);
                             return new Tuple<WayWork, WayWork>(a.PrevBorder, a.NextBorder);
                         }))
                {
                    var laneWays = lw.ToList();
                    var (leftWay, rightWay) = (laneWays.ElementAtOrDefault(0), laneWays.ElementAtOrDefault(1));
                    if (leftWay?.Way?.IsReversed ?? false)
                        (rightWay, leftWay) = (leftWay, rightWay);

                    var link = new RnLink(tranWork.TargetTran);
                    if (leftWay != null && rightWay != null)
                    {


                        var startBorderWay = leftWay?.PrevBorder?.Way;
                        var endBorderWay = leftWay?.NextBorder?.Way;
                        var l = new RnLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                        var startBorderLength = GeoGraphEx.GetEdges(startBorderWay?.Vertices ?? new List<Vector3>(), false)
                            .Sum(e => (e.Item2 - e.Item1).magnitude);
                        var endBorderLength = GeoGraphEx.GetEdges(endBorderWay?.Vertices ?? new List<Vector3>(), false)
                            .Sum(e => (e.Item2 - e.Item1).magnitude);
                        var num = (int)(Mathf.Min(startBorderLength, endBorderLength) / roadSize);
                        if (l.HasBothBorder && num > 1)
                        {
                            var lanes = l.SplitLane(num);
                            foreach (var lane in lanes)
                                link.AddMainLane(lane);
                        }
                        else
                        {
                            link.AddMainLane(l);
                        }

                    }
                    else if (leftWay != null || rightWay != null)
                    {
                        var way = leftWay ?? rightWay;
                        var startBorderWay = way?.PrevBorder?.Way;
                        var endBorderWay = way?.NextBorder?.Way;
                        var lane = new RnLane(leftWay?.Way, rightWay?.Way, startBorderWay, endBorderWay);
                        link.AddMainLane(lane);
                        Debug.LogWarning("不正なレーン構成(片側Wayのみ存在)");
                    }
                    else
                    {
                        Debug.LogError("不正なーレーン構成(Wayの存在しないLane)");
                    }

                    if (link.MainLanes.Count > 0)
                    {
                        tranWork.Bind(link);
                        ret.AddLink(link);
                    }
                }


            }
        }

        private static List<TranWork> CreateTranWorks(IList<RoadNetworkTranMesh> targets, Vertex2PointTable vertex2Points, LineStringTable lineStringTable, out Dictionary<RnPoint, Cell> cell2Groups)
        {
            cell2Groups = new Dictionary<RnPoint, Cell>();
            // レーンの頂点情報を構築
            var tranWorks = new List<TranWork>();
            foreach (var item in targets)
            {
                var cityObject = item.CityObjectGroup;
                var linkOrNodeWork = new TranWork(cityObject, item.LodLevel);

                //var vertices = laneWork.LineString.Vertices;
                var vertices = item.Vertices.Select(v => vertex2Points[v]).ToList();
                // 時計回りになるように順番チェック
                if (GeoGraph2D.IsClockwise(vertices.Select(x => x.Vertex.Xz())) == false)
                    vertices.Reverse();

                foreach (var v in vertices)
                {
                    var cell = cell2Groups.GetValueOrCreate(v, v => new Cell { });
                    cell.Add(linkOrNodeWork);
                }
                var lineString = lineStringTable.Create(vertices, out bool isReverse);
                linkOrNodeWork.Way = new RnWay(lineString, isReverse);
                tranWorks.Add(linkOrNodeWork);
            }

            // laneのConnectedを構築
            // 特定頂点につながっているLinkNodeは接続情報を付与する
            foreach (var tranWork in tranWorks)
            {
                // 頂点に対してつながっているTranを追加
                foreach (var p in tranWork.Way.Points)
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

            foreach (var tran in tranWorks)
                tran.Split2Way(lineStringTable);
            return tranWorks;
        }
    }
}