using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Assertions;
using Object = System.Object;

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
                    foreach (var poly in edge.Faces)
                        ret |= poly.RoadTypes;
                }

                return ret;
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
        public void SplitEdge(RVertex v)
        {
            var lastV1 = V1;
            SetVertex(VertexType.V1, v);
            var newEdge = new REdge(v, lastV1);
            foreach (var p in Faces)
            {
                p.AddEdge(newEdge);
            }
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
        private List<PLATEAUCityObjectGroup> cityObjectGroups = new List<PLATEAUCityObjectGroup>();

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
        private HashSet<RGraph> graphs = new HashSet<RGraph>();

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
        public IReadOnlyCollection<RGraph> Graphs => graphs;

        /// <summary>
        /// 関連するCityObjectGroup
        /// </summary>
        public IReadOnlyList<PLATEAUCityObjectGroup> CityObjectGroups => cityObjectGroups;

        // 有効なポリゴンかどうか
        public bool IsValid => edges.Count > 0;

        public RFace(RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadType, int lodLevel)
        {
            RoadTypes = roadType;
            LodLevel = lodLevel;
            AddCityObjectGroup(cityObjectGroup);
            AddGraph(graph);
        }

        public void AddCityObjectGroup(PLATEAUCityObjectGroup cityObjectGroup)
        {
            if (cityObjectGroups.Contains(cityObjectGroup))
                return;
            cityObjectGroups.Add(cityObjectGroup);
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
        /// 親グラフ参照追加
        /// </summary>
        /// <param name="graph"></param>
        public void AddGraph(RGraph graph)
        {
            if (graphs.Contains(graph))
                return;
            graphs.Add(graph);
        }

        /// <summary>
        /// 親グラフ削除
        /// </summary>
        /// <param name="graph"></param>
        public void RemoveGraph(RGraph graph)
        {
            graphs.Remove(graph);
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
            foreach (var co in cityObjectGroups)
            {
                dst.AddCityObjectGroup(co);
            }
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
            foreach (var g in graphs)
                g.RemoveFace(this);
            edges.Clear();
            graphs.Clear();
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
        public IEnumerable<REdge> GetAllEdge()
        {
            return Faces.SelectMany(p => p.Edges).Distinct();
        }

        /// <summary>
        /// 全Vertexを取得(重い)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetAllVertex()
        {
            return GetAllEdge().SelectMany(e => e.Vertices).Distinct();
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
            face.AddGraph(this);
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

        public static RGraph Create(List<ConvertedCityObject> cityObjects)
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
        /// 頂点をマージする
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="mergeEpsilon"></param>
        /// <param name="mergeCellLength"></param>
        public static void MergeVertices(this RGraph graph, float mergeEpsilon, int mergeCellLength)
        {
            var vertices = graph.GetAllVertex().ToList();

            var vertexTable = GeoGraphEx.MergeVertices(vertices.Select(v => v.Position), mergeEpsilon, mergeCellLength);
            var vertex2RVertex = vertexTable.Values.Distinct().ToDictionary(v => v, v => new RVertex(v));
            Debug.Log($"MergeVertices: {vertices.Count} -> {vertex2RVertex.Count + vertices.Count(v => vertexTable.ContainsKey(v.Position) == false)}");
            var hoge = vertices.Where(v => (v.Position.Xz() - new Vector2(-439.77f, -16.70f)).sqrMagnitude < 0.1f).ToList();
            foreach (var v in vertices)
            {
                if (vertexTable.TryGetValue(v.Position, out var dst))
                {
                    v.MergeTo(vertex2RVertex[dst]);
                }
            }
        }

        /// <summary>
        /// アウトライン頂点を計算する
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static List<RVertex> ComputeOutlineVertices(this RFace face)
        {
            return GeoGraph2D.ComputeOutline(
                face.Edges.SelectMany(e => e.Vertices).Distinct()
                , v => v.Position.GetTangent(AxisPlane.Xz)
                , v => v.GetNeighborVertices().Where(b => b.Edges.Any(e => e.Faces.Contains(face))),
                false);
        }
    }
}
