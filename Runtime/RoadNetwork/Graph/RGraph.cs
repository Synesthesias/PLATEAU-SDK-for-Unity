using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.UI;
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
        private List<REdge> edges = new List<REdge>();

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
        public IReadOnlyList<REdge> Edges => edges;

        public RRoadTypeMask TypeMask
        {
            get
            {
                var ret = RRoadTypeMask.Empty;
                foreach (var edge in Edges)
                {
                    foreach (var poly in edge.Polygons)
                        ret |= poly.RoadType;
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
        /// <param name="keepLink"></param>
        public void DisConnect(bool keepLink)
        {
            var neighbors = GetNeighborVertices().ToList();
            foreach (var e in Edges)
            {
                e.DisConnect();
            }

            if (keepLink)
            {
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


    }


    /// <summary>
    /// 辺
    /// </summary>
    [Serializable]
    public class REdge : ARnParts<REdge>
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
        private List<RPolygon> polygons = new List<RPolygon>();

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
        public IReadOnlyList<RPolygon> Polygons => polygons;

        /// <summary>
        /// 構成頂点(2個)
        /// </summary>
        public IReadOnlyList<RVertex> Vertices => vertices;

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
        /// <param name="polygon"></param>
        public void AddPolygon(RPolygon polygon)
        {
            if (polygons.Contains(polygon))
                return;
            polygons.Add(polygon);
        }

        /// <summary>
        /// 基本呼び出し禁止. 面のつながりを消す
        /// </summary>
        /// <param name="polygon"></param>
        public void RemovePolygon(RPolygon polygon)
        {
            polygons.Remove(polygon);
        }

        /// <summary>
        /// 自分の接続を解除する
        /// </summary>
        public void DisConnect()
        {
            foreach (var v in vertices)
            {
                v?.RemoveEdge(this);
            }

            // 親に自分の接続を解除するように伝える
            foreach (var p in polygons)
            {
                p?.RemoveEdge(this);
            }

            polygons.Clear();
            Array.Fill(vertices, null);
        }



        /// <summary>
        /// edgeをvで2つに分割する
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="v"></param>
        public void SplitEdge(RVertex v)
        {
            var lastV1 = V1;
            SetVertex(VertexType.V1, v);
            var newEdge = new REdge(v, lastV1);
            foreach (var p in Polygons)
            {
                p.InsertEdge(newEdge, this);
            }
        }
    }

    /// <summary>
    /// 多角形
    /// </summary>
    [Serializable]
    public class RPolygon : ARnParts<RPolygon>
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
        [field: SerializeField]
        public PLATEAUCityObjectGroup CityObjectGroup { get; set; }

        /// <summary>
        /// 道路タイプ
        /// </summary>
        [field: SerializeField]
        public RRoadTypeMask RoadType { get; set; }

        /// <summary>
        /// LodLevel
        /// </summary>
        [field: SerializeField]
        public int LodLevel { get; set; }

        /// <summary>
        /// 親グラフ
        /// </summary>
        public RGraph Graph { get; private set; }

        /// <summary>
        /// 構成辺
        /// </summary>
        private List<REdge> edges = new List<REdge>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// 構成辺
        /// </summary>
        public IReadOnlyList<REdge> Edges => edges;

        // 有効なポリゴンかどうか
        public bool IsValid => Edges.Count >= 3;

        public RPolygon(RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadType, int lodLevel)
        {
            Graph = graph;
            CityObjectGroup = cityObjectGroup;
            RoadType = roadType;
            LodLevel = lodLevel;
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
            edge.AddPolygon(this);
        }

        /// <summary>
        /// 基本呼ぶの禁止. edgeをposの後ろに追加する
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="pos"></param>
        public void InsertEdge(REdge edge, REdge pos)
        {
            if (edges.Contains(edge))
                return;
            var index = edges.IndexOf(pos);
            edges.Insert(index + 1, edge);
            edge.AddPolygon(this);
        }

        /// <summary>
        /// 辺削除
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
            edge.RemovePolygon(this);
        }

    }

    [Serializable]
    public class RGraph : ARnParts<RGraph>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        private List<RPolygon> polygons = new List<RPolygon>();

        //----------------------------------
        // end: フィールド
        //----------------------------------
        /// <summary>
        /// 面
        /// </summary>
        public IReadOnlyList<RPolygon> Polygons => polygons;

        /// <summary>
        /// 全Edgeを取得(重い)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<REdge> GetAllEdge()
        {
            return Polygons.SelectMany(p => p.Edges).Distinct();
        }

        /// <summary>
        /// 全Vertexを取得(重い)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetAllVertex()
        {
            return GetAllEdge().SelectMany(e => e.Vertices).Distinct();
        }



        public void AddPolygon(RPolygon polygon)
        {
            polygons.Add(polygon);
        }

        public void RemovePolygon(RPolygon polygon)
        {
            polygons.Remove(polygon);
        }

        /// <summary>
        /// srcをdstにマージする
        /// </summary>
        public void MergeVertex(RVertex src, RVertex dst)
        {
            // srcに繋がっている辺に変更を通知する
            foreach (var e in src.Edges)
            {
                e.ChangeVertex(src, dst);
            }
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
            Dictionary<EdgeKey, REdge> edgeMap = new Dictionary<EdgeKey, REdge>();
            foreach (var cityObject in cityObjects)
            {
                var root = cityObject.CityObjects.rootCityObjects[0];
                var lodLevel = cityObject.CityObjectGroup.GetLodLevel();
                var roadType = root.GetRoadType();

                foreach (var mesh in cityObject.Meshes)
                {
                    var polygon = new RPolygon(graph, cityObject.CityObjectGroup, roadType, lodLevel);
                    var vertices = mesh.Vertices.Select(v => new RVertex(v)).ToList();
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
                                    polygon.AddEdge(e);
                            }
                        }
                    }

                    graph.AddPolygon(polygon);
                }
            }
            return graph;
        }

        public static void MergeVertices(this RGraph graph, float mergeEpsilon, int mergeCellLength)
        {
            var vertices = graph.GetAllVertex().ToList();

            var vertexTable = GeoGraphEx.MergeVertices(vertices.Select(v => v.Position), mergeEpsilon, mergeCellLength);
            var vertex2RVertex = vertexTable.Values.Distinct().ToDictionary(v => v, v => new RVertex(v));
            foreach (var v in vertices)
            {
                if (vertexTable.TryGetValue(v.Position, out var dst))
                {
                    graph.MergeVertex(v, vertex2RVertex[dst]);
                }
            }
        }
    }
}
