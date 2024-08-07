using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork.Factory
{
    [Serializable]
    public partial class RoadNetworkFactory
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
        // Lod3の歩道を追加するかどうか
        [SerializeField] public bool addLod3SideWalk = true;
        // 高速道路を無視するかのフラグ
        [SerializeField] public bool ignoreHighway = false;
        // RGraph作るときのファクトリパラメータ
        [SerializeField] public RGraphFactory graphFactory;
        // 中間データ
        [SerializeField] public RsFactoryMidStageData midStageData;


        // --------------------
        // end:フィールド
        // --------------------


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

        private class Work
        {
            public Dictionary<PLATEAUCityObjectGroup, Tran> TranMap { get; } = new();

            public Dictionary<RVertex, RnPoint> PointMap { get; } = new();

            public Dictionary<List<RVertex>, RnLineString> LineMap { get; } = new();

            public float terminateAllowEdgeAngle = 20f;

            public RnPoint CreatePoint(RVertex v)
            {
                return PointMap.GetValueOrCreate(v, x => new RnPoint(x.Position));
            }

            private bool IsEqual(List<RVertex> a, List<RVertex> b, out bool isReverse)
            {
                isReverse = false;
                if (a.Count != b.Count)
                    return false;
                if (a[0] == b[0])
                {
                    isReverse = false;
                    return a.SequenceEqual(b);
                }

                isReverse = true;
                return a.SequenceEqual(Enumerable.Range(0, a.Count).Select(i => b[b.Count - 1 - i]));
            }

            public RnWay CreateWay(List<RVertex> line)
            {
                foreach (var item in LineMap)
                {
                    if (IsEqual(item.Key, line, out var isReverse))
                    {
                        return new RnWay(item.Value, isReverse);
                    }
                }

                // 元のリストが変わるかもしれないのでコピーで持つ
                var key = line.ToList();
                LineMap[key] = RnLineString.Create(key.Select(CreatePoint), true);
                return new RnWay(LineMap[key], false);
            }



            public List<RnLineString> CreateSideWalk(float lod1SideWalkSize)
            {
                var visited = new Dictionary<RnPoint, (Vector3 pos, Vector3 normal)>();

                var ret = new List<RnLineString>();
                // LOD1の場合は周りにlod1RoadSize分歩道があると仮定して動かす
                void MoveWay(RnWay way)
                {
                    if (way == null)
                        return;
                    if (lod1SideWalkSize <= 0f)
                        return;

                    var normals = Enumerable.Range(0, way.Count)
                        .Select(i => way.GetVertexNormal(i).normalized)
                        .ToList();

                    // 始点/終点は除いて法線と逆方向に動かす
                    var points = new List<RnPoint> { };
                    foreach (var i in Enumerable.Range(0, way.Count))
                    {
                        var p = way.GetPoint(i);
                        var n = normals[i];
                        if (visited.ContainsKey(p) == false)
                        {
                            var last = p.Vertex;
                            p.Vertex -= n * lod1SideWalkSize;
                            visited[p] = new(last, n); //new VertexInfo { Normal = n, Position = last };
                            points.Add(new RnPoint(last));
                        }
                        else
                        {
                            var last = visited[p];
                            points.Add(new RnPoint(last.pos));

                        }
                    }

                    ret.Add(RnLineString.Create(points));
                }

                foreach (var tran in TranMap.Values)
                {
                    // 移動を適用するのはLODLevel1のみ
                    if (tran.LodLevel != 1)
                        continue;
                    if (tran.Node is RnRoad road)
                    {
                        var leftLane = road.MainLanes.FirstOrDefault();
                        var rightLane = road.MainLanes.LastOrDefault();
                        MoveWay(leftLane?.LeftWay);
                        MoveWay(rightLane?.RightWay);

                    }
                }

                return ret;
            }
        }

        private class Tran
        {
            // 親グラフ
            public RGraph Graph { get; }

            // 対象のCityObjectGroup
            public PLATEAUCityObjectGroup CityObjectGroup { get; }

            // 構成Face
            public HashSet<RFace> Faces { get; }

            public List<RVertex> Vertices { get; }

            public class Line
            {
                // 隣接Tran
                public PLATEAUCityObjectGroup Neighbor { get; set; }

                // 構成Edge
                public List<REdge> Edges { get; } = new List<REdge>();

                // 構成頂点
                public List<RVertex> Vertices { get; } = new List<RVertex>();

                public RnWay Way { get; set; }

                // 他との境界線かどうか
                public bool IsBorder => Neighbor != null;

                public Line Next { get; set; }

                public Line Prev { get; set; }

                public Tran GetNeighborTran(Work work)
                {
                    if (!Neighbor)
                        return null;
                    return work.TranMap.GetValueOrDefault(Neighbor);
                }

                public float GetMedianLength(PLATEAUCityObjectGroup cog)
                {
                    var ret = Vertices
                        .SkipWhile(v => v.GetRoadType(cog).IsMedian() == false)
                        .TakeWhile(v => v.GetRoadType(cog).IsMedian())
                        .Aggregate(new { last = (RVertex)null, len = 0f }, (a, e) =>
                        {
                            var len = a.len;
                            if (a.last != null)
                            {
                                len += (e.Position - a.last.Position).magnitude;
                            }

                            return new { last = e, len = len };
                        });
                    return ret.len;
                }
            }

            public List<Line> Lines { get; } = new List<Line>();

            // 隣接オブジェクトの数
            public int NeighborCount => Lines.Where(l => l.IsBorder).Select(l => l.Neighbor).Distinct().Count();

            // LodLevel
            public int LodLevel => Faces.Any() ? Faces.Max(f => f.LodLevel) : 0;

            public RoadType RoadType
            {
                get
                {
                    switch (NeighborCount)
                    {
                        case 0: return RoadType.Isolated;
                        case 1: return RoadType.Terminate;
                        case 2: return RoadType.Road;
                        // 3つ以上の隣接オブジェクトを持つかどうか
                        default: return RoadType.Intersection;
                    };
                }
            }

            public bool IsValid { get; private set; } = true;

            public Work Work { get; }

            // RnIntersection or RnRoad
            public RnRoadBase Node { get; private set; }

            // 中央分離帯の幅を求める
            public float GetMedianWidth()
            {
                var lines = Lines.Where(l => l.IsBorder && l.GetMedianLength(CityObjectGroup) > 0).ToList();
                if (lines.Any() == false)
                    return 0f;

                return lines.Min(v => v.GetMedianLength(CityObjectGroup));
            }

            public Tran(Work work, RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask targetRoadTypeMask)
            {
                Work = work;
                Graph = graph;
                CityObjectGroup = cityObjectGroup;
                Vertices = Graph.ComputeOutlineVerticesByCityObjectGroup(cityObjectGroup, targetRoadTypeMask, out var faces);
                // 時計回りになるように順番チェック
                if (GeoGraph2D.IsClockwise(Vertices.Select(x => x.Position.Xz())) == false)
                    Vertices.Reverse();
                Faces = new HashSet<RFace>(faces);

                // 線分構築
                IsValid = BuildLines();
            }

            public void Build()
            {
                Node = CreateRoad();
            }

            private RnRoadBase CreateRoad()
            {
                // 孤立
                if (RoadType == RoadType.Isolated)
                {
                    var way = Work.CreateWay(Vertices);
                    var road = RnRoad.CreateIsolatedRoad(CityObjectGroup, way);
                    return road;
                }

                RnRoadBase GetNode(Line l)
                {
                    if (l == null)
                        return null;
                    if (!l.Neighbor)
                        return null;
                    return Work.TranMap.GetValueOrDefault(l.Neighbor)?.Node;
                }
                // 行き止まり
                if (RoadType == RoadType.Terminate)
                {
                    var border = Lines.FirstOrDefault(l => l.IsBorder);
                    var line = Lines.FirstOrDefault(l => l.IsBorder == false);
                    if (line == null)
                    {
                        Debug.LogError($"不正なレーン構成[Terminate] : {CityObjectGroup.name}");
                        return null;
                    }

                    var vertices = line.Vertices.Select(v => v.Position.Xz()).ToList();
                    var edgeIndices = GeoGraph2D.FindMidEdge(vertices, Work.terminateAllowEdgeAngle);

                    RnWay AsWay(IEnumerable<int> ind, bool isReverse, bool isRightSide)
                    {
                        var ls = Work.CreateWay(ind.Select(x => line.Vertices[x]).ToList());
                        return new RnWay(ls.LineString, ls.IsReversed ? !isReverse : isReverse, isRightSide);
                    }

                    var rWay = AsWay(Enumerable.Range(0, edgeIndices[0] + 1), true, true);
                    var lWay = AsWay(Enumerable.Range(edgeIndices.Last(), line.Vertices.Count - edgeIndices.Last()), false, false);
                    var prevBorderWay = AsWay(edgeIndices, true, false);
                    var nextBorderWay = Work.CreateWay(border.Vertices);
                    var lane = new RnLane(lWay, rWay, prevBorderWay, nextBorderWay);
                    var road = RnRoad.CreateOneLaneRoad(CityObjectGroup, lane);
                    road.SetPrevNext(null, border.GetNeighborTran(Work)?.Node);
                    return road;
                }

                // 通常の道
                if (RoadType == RoadType.Road)
                {
                    var lanes = Lines.Where(l => l.IsBorder == false).ToList();
                    var road = new RnRoad(CityObjectGroup);

                    // 同じPrev/Nextの組み合わせでグループ化(順序逆は問わない)
                    var pairs = lanes.GroupBy(l => RnEx.CombSet(l.Prev, l.Next))
                        .Select(g =>
                        {
                            var laneWays = g.ToList();
                            var (leftWay, rightWay) = (laneWays.ElementAtOrDefault(0), laneWays.ElementAtOrDefault(1));
                            if (leftWay?.Way?.IsReversed ?? false)
                                (leftWay, rightWay) = (rightWay, leftWay);
                            return new { leftWay, rightWay };
                        }).ToList();
                    var pair = pairs.FirstOrDefault(p => p.leftWay != null && p.rightWay != null);
                    if (pair == null)
                        pair = pairs.FirstOrDefault();
                    if (pair != null)
                    {
                        var (leftWay, rightWay) = (pair.leftWay, pair.rightWay);

                        if (leftWay == null && rightWay == null)
                        {
                            Debug.LogError($"不正なーレーン構成(Wayの存在しないLane) {CityObjectGroup.name}");
                            return road;
                        }

                        // ログだけ残しておく
                        if (leftWay == null || rightWay == null)
                            Debug.LogWarning($"不正なレーン構成(片側Wayのみ存在) {CityObjectGroup.name}");

                        var way = leftWay ?? rightWay;
                        var prevBorderWay = way?.Prev?.Way;
                        var nextBorderWay = way?.Next?.Way;

                        // もともと時計回りなのでleftWayとrightWayが逆になっている
                        // 方向そろえるためにrightWayを反転させている
                        // #TODO : ちゃんと始点/終点がPrev/Nextになっているか確認すべき
                        var lane = new RnLane(leftWay?.Way, rightWay?.Way?.ReversedWay(), prevBorderWay, nextBorderWay);
                        road.AddMainLane(lane);
                        return road;
                    }
                    Assert.IsTrue(false, $"不正なレーン構成 {CityObjectGroup.name}");
                }

                // 交差点
                if (RoadType == RoadType.Intersection)
                {
                    var intersection = new RnIntersection(CityObjectGroup);
                    // Track情報
                    foreach (var l in Lines.Where(l => l.IsBorder == false))
                    {
                        var prevBorder = l.Prev?.Way;
                        var nextBorder = l.Next?.Way;
                        var lane = new RnLane(l.Way, null, prevBorder, nextBorder);
                        intersection.AddLane(lane);
                    }

                    return intersection;
                }
                Debug.LogError($"不正なレーン構成 : {CityObjectGroup.name}");
                return null;
            }

            /// <summary>
            /// 接続情報作成
            /// </summary>
            public void BuildConnection()
            {
                if (Node is RnRoad road)
                {
                    var lane = road.MainLanes.FirstOrDefault();
                    if (lane == null)
                        return;
                    var prev = Lines.Where(l => l.IsBorder && l.Way != null)
                        .FirstOrDefault(l => lane.PrevBorder?.IsSameLine(l.Way) ?? false);
                    var next = Lines.Where(l => l.IsBorder && l.Way != null)
                        .FirstOrDefault(l => lane.NextBorder?.IsSameLine(l.Way) ?? false);

                    var prevRoad = prev?.GetNeighborTran(Work)?.Node;
                    var nextRoad = next?.GetNeighborTran(Work)?.Node;
                    road.SetPrevNext(prevRoad, nextRoad);
                }
                else if (Node is RnIntersection intersection)
                {
                    // 境界線情報
                    foreach (var b in Lines.Where(l => l.IsBorder))
                    {
                        intersection.AddNeighbor(Work.TranMap[b.Neighbor].Node, b.Way);
                    }
                }
            }

            private bool BuildLines()
            {
                var edges = new List<REdge>(Vertices.Count);
                var neighbors = new List<PLATEAUCityObjectGroup>(Vertices.Count);
                var success = true;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    var v0 = Vertices[i];
                    var v1 = Vertices[(i + 1) % Vertices.Count];

                    var e = v0.Edges.FirstOrDefault(e => e.IsSameVertex(v0, v1));

                    if (e == null)
                    {
                        Debug.LogError($"ループしていないメッシュ. {CityObjectGroup.name}");
                        success = false;
                        continue;
                    }

                    edges.Add(e);
                    var neighbor = e.Faces.FirstOrDefault(f => f.CityObjectGroup != CityObjectGroup);
                    neighbors.Add(neighbor?.CityObjectGroup);
                }

                var startIndex = 0;
                for (var i = 1; i < edges.Count; i++)
                {
                    var e = edges[i];
                    if (neighbors[i] != neighbors[0])
                    {
                        startIndex = i;
                        break;
                    }
                }

                Line line = null;
                foreach (var i in Enumerable.Range(0, edges.Count).Select(i => (i + startIndex) % edges.Count))
                {
                    var v = Vertices[i];
                    var e = edges[i];
                    var n = neighbors[i];
                    // 切り替わり発生したら新しいLineを作成
                    if (line == null || line.Neighbor != n)
                    {
                        if (line != null)
                            line.Vertices.Add(v);
                        var next = new Line { Neighbor = n, Prev = line };
                        if (line != null)
                            line.Next = next;
                        line = next;
                        Lines.Add(line);
                    }

                    line.Edges.Add(e);
                    line.Vertices.Add(v);
                }

                if (line != null && Lines.Count > 1)
                {
                    line.Vertices.Add(Vertices[startIndex]);
                    line.Next = Lines[0];
                    Lines[0].Prev = line;
                }

                // Wayを先に作っておく
                foreach (var l in Lines)
                    l.Way = Work.CreateWay(l.Vertices);
                return success;
            }
        }

        public Task<RnModel> CreateRnModelAsync(RGraph graph)
        {
            try
            {
                // 同じCityObjectGroupでFaceをまとめる
                var faceGroups = graph
                    .Faces
                    .GroupBy(f => f.CityObjectGroup)
                    .Where(f =>
                    {
                        // 道路を全く含まない時は無視
                        if (f.Any(f => f.RoadTypes.IsRoad()) == false)
                            return false;
                        // 高速道路を無視しない場合はOK
                        if (ignoreHighway == false)
                            return true;
                        // 高速道路を含むFaceがあったらダメ
                        return f.Any(f => f.RoadTypes.IsHighWay()) == false;
                    })
                    .ToList();

                RRoadTypeMask mask = RRoadTypeMask.Road | RRoadTypeMask.Median;
                if (ignoreHighway == false)
                    mask |= RRoadTypeMask.HighWay;

                var ret = new RnModel();
                var work = new Work { terminateAllowEdgeAngle = terminateAllowEdgeAngle };

                // 最初にTranを作成
                foreach (var cog in faceGroups.Select(c => c.Key))
                    work.TranMap[cog] = new Tran(work, graph, cog, mask);

                // 作成したTranを元にRoadを作成
                foreach (var tran in work.TranMap.Values)
                {
                    tran.Build();
                    if (tran.Node != null)
                        ret.AddRoadBase(tran.Node);
                }

                foreach (var tran in work.TranMap.Values)
                {
                    tran.BuildConnection();
                }

                // 歩道を作成する
                var sideWalks = work.CreateSideWalk(lod1SideWalkSize);
                foreach (var sideWalk in sideWalks)
                    ret.AddSideWalk(sideWalk);
                if (addLod3SideWalk)
                {
                    foreach (var c in faceGroups)
                    {
                        foreach (var sideWalkFace in c.Where(f => f.RoadTypes.IsSideWalk()))
                        {
                            var vertices = sideWalkFace.ComputeOutlineVertices(RRoadTypeMask.SideWalk);
                            var way = work.CreateWay(vertices);
                            ret.AddSideWalk(way.LineString);
                        }
                    }
                }

                {
                    var visited = new HashSet<RnRoad>();
                    foreach (var n in work.TranMap.Values)
                    {
                        if (n.Node is RnRoad road)
                        {
                            if (visited.Contains(road))
                                continue;
                            var width = n.GetMedianWidth();
                            if (width <= 0f)
                                continue;
                            var linkGroup = road.CreateRoadGroupOrDefault();
                            if (linkGroup == null)
                                continue;
                            linkGroup.SetLaneCount(1, 1);
                            linkGroup.SetMedianWidth(width, LaneWayMoveOption.MoveBothWay);
                            foreach (var r in linkGroup.Roads)
                                visited.Add(r);
                        }
                    }
                }

                return Task.FromResult(ret);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public async Task<RnModel> CreateRnModelAsync(List<PLATEAUCityObjectGroup> cityObjectGroups)
        {
            await midStageData.ConvertCityObjectAsync(cityObjectGroups);
            var graph = midStageData.CreateGraph(graphFactory);
            var model = await CreateRnModelAsync(graph);
            if (midStageData.saveTmpData == false)
                midStageData.rGraph.rGraph = null;
            return model;
        }

        public void DebugDraw()
        {
            midStageData?.DebugDraw();
        }
    }
}