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

        // 中央分離帯をチェックする
        [SerializeField] public bool checkMedian = true;
        [SerializeField] public bool zeroWidthMedian = false;
        // 高速道路を無視するかのフラグ
        [SerializeField] public bool ignoreHighway = false;
        // RGraph作るときのファクトリパラメータ
        [SerializeField] public RGraphFactory graphFactory;
        // 中間データ
        [SerializeField] public RsFactoryMidStageData midStageData;


        // --------------------
        // end:フィールド
        // --------------------


        public enum RoadType
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

        public class Work
        {
            public Dictionary<RFaceGroup, Tran> TranMap { get; } = new();

            public Dictionary<RVertex, RnPoint> PointMap { get; } = new();

            public Dictionary<List<RVertex>, RnLineString> LineMap { get; } = new();

            // key   : RnLineString.PointsのDebugIdのxor(高速化用)
            // value : RnLineString
            public Dictionary<ulong, List<RnLineString>> RnPointList2LineStringMap { get; } = new();

            public float terminateAllowEdgeAngle = 20f;

            public Tran FindTranOrDefault(RFace face)
            {
                return TranMap.FirstOrDefault(x => x.Key.Faces.Contains(face)).Value;
            }

            public Tran FindTranOrDefault(RFaceGroup faceGroup)
            {
                return TranMap.GetValueOrDefault(faceGroup);
            }

            public RnPoint CreatePoint(RVertex v)
            {
                return PointMap.GetValueOrCreate(v, x => new RnPoint(x.Position));
            }

            private static bool IsEqual(List<RVertex> a, List<RVertex> b, out bool isReverse)
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

            private static bool IsEqual(List<RnPoint> a, List<RnPoint> b, out bool isReverse)
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

            public RnWay CreateWay(List<RnPoint> points)
            {
                return CreateWay(points, out var _);
            }

            public RnWay CreateWay(List<RnPoint> points, out bool isCached)
            {
                isCached = false;
                if (points.Count <= 1)
                    return null;

                var key = points.Aggregate(0Lu, (a, p) => a ^ p.DebugMyId);
                var lines = RnPointList2LineStringMap.GetValueOrCreate(key);
                foreach (var line in lines)
                {
                    if (IsEqual(line.Points, points, out var isReverse))
                    {
                        isCached = true;
                        return new RnWay(line, false);
                    }
                }
                var newLine = RnLineString.Create(points, true);
                lines.Add(newLine);
                return new RnWay(newLine, false);
            }

            public RnWay CreateWay(List<RVertex> vertices)
            {
                return CreateWay(vertices, out var _);
            }

            public RnWay CreateWay(List<RVertex> vertices, out bool isCached)
            {
                isCached = false;
                // 頂点無い場合はnull
                if (vertices.Any() == false)
                    return null;
                foreach (var item in LineMap)
                {
                    if (IsEqual(item.Key, vertices, out var isReverse))
                    {
                        isCached = true;
                        return new RnWay(item.Value, isReverse);
                    }
                }

                // 元のリストが変わるかもしれないのでコピーで持つ
                var key = vertices.ToList();
                LineMap[key] = RnLineString.Create(key.Select(CreatePoint), true);
                return new RnWay(LineMap[key], false);
            }

            public List<RnSideWalk> CreateSideWalk(float lod1SideWalkSize)
            {
                var visited = new Dictionary<RnPoint, (RnPoint pos, Vector3 normal)>();

                var ret = new List<RnSideWalk>();
                // LOD1の場合は周りにlod1RoadSize分歩道があると仮定して動かす
                void MoveWay(RnWay way, RnRoad parent)
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
                            var before = new RnPoint(p.Vertex);
                            p.Vertex -= n * lod1SideWalkSize;
                            visited[p] = new(before, n); //new VertexInfo { Normal = n, Position = last };
                            points.Add(before);
                        }
                        else
                        {
                            var last = visited[p];
                            points.Add(last.pos);
                        }
                    }
                    var startWay = CreateWay(new List<RnPoint> { points[0], way.GetPoint(0) });
                    var endWay = CreateWay(new List<RnPoint> { points[^1], way.GetPoint(-1) });
                    var sideWalk = RnSideWalk.Create(parent, CreateWay(points), way, startWay, endWay);
                    ret.Add(sideWalk);
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
                        MoveWay(leftLane?.LeftWay, road);
                        MoveWay(rightLane?.RightWay, road);
                    }
                }

                return ret;
            }
        }

        public class Tran
        {
            // 親グラフ
            public RGraph Graph { get; }

            // 構成Face
            public HashSet<RFace> Faces => FaceGroup.Faces;

            public RFaceGroup FaceGroup { get; }

            public List<RVertex> Vertices { get; }

            public class Line
            {
                public Tran Neighbor { get; set; }

                // 構成Edge
                public List<REdge> Edges { get; } = new List<REdge>();

                // 構成頂点
                public List<RVertex> Vertices { get; } = new List<RVertex>();

                public RnWay Way { get; set; }

                // 他との境界線かどうか
                public bool IsBorder => Neighbor != null;

                public Line Next { get; set; }

                public Line Prev { get; set; }
            }

            public List<Line> Lines { get; } = new List<Line>();

            // 隣接オブジェクトの数
            public int NeighborCount => Lines.Where(l => l.IsBorder).Count();

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


            private float GetMedianLength(Line line)
            {
                var ret = line.Vertices
                    .SkipWhile(v => v.GetRoadType(f => Faces.Contains(f)).IsMedian() == false)
                    .TakeWhile(v => v.GetRoadType(f => Faces.Contains(f)).IsMedian())
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
            // 中央分離帯の幅を求める
            public float GetMedianWidth()
            {
                var lines = Lines.Where(l => l.IsBorder && GetMedianLength(l) > 0).ToList();
                if (lines.Any() == false)
                    return 0f;

                return lines.Min(GetMedianLength);
            }

            public Tran(Work work, RGraph graph, RFaceGroup faceGroup)
            {
                Work = work;
                Graph = graph;

                FaceGroup = faceGroup;
                Vertices = faceGroup.ComputeOutlineVertices(f => f.RoadTypes.HasAnyFlag(RRoadTypeMask.SideWalk) == false);

                // 時計回りになるように順番チェック
                if (GeoGraph2D.IsClockwise(Vertices.Select(x => x.Position.Xz())) == false)
                    Vertices.Reverse();
            }

            public void Build()
            {
                Node = CreateRoad();
            }

            public void BuildLine()
            {
                IsValid = BuildLines();
            }

            private RnRoadBase CreateRoad()
            {
                var cityObjectGroup = FaceGroup.CityObjectGroup;
                // 孤立
                if (RoadType == RoadType.Isolated)
                {
                    var way = Work.CreateWay(Vertices);
                    var road = RnRoad.CreateIsolatedRoad(cityObjectGroup, way);
                    return road;
                }

                // 行き止まり
                if (RoadType == RoadType.Terminate)
                {
                    var border = Lines.FirstOrDefault(l => l.IsBorder);
                    var line = Lines.FirstOrDefault(l => l.IsBorder == false);
                    if (line == null)
                    {
                        Debug.LogError($"不正なレーン構成[Terminate] : {cityObjectGroup.name}");
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
                    var road = RnRoad.CreateOneLaneRoad(cityObjectGroup, lane);
                    road.SetPrevNext(null, border.Neighbor?.Node);
                    return road;
                }

                // 通常の道
                if (RoadType == RoadType.Road)
                {
                    var lanes = Lines.Where(l => l.IsBorder == false).ToList();
                    var road = new RnRoad(cityObjectGroup);

                    // 同じPrev/Nextの組み合わせでグループ化(順序逆は問わない)
                    var pairs = lanes.GroupBy(l => RnEx.CombSet(l.Prev, l.Next))
                        .Select(g =>
                        {
                            var laneLines = g.ToList();
                            var (left, right) = (laneLines.ElementAtOrDefault(0), laneLines.ElementAtOrDefault(1));
                            if (left?.Way?.IsReversed ?? false)
                                (left, right) = (right, left);
                            return new { leftLine = left, rightLine = right };
                        }).ToList();
                    var pair = pairs.FirstOrDefault(p => p.leftLine != null && p.rightLine != null);
                    if (pair == null)
                        pair = pairs.FirstOrDefault();

                    //Assert.IsTrue(pair != null, $"不正なレーン構成 {cityObjectGroup.name}");
                    if (pair == null)
                        return road;
                    var (leftLine, rightLine) = (pair.leftLine, pair.rightLine);

                    if (leftLine == null && rightLine == null)
                    {
                        Debug.LogError($"不正なーレーン構成(Wayの存在しないLane) {cityObjectGroup.name}");
                        return road;
                    }

                    // ログだけ残しておく
                    if (leftLine == null || rightLine == null)
                        Debug.LogWarning($"不正なレーン構成(片側Wayのみ存在) {cityObjectGroup.name}");

                    var line = leftLine ?? rightLine;
                    var prevBorderLine = line?.Prev; ;
                    var nextBorderLine = line?.Next;
                    var prevBorderWay = prevBorderLine?.Way;
                    var nextBorderWay = nextBorderLine?.Way;

                    var leftWay = leftLine?.Way;
                    var rightWay = rightLine?.Way;

                    // 方向そろえる
                    if (leftLine != null && leftLine.Prev != prevBorderLine)
                        leftWay = leftWay.ReversedWay();
                    if (rightLine != null && rightLine.Prev != prevBorderLine)
                        rightWay = rightWay.ReversedWay();
                    if (rightWay != null)
                        rightWay.IsReverseNormal = true;

                    var lane = new RnLane(leftWay, rightWay, prevBorderWay, nextBorderWay);
                    road.AddMainLane(lane);
                    return road;
                }

                // 交差点
                if (RoadType == RoadType.Intersection)
                {
                    var intersection = new RnIntersection(cityObjectGroup);
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
                Debug.LogError($"不正なレーン構成 : {cityObjectGroup.name}");
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

                    var prevRoad = prev?.Neighbor?.Node;
                    var nextRoad = next?.Neighbor?.Node;
                    road.SetPrevNext(prevRoad, nextRoad);
                }
                else if (Node is RnIntersection intersection)
                {
                    // 境界線情報
                    foreach (var b in Lines.Where(l => l.IsBorder))
                    {
                        intersection.AddNeighbor(b.Neighbor.Node, b.Way);
                    }
                }
            }

            private bool BuildLines()
            {
                var edges = new REdge[Vertices.Count];
                // index : アウトラインの辺に隣接するTran
                var neighbors = new Tran[edges.Length];
                var success = true;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    var v0 = Vertices[i];
                    var v1 = Vertices[(i + 1) % Vertices.Count];

                    var e = v0.Edges.FirstOrDefault(e => e.IsSameVertex(v0, v1));
                    if (e == null)
                    {
                        Debug.LogError($"ループしていないメッシュ. {FaceGroup.CityObjectGroup.name}");
                        success = false;
                        continue;
                    }

                    edges[i] = e;
                    neighbors[i] = e.Faces.Select(f => Work.FindTranOrDefault(f)).FirstOrDefault(t => t != null && t != this);
                }

                var startIndex = 0;
                for (var i = 1; i < edges.Length; i++)
                {
                    var e = edges[i];
                    if (neighbors[i] != neighbors[0])
                    {
                        startIndex = i;
                        break;
                    }
                }

                Line line = null;
                foreach (var i in Enumerable.Range(0, edges.Length).Select(i => (i + startIndex) % edges.Length))
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
                // 道路/中央分離帯は一つのfaceGroupとしてまとめる
                var mask = ~(RRoadTypeMask.Road | RRoadTypeMask.Median);
                var faceGroups = graph.GroupBy((f0, f1) =>
                {
                    var m0 = f0.RoadTypes & mask;
                    var m1 = f1.RoadTypes & mask;
                    return m0 == m1;
                }).ToList();

                var ret = new RnModel();
                var work = new Work { terminateAllowEdgeAngle = terminateAllowEdgeAngle };
                foreach (var faceGroup in faceGroups)
                {
                    var roadType = faceGroup.RoadTypes;

                    // 道路を全く含まない時は無視
                    if (roadType.IsRoad() == false)
                        continue;
                    if (roadType.IsSideWalk())
                        continue;
                    // ignoreHighway=trueの時は高速道路も無視
                    if (roadType.IsHighWay() && ignoreHighway)
                        continue;
                    work.TranMap[faceGroup] = new Tran(work, graph, faceGroup);
                }

                // 作成したTranを元にRoadを作成
                foreach (var tran in work.TranMap.Values)
                    tran.BuildLine();
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
                    foreach (var fg in faceGroups)
                    {
                        foreach (var sideWalkFace in fg.Faces.Where(f => f.RoadTypes.IsSideWalk()))
                        {
                            if (sideWalkFace.CreateSideWalk(out var outsideEdges, out var insideEdges,
                                    out var startEdges, out var endEdges) == false)
                                continue;

                            RnWay AsWay(IReadOnlyList<REdge> edges, out bool isCached)
                            {
                                isCached = false;
                                if (edges.Any() == false)
                                    return null;
                                if (RGraphEx.SegmentEdge2Vertex(edges, out var vertices, out var isLoop))
                                    return work.CreateWay(vertices, out isCached);
                                return null;
                            }
                            var outsideWay = AsWay(outsideEdges, out var outsideCached);
                            var insideWay = AsWay(insideEdges, out var insideCached);
                            var startWay = AsWay(startEdges, out var startCached);
                            var endWay = AsWay(endEdges, out var endCached);
                            var parent = work.TranMap.Values.FirstOrDefault(t =>
                                t.FaceGroup.CityObjectGroup == sideWalkFace.CityObjectGroup && t.Node != null);
                            var sideWalk = RnSideWalk.Create(parent?.Node, outsideWay, insideWay, startWay, endWay);
                            ret.AddSideWalk(sideWalk);
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
                            if (checkMedian)
                            {
                                linkGroup.SetLaneCount(1, 1);
                                if (zeroWidthMedian == false)
                                    linkGroup.SetMedianWidth(width, LaneWayMoveOption.MoveBothWay);
                            }
                            foreach (var r in linkGroup.Roads)
                                visited.Add(r);
                        }
                    }
                }


                ret.SplitLaneByWidth(roadSize, out var failedLinks);
                ret.ReBuildIntersectionTracks();
                return Task.FromResult(ret);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public async Task<RnModel> CreateRnModelImplAsync(List<PLATEAUCityObjectGroup> cityObjectGroups, GameObject target)
        {
            await midStageData.ConvertCityObjectAsync(cityObjectGroups);
            var graph = midStageData.CreateGraph(graphFactory, target);
            var model = await CreateRnModelAsync(graph);
            return model;
        }

        public async Task<PLATEAURnStructureModel> CreateRnModelAsync(List<PLATEAUCityObjectGroup> cityObjectGroups, GameObject target)
        {
            if (!target)
                target = new GameObject("RoadNetworkStructure");
            var ret = target.GetComponent<PLATEAURnStructureModel>() ?? target.AddComponent<PLATEAURnStructureModel>();
            var model = await CreateRnModelImplAsync(cityObjectGroups, target);
            ret.RoadNetwork = model;
            return ret;
        }

        public async Task<PLATEAURnStructureModel> CreateRnModelAsync(RGraph graph, GameObject target)
        {
            if (!target)
                target = new GameObject("RoadNetworkStructure");
            var ret = target.GetComponent<PLATEAURnStructureModel>() ?? target.AddComponent<PLATEAURnStructureModel>();
            var model = await CreateRnModelAsync(graph);
            ret.RoadNetwork = model;
            return ret;
        }

        public void DebugDraw()
        {
            midStageData?.DebugDraw();
        }
    }
}