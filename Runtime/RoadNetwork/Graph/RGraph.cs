using PLATEAU.CityInfo;
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
    /// 頂点属性
    /// </summary>
    [Flags]
    public enum RVertexType
    {
        /// <summary>
        /// 中央分離帯と隣接
        /// </summary>
        Median = 1 << 0,
        /// <summary>
        /// 歩道と隣接
        /// </summary>
        SideWalk = 1 << 2,
    }

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
    public class RVertex
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        [SerializeField]
        private List<REdge> edges = new List<REdge>();

        /// <summary>
        /// 位置
        /// </summary>
        [field: SerializeField]
        public Vector3 Position { get; set; }

        /// <summary>
        /// 頂点属性
        /// </summary>
        [field: SerializeField]
        public RVertexType Types { get; set; }

        //----------------------------------
        // start: フィールド
        //----------------------------------

        /// <summary>
        /// 接続辺
        /// </summary>
        public IReadOnlyList<REdge> Edges => edges;

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
        /// 頂点属性typeが有効かどうかを設定する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="enable"></param>
        public void SetAttributeEnable(RVertexType type, bool enable)
        {
            if (enable)
            {
                Types |= type;
            }
            else
            {
                Types &= ~type;
            }
        }

        /// <summary>
        /// 頂点属性typeが有効かどうかを取得する
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool GetAttributeEnable(RVertexType type)
        {
            return (Types & type) != 0;
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
    public class REdge
    {
        public enum VertexType
        {
            V0,
            V1,
        }

        //----------------------------------
        // start: フィールド
        //----------------------------------
        [SerializeField]
        private List<RPolygon> polygons = new List<RPolygon>();

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
        /// 接続頂点(2個)
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
        }
    }

    /// <summary>
    /// 多角形
    /// </summary>
    [Serializable]
    public class RPolygon
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        /// <summary>
        /// 表示非表示
        /// </summary>
        [field: SerializeField]
        public bool Visible { get; set; }

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
        [field: SerializeField]
        public RGraph Graph { get; private set; }

        /// <summary>
        /// 構成辺
        /// </summary>
        [SerializeField]
        private List<REdge> edges = new List<REdge>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        /// <summary>
        /// 構成辺
        /// </summary>
        public IReadOnlyList<REdge> Edges => edges;

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
        /// 辺削除
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
            edge.RemovePolygon(this);
        }

    }

    public class RGraph
    {
        /// <summary>
        /// 頂点
        /// </summary>
        public List<RVertex> Vertices { get; set; }

        /// <summary>
        /// 辺
        /// </summary>
        public List<REdge> Edges { get; set; }

        /// <summary>
        /// 面
        /// </summary>
        public List<RPolygon> Faces { get; set; }
    }
}
