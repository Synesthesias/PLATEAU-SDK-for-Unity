using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork.Graph
{
    /// <summary>
    /// 道路タイプ
    /// </summary>
    [Flags]
    public enum RRoadTypeMask
    {
        /// <summary>
        /// 何もなし
        /// </summary>
        Empty = 0,
        /// <summary>
        /// 車道
        /// </summary>
        Road = 1 << 0,
        /// <summary>
        /// 歩道
        /// </summary>
        SideWalk = 1 << 1,
        /// <summary>
        /// 中央分離帯
        /// </summary>
        Median = 1 << 2,
        /// <summary>
        /// 高速道路
        /// </summary>
        HighWay = 1 << 3,
        /// <summary>
        /// 不正な値
        /// </summary>
        Undefined = 1 << 4,
        /// <summary>
        /// 全ての値
        /// </summary>
        All = ~0
    }

    public static class RRoadTypeEx
    {
        /// <summary>
        /// 車道部分
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsRoad(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.Road) != 0;
        }

        /// <summary>
        /// 交通道路
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsHighWay(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.HighWay) != 0;
        }

        /// <summary>
        /// 歩道
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsSideWalk(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.SideWalk) != 0;
        }

        /// <summary>
        /// 歩道
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsMedian(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.Median) != 0;
        }

        /// <summary>
        /// selfがflagのどれかを持っているかどうか
        /// </summary>
        /// <param name="self"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasAnyFlag(this RRoadTypeMask self, RRoadTypeMask flag)
        {
            return (self & flag) != 0;
        }
    }

    /// <summary>
    /// 接点
    /// </summary>
    [Serializable]
    public class RVertex : ARnParts<RVertex>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        /// <summary>
        /// 接続辺
        /// </summary>
        private HashSet<REdge> edges = new HashSet<REdge>();

        /// <summary>
        /// 位置
        /// </summary>
        [field: SerializeField]
        public Vector3 Position { get; set; }

        //----------------------------------
        // start: フィールド
        //----------------------------------

        /// <summary>
        /// 接続辺
        /// </summary>
        public IReadOnlyCollection<REdge> Edges => edges;

        public RRoadTypeMask TypeMask
        {
            get
            {
                var ret = RRoadTypeMask.Empty;
                foreach (var edge in Edges)
                {
                    foreach (var face in edge.Faces)
                        ret |= face.RoadTypes;
                }

                return ret;
            }
        }

        public IEnumerable<RFace> GetFaces()
        {
            foreach (var edge in Edges)
            {
                foreach (var face in edge.Faces)
                    yield return face;
            }
        }

        public RVertex(Vector3 v)
        {
            Position = v;
        }

        /// <summary>
        /// 基本呼び出し禁止. 接続辺追加
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(REdge edge)
        {
            if (edges.Contains(edge))
                return;

            edges.Add(edge);
        }

        /// <summary>
        /// 基本呼び出し禁止. 接続辺削除
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
        }

        /// <summary>
        /// 自分自身を外す.
        /// keepLinkがtrueの時は自分がいなくなっても接続頂点同士のEdgeを貼って接続が消えないようにする
        /// </summary>
        public void DisConnect()
        {
            // 自分を持っている辺を削除する
            foreach (var e in Edges.ToList())
                e.RemoveVertex(this);
        }

        /// <summary>
        /// 自分自身を解除するうえで, 今まであったつながりは残すようにする
        /// </summary>
        public void DisConnectWithKeepLink()
        {
            var neighbors = GetNeighborVertices().ToList();

            // 自分と繋がっている辺は一旦削除
            foreach (var e in Edges.ToList())
                e.DisConnect();

            // 貼りなおす
            for (var i = 0; i < neighbors.Count; i++)
            {
                var v0 = neighbors[i];
                if (v0 == null)
                    continue;
                for (var j = i; j < neighbors.Count; ++j)
                {
                    var v1 = neighbors[j];
                    if (v1 == null)
                        continue;
                    if (v0.IsNeighbor(v1))
                        continue;
                    // 新しい辺を作成する
                    var _ = new REdge(v0, v1);
                }
            }
        }

        /// <summary>
        /// 隣接頂点を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetNeighborVertices()
        {
            foreach (var edge in Edges)
            {
                if (edge.V0 == this)
                {
                    Assert.IsTrue(edge.V1 != this);
                    yield return edge.V1;
                }
                else
                {
                    Assert.IsTrue(edge.V0 != this);
                    yield return edge.V0;
                }
            }
        }

        /// <summary>
        /// otherとの直接の辺を持っているか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsNeighbor(RVertex other)
        {
            return Edges.Any(e => e.Vertices.Contains(other));
        }


        /// <summary>
        /// 自身をdstにマージする
        /// </summary>
        public void MergeTo(RVertex dst, bool checkEdgeMerge = true)
        {
            // srcに繋がっている辺に変更を通知する
            foreach (var e in Edges.ToList())
            {
                e.ChangeVertex(this, dst);
            }
            // 自分の接続は解除する
            DisConnect();
            if (checkEdgeMerge == false)
                return;
            // 同じ頂点を持っている辺もマージする
            var queue = dst.Edges.ToList();
            while (queue.Any())
            {
                var edge = queue[0];
                queue.RemoveAt(0);
                for (var i = 0; i < queue.Count;)
                {
                    if (edge.IsSameVertex(queue[i]))
                    {
                        queue[i].MergeTo(edge);
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }


    /// <summary>
    /// 辺
    /// </summary>
    [Serializable]
    public class REdge
        : ARnParts<REdge>
    {
        public enum VertexType
        {
            V0,
            V1,
        }

        //----------------------------------
        // start: フィールド
        //----------------------------------

        /// <summary>
        /// 接続面
        /// </summary>
        private HashSet<RFace> faces = new HashSet<RFace>();

        /// <summary>
        /// 構成頂点(2個)
        /// </summary>
        [SerializeField]
        private RVertex[] vertices = new RVertex[2];

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// 開始点
        /// </summary>
        public RVertex V0 => GetVertex(VertexType.V0);

        /// <summary>
        /// 終了点
        /// </summary>
        public RVertex V1 => GetVertex(VertexType.V1);

        /// <summary>
        /// 接続面
        /// </summary>
        public IReadOnlyCollection<RFace> Faces => faces;

        /// <summary>
        /// 構成頂点(2個)
        /// </summary>
        public IReadOnlyList<RVertex> Vertices => vertices;

        /// <summary>
        /// 有効な辺かどうか. 2つの頂点が存在して、かつ異なるかどうか
        /// </summary>
        public bool IsValid => V0 != null && V1 != null && V0 != V1;

        public REdge(RVertex v0, RVertex v1)
        {
            SetVertex(VertexType.V0, v0);
            SetVertex(VertexType.V1, v1);
        }

        /// <summary>
        /// 隣接しているEdgeを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<REdge> GetNeighborEdges()
        {
            if (V0 != null)
            {
                foreach (var e in V0.Edges)
                {
                    if (e != this)
                        yield return e;
                }
            }

            if (V1 != null && V1 != V0)
            {
                foreach (var e in V1.Edges)
                {
                    if (e != this)
                        yield return e;
                }
            }
        }

        /// <summary>
        /// 頂点ノード取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public RVertex GetVertex(VertexType type)
        {
            return vertices[(int)type];
        }

        /// <summary>
        /// 頂点ノードを差し替え
        /// </summary>
        /// <param name="type"></param>
        /// <param name="vertex"></param>
        public void SetVertex(VertexType type, RVertex vertex)
        {
            var old = vertices[(int)type];
            if (old == vertex)
                return;

            old?.RemoveEdge(this);
            vertices[(int)type] = vertex;
            vertex?.AddEdge(this);
        }

        /// <summary>
        /// 頂点from -> toに変更する
        /// fromを持っていない場合は無視
        /// 変更した結果両方ともtoになる場合は接続が解除される
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ChangeVertex(RVertex from, RVertex to)
        {
            if (V0 == from)
            {
                // 両方ともtoになる場合は接続が解除される
                if (V1 == to)
                    DisConnect();
                else
                    SetVertex(VertexType.V0, to);
            }

            if (V1 == from)
            {
                // 両方ともtoになる場合は接続が解除される
                if (V0 == to)
                    DisConnect();
                else
                    SetVertex(VertexType.V1, to);
            }
        }

        /// <summary>
        /// 基本呼び出し禁止. 隣接面追加
        /// </summary>
        /// <param name="face"></param>
        public void AddFace(RFace face)
        {
            if (faces.Contains(face))
                return;
            faces.Add(face);
        }

        /// <summary>
        /// 基本呼び出し禁止. 面のつながりを消す
        /// </summary>
        /// <param name="face"></param>
        public void RemoveFace(RFace face)
        {
            faces.Remove(face);
        }

        /// <summary>
        /// 頂点を削除する
        /// </summary>
        /// <param name="vertex"></param>
        public void RemoveVertex(RVertex vertex)
        {
            if (V0 == vertex)
                SetVertex(VertexType.V0, null);
            if (V1 == vertex)
                SetVertex(VertexType.V1, null);
        }

        /// <summary>
        /// 自分の接続を解除する
        /// </summary>
        public void DisConnect()
        {
            // 子に自分の接続を解除するように伝える
            foreach (var v in vertices.ToList())
            {
                v?.RemoveEdge(this);
            }

            // 親に自分の接続を解除するように伝える
            foreach (var p in faces.ToList())
            {
                p?.RemoveEdge(this);
            }

            faces.Clear();
            Array.Fill(vertices, null);
        }

        /// <summary>
        /// vで2つに分割する
        /// </summary>
        /// <param name="v"></param>
        public REdge SplitEdge(RVertex v)
        {
            var lastV1 = V1;
            SetVertex(VertexType.V1, v);
            var newEdge = new REdge(v, lastV1);
            foreach (var p in Faces)
            {
                p.AddEdge(newEdge);
            }

            return newEdge;
        }

        /// <summary>
        /// 同じ頂点を参照しているかどうか. (順序は問わない)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public bool IsSameVertex(RVertex v0, RVertex v1)
        {
            return (V0 == v0 && V1 == v1) || (V0 == v1 && V1 == v0);
        }

        public bool IsSameVertex(REdge other)
        {
            return IsSameVertex(other.V0, other.V1);
        }

        /// <summary>
        /// otherと共有している頂点があるかどうか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsShareAnyVertex(REdge other)
        {
            return V0 == other.V0 || V1 == other.V0 || V0 == other.V1 || V1 == other.V1;
        }

        /// <summary>
        /// 自身をdstにマージする
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="checkFaceMerge"></param>
        public void MergeTo(REdge dst, bool checkFaceMerge = true)
        {
            var addFaces =
                Faces.Where(poly => dst.Faces.Contains(poly) == false).ToList();
            foreach (var p in addFaces)
            {
                p.ChangeEdge(this, dst);
            }

            // 最後に自分の接続は解除する
            DisConnect();

            if (checkFaceMerge == false)
                return;
            var queue = dst.Faces.ToList();
            while (queue.Any())
            {
                var poly = queue[0];
                queue.RemoveAt(0);
                for (var i = 0; i < queue.Count;)
                {
                    if (poly.IsSameEdges(queue[i]))
                    {
                        queue[i].MergeTo(poly);
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 辺の集合
    /// </summary>
    [Serializable]
    public class RFace : ARnParts<RFace>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        /// <summary>
        /// 表示非表示
        /// </summary>
        [field: SerializeField]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 対応するCityObjectGroup
        /// </summary>
        [SerializeField]
        private PLATEAUCityObjectGroup cityObjectGroup = null;

        /// <summary>
        /// 道路タイプ
        /// </summary>
        [field: SerializeField]
        public RRoadTypeMask RoadTypes { get; set; }

        /// <summary>
        /// LodLevel
        /// </summary>
        [field: SerializeField]
        public int LodLevel { get; set; }

        /// <summary>
        /// 親グラフ
        /// </summary>
        private RGraph graph = null;

        /// <summary>
        /// 構成辺
        /// </summary>
        private HashSet<REdge> edges = new HashSet<REdge>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// 構成辺
        /// </summary>
        public IReadOnlyCollection<REdge> Edges => edges;

        /// <summary>
        /// 接続面
        /// </summary>
        public RGraph Graph => graph;

        /// <summary>
        /// 関連するCityObjectGroup
        /// </summary>
        public PLATEAUCityObjectGroup CityObjectGroup => cityObjectGroup;

        // 有効なポリゴンかどうか
        public bool IsValid => edges.Count > 0;

        public RFace(RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadType, int lodLevel)
        {
            RoadTypes = roadType;
            LodLevel = lodLevel;
            this.cityObjectGroup = cityObjectGroup;
            this.graph = graph;
        }

        /// <summary>
        /// 辺追加
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(REdge edge)
        {
            if (edges.Contains(edge))
                return;

            edges.Add(edge);
            edge.AddFace(this);
        }

        /// <summary>
        /// 親グラフ削除
        /// </summary>
        /// <param name="g"></param>
        public void RemoveGraph(RGraph g)
        {
            if (Graph == g)
                graph = null;
        }

        /// <summary>
        /// 親グラフ差し替え
        /// </summary>
        /// <param name="g"></param>
        public void SetGraph(RGraph g)
        {
            if (graph == g)
                return;

            graph?.RemoveFace(this);
            graph = g;
        }

        ///// <summary>
        ///// 基本呼ぶの禁止. edgeをposの後ろに追加する
        ///// </summary>
        ///// <param name="edge"></param>
        ///// <param name="pos"></param>
        //public void InsertEdge(REdge edge, REdge pos)
        //{
        //    if (edges.Contains(edge))
        //        return;
        //    var index = edges.IndexOf(pos);
        //    edges.Insert(index + 1, edge);
        //    edge.AddFace(this);
        //}

        /// <summary>
        /// 辺削除
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
            // 子から自分を削除
            edge.RemoveFace(this);
        }

        /// <summary>
        /// 辺の変更
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ChangeEdge(REdge from, REdge to)
        {
            RemoveEdge(from);
            AddEdge(to);
        }

        /// <summary>
        /// 同じEdgeで構成されているかどうか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameEdges(RFace other)
        {
            if (edges.Count != other.edges.Count)
                return false;
            return other.Edges.All(e => Edges.Contains(e));
        }

        /// <summary>
        /// 基本呼び出し禁止. 自身をdstにマージする
        /// </summary>
        /// <param name="dst"></param>
        public void MergeTo(RFace dst)
        {
            dst.RoadTypes |= RoadTypes;

            if (dst.CityObjectGroup && dst.CityObjectGroup != CityObjectGroup)
                throw new InvalidDataException("CityObjectGroupが異なる場合はマージできません");
            dst.cityObjectGroup = CityObjectGroup;
            dst.LodLevel = Mathf.Max(dst.LodLevel, LodLevel);
            DisConnect();
        }


        /// <summary>
        /// 自分の接続を解除する
        /// </summary>
        public void DisConnect()
        {
            // 子に自分の接続を解除するように伝える
            foreach (var e in Edges)
                e.RemoveFace(this);

            // 親に自分の接続を解除するように伝える
            Graph?.RemoveFace(this);

            edges.Clear();
        }

    }

    [Serializable]
    public class RGraph : ARnParts<RGraph>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        private HashSet<RFace> faces = new HashSet<RFace>();

        //----------------------------------
        // end: フィールド
        //----------------------------------
        /// <summary>
        /// 面
        /// </summary>
        public IReadOnlyCollection<RFace> Faces => faces;

        /// <summary>
        /// 全Edgeを取得(重い)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<REdge> GetAllEdges()
        {
            return Faces.SelectMany(p => p.Edges).Distinct();
        }

        /// <summary>
        /// 全Vertexを取得(重い)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetAllVertices()
        {
            return GetAllEdges().SelectMany(e => e.Vertices).Distinct();
        }

        /// <summary>
        /// 親Face追加
        /// </summary>
        /// <param name="face"></param>
        public void AddFace(RFace face)
        {
            if (face == null)
                return;
            if (faces.Contains(face))
                return;
            faces.Add(face);
            face.SetGraph(this);
        }

        /// <summary>
        /// 親Face削除
        /// </summary>
        /// <param name="face"></param>
        public void RemoveFace(RFace face)
        {
            faces.Remove(face);
            face?.RemoveGraph(this);
        }
    }

    public static class RVertexEx
    {
        /// <summary>
        /// selfに対して、指定したCityObjectGroupに紐づくTypeを取得する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cog"></param>
        /// <returns></returns>
        public static RRoadTypeMask GetRoadType(this RVertex self, PLATEAUCityObjectGroup cog)
        {
            RRoadTypeMask roadType = RRoadTypeMask.Empty;
            foreach (var face in self.GetFaces().Where(f => f.CityObjectGroup == cog))
            {
                roadType |= face.RoadTypes;
            }
            return roadType;
        }
    }

    public static class RGraphEx
    {
        public readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public RVertex V0 { get; }
            public RVertex V1 { get; }

            public EdgeKey(RVertex v0, RVertex v1)
            {
                V0 = v0;
                V1 = v1;
                if (V0.GetHashCode() > V1.GetHashCode())
                {
                    (V0, V1) = (V1, V0);
                }
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(V0, V1);
            }

            public bool Equals(EdgeKey other)
            {
                // V0/V1が逆でも同じとみなす
                if (Equals(V0, other.V0) && Equals(V1, other.V1))
                    return true;

                if (Equals(V0, other.V1) && Equals(V1, other.V0))
                    return true;

                return false;
            }
        }

        public static RGraph Create(List<SubDividedCityObject> cityObjects)
        {
            var graph = new RGraph();
            Dictionary<Vector3, RVertex> vertexMap = new Dictionary<Vector3, RVertex>();
            Dictionary<EdgeKey, REdge> edgeMap = new Dictionary<EdgeKey, REdge>();
            foreach (var cityObject in cityObjects)
            {
                var root = cityObject.CityObjects.rootCityObjects[0];
                var lodLevel = cityObject.CityObjectGroup.GetLodLevel();
                var roadType = root.GetRoadType();

                foreach (var mesh in cityObject.Meshes)
                {
                    var face = new RFace(graph, cityObject.CityObjectGroup, roadType, lodLevel);
                    var vertices = mesh.Vertices.Select(v =>
                    {
                        return vertexMap.GetValueOrCreate(v, k => new RVertex(k));
                    }).ToList();
                    foreach (var s in mesh.SubMeshes)
                    {
                        var separated = s.Separate();

                        foreach (var m in separated)
                        {
                            for (var i = 0; i < m.Triangles.Count; i += 3)
                            {
                                var e0 = edgeMap.GetValueOrCreate(new EdgeKey(vertices[m.Triangles[i]], vertices[m.Triangles[i + 1]])
                                    , e => new REdge(e.V0, e.V1));
                                var e1 = edgeMap.GetValueOrCreate(new EdgeKey(vertices[m.Triangles[i + 1]], vertices[m.Triangles[i + 2]])
                                    , e => new REdge(e.V0, e.V1));
                                var e2 = edgeMap.GetValueOrCreate(new EdgeKey(vertices[m.Triangles[i + 2]], vertices[m.Triangles[i]])
                                    , e => new REdge(e.V0, e.V1));
                                var edges = new[] { e0, e1, e2 };
                                foreach (var e in edges)
                                    face.AddEdge(e);
                            }
                        }
                    }

                    graph.AddFace(face);
                }
            }
            return graph;
        }

        /// <summary>
        /// 頂点をリダクション処理
        /// </summary>
        /// <param name="self"></param>
        /// <param name="mergeCellSize"></param>
        /// <param name="mergeCellLength"></param>
        /// <param name="midPointTolerance">aとcとしか接続していない点bに対して、a-cの直線との距離がこれ以下だとbをマージする</param>
        public static void VertexReduction(this RGraph self, float mergeCellSize, int mergeCellLength, float midPointTolerance)
        {
            {
                var vertices = self.GetAllVertices().ToList();

                var vertexTable = GeoGraphEx.MergeVertices(vertices.Select(v => v.Position), mergeCellSize, mergeCellLength);
                var vertex2RVertex = vertexTable.Values.Distinct().ToDictionary(v => v, v => new RVertex(v));
                Debug.Log($"MergeVertices: {vertices.Count} -> {vertex2RVertex.Count + vertices.Count(v => vertexTable.ContainsKey(v.Position) == false)}");
                foreach (var v in vertices)
                {
                    if (vertexTable.TryGetValue(v.Position, out var dst))
                    {
                        v.MergeTo(vertex2RVertex[dst]);
                    }
                }
            }

            while (true)
            {
                var vertices = self.Faces
                    .SelectMany(f => f.Edges)
                    .SelectMany(e => e.Vertices)
                    .Where(v => v.Edges.Count == 2)
                    .Distinct()
                    .ToList();

                var sqrLen = midPointTolerance * midPointTolerance;
                var count = 0;
                foreach (var v in vertices)
                {
                    var neighbor = v.GetNeighborVertices().ToList();
                    if (neighbor.Count != 2)
                        continue;

                    // 中間点があってもほぼ直線だった場合は中間点は削除する
                    var segment = new LineSegment3D(neighbor[0].Position, neighbor[1].Position);
                    var p = segment.GetNearestPoint(v.Position);
                    if ((p - v.Position).sqrMagnitude < sqrLen)
                    {
                        v.MergeTo(neighbor[0]);
                        count++;
                    }
                }
                Debug.Log($"RemoveMidPoint : {vertices.Count} -> {vertices.Count - count}");
                if (count == 0)
                    break;
            }
        }

        /// <summary>
        /// 辺のリダクション処理（同じ頂点を持つ辺をマージする)
        /// </summary>
        /// <param name="self"></param>
        public static void EdgeReduction(this RGraph self)
        {
            var edges = self.GetAllEdges().ToList();
            var edgeTable = new Dictionary<EdgeKey, HashSet<REdge>>();

            foreach (var e in edges)
            {
                var key = new EdgeKey(e.V0, e.V1);
                edgeTable.GetValueOrCreate(key).Add(e);
            }
            Debug.Log($"MergeEdges: {edges.Count} -> {edgeTable.Count}");
            foreach (var e in edgeTable.Where(e => e.Value.Count > 1))
            {
                var dst = e.Value.First();
                var remove = e.Value.Skip(1).ToList();

                foreach (var r in remove)
                {
                    r.MergeTo(dst);
                }
            }
        }

        /// <summary>
        /// 交差する辺の交点に頂点を追加する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="heightTolerance">交点が高さ方向にずれていた時の許容量</param>
        public static void InsertVerticesInEdgeIntersection(this RGraph self, float heightTolerance)
        {
            var vertices = self.GetAllVertices().ToList();

            var comp = Comparer<float>.Default;
            int Compare(RVertex v0, RVertex v1)
            {
                var x = comp.Compare(v0.Position.x, v1.Position.x);
                if (x != 0)
                    return x;
                var z = comp.Compare(v0.Position.z, v1.Position.z);
                if (z != 0)
                    return z;
                return comp.Compare(v0.Position.y, v1.Position.y);
            }
            vertices.Sort(Compare);

            var queue = new HashSet<REdge>();

            Dictionary<REdge, HashSet<RVertex>> edgeInsertMap = new();
            Dictionary<Vector3, RVertex> vertexMap = new();
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                // 新規追加分
                var addEdges = new List<REdge>();
                var removeEdges = new HashSet<REdge>();
                foreach (var e in v.Edges)
                {
                    // vと反対側の点を見る
                    var o = e.V0 == v ? e.V1 : e.V0;
                    var d = Compare(v, o);
                    // vが開始点の辺を追加する
                    if (d < 0)
                        addEdges.Add(e);
                    // vが終了点の辺を取り出す
                    else if (d > 0)
                        removeEdges.Add(e);
                }
                bool NearlyEqual(float a, float b)
                {
                    return Mathf.Abs(a - b) < 1e-3f;
                }

                // 今回除かれる線分同士はチェックしない(vで交差しているから)
                var targets = queue.Where(e =>
                {
                    // vを端点に持つ辺は無視(端点で
                    if (e.V0 == v || e.V1 == v)
                        return false;
                    return removeEdges.Contains(e) == false;
                }).ToList();
                foreach (var e0 in removeEdges)
                {
                    var s0 = new LineSegment3D(e0.V0.Position, e0.V1.Position);
                    foreach (var e1 in targets)
                    {
                        // e0とe1が共有している頂点がある場合は無視
                        if (e0.IsShareAnyVertex(e1))
                            continue;

                        var s1 = new LineSegment3D(e1.V0.Position, e1.V1.Position);
                        if (s0.TrySegmentIntersectionBy2D(s1, AxisPlane.Xz, heightTolerance, out var intersection,
                                out var t1, out var t2))
                        {
                            // お互いの端点で交差している場合は無視
                            if ((NearlyEqual(t1, 0) || NearlyEqual(t1, 1)) && (NearlyEqual(t2, 0) || NearlyEqual(t2, 1)))
                                continue;
                            var p = vertexMap.GetValueOrCreate(intersection, k => new RVertex(k));
                            // #TODO : 0 or 1で交差した場合を考慮
                            edgeInsertMap.GetValueOrCreate(e0).Add(p);
                            edgeInsertMap.GetValueOrCreate(e1).Add(p);
                            DebugEx.DrawSphere(intersection, 0.1f, Color.red);
                        }
                    }
                }

                foreach (var add in addEdges)
                    queue.Add(add);

                foreach (var remove in removeEdges)
                    queue.Remove(remove);
            }

            Debug.Log($"Add Vertex [{vertexMap.Count}]");
            foreach (var e in edgeInsertMap)
            {
                var sortedV = e.Value.OrderBy(p => (p.Position - e.Key.V0.Position).sqrMagnitude).ToList();

                var edge = e.Key;
                foreach (var v in sortedV)
                {
                    edge = edge.SplitEdge(v);
                }
            }
        }

        public static void SeparateFaces(this RGraph self)
        {
            foreach (var p in self.Faces.ToList())
            {
                p.Separate();
            }
        }

        /// <summary>
        /// アウトライン頂点を計算する
        /// </summary>
        /// <param name="face"></param>
        /// <param name="roadTypes"></param>
        /// <returns></returns>
        public static List<RVertex> ComputeOutlineVertices(this RFace face, RRoadTypeMask roadTypes)
        {
            var vertices = face.Edges.SelectMany(e => e.Vertices).Where(v => v.TypeMask.HasAnyFlag(roadTypes)).ToHashSet();
            var res = GeoGraph2D.ComputeOutline(
                vertices
                , v => v.Position
                , AxisPlane.Xz
                , v => v.GetNeighborVertices().Where(b => vertices.Contains(b)));
            return res.Outline ?? new List<RVertex>();
        }

        /// <summary>
        /// CityObjectGroupに属する面のアウトライン頂点を計算する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cityObjectGroup"></param>
        /// <param name="roadTypes"></param>
        /// <returns></returns>
        public static List<RVertex> ComputeOutlineVerticesByCityObjectGroup(this RGraph self, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadTypes, out List<RFace> faces)
        {
            faces = self.Faces.Where(p => p.CityObjectGroup == cityObjectGroup).ToList();
            var vertices = faces.SelectMany(f => f.Edges.SelectMany(e => e.Vertices))
                .Where(v =>
                {
                    RRoadTypeMask roadType = 0;
                    foreach (var face in v.GetFaces())
                    {
                        if (face.CityObjectGroup != cityObjectGroup)
                            continue;
                        roadType |= face.RoadTypes;
                    }
                    return roadType.HasAnyFlag(roadTypes);
                })
                .ToHashSet();
            var res = GeoGraph2D.ComputeOutline(
                vertices
                , v => v.Position
                , AxisPlane.Xz
                , v => v.GetNeighborVertices().Where(b => vertices.Contains(b))
            );
            return res.Outline ?? new List<RVertex>();
        }

        /// <summary>
        /// selfの非連結な部分を分離する
        /// </summary>
        /// <param name="self"></param>
        public static void Separate(this RFace self)
        {
            var edges = self.Edges.ToHashSet();
            if (edges.Any() == false)
                return;
            List<HashSet<REdge>> separatedEdges = new();
            while (edges.Any())
            {
                var queue = new Queue<REdge>();
                queue.Enqueue(edges.First());
                edges.Remove(edges.First());
                var subFace = new HashSet<REdge>();
                while (queue.Any())
                {
                    var edge = queue.Dequeue();
                    subFace.Add(edge);
                    foreach (var e in edge.GetNeighborEdges())
                    {
                        if (edges.Contains(e))
                        {
                            edges.Remove(e);
                            queue.Enqueue(e);
                        }
                    }
                }
                separatedEdges.Add(subFace);
            }

            if (separatedEdges.Count <= 1)
                return;

            foreach (var e in self.Edges.Where(e => separatedEdges[0].Contains(e) == false).ToList())
                self.RemoveEdge(e);

            for (var i = 1; i < separatedEdges.Count; i++)
            {
                var face = new RFace(self.Graph, self.CityObjectGroup, self.RoadTypes, self.LodLevel);
                foreach (var e in separatedEdges[i])
                    face.AddEdge(e);
                self.Graph.AddFace(face);
            }
        }
    }

}
