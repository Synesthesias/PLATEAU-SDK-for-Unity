using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
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
        // 中央分離帯と隣接
        Median = 1 << 0,
        // 歩道と隣接
        SideWalk = 1 << 2,
    }

    /// <summary>
    /// 接点
    /// </summary>
    public class RVertex
    {
        private List<REdge> edges = new List<REdge>();

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 接続辺
        /// </summary>
        public IReadOnlyList<REdge> Edges => edges;

        /// <summary>
        /// 頂点属性
        /// </summary>
        public RVertexType Types { get; set; }

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
                if (edge.Start == this)
                {
                    Assert.IsTrue(edge.End != this);
                    yield return edge.End;
                }
                else
                {
                    Assert.IsTrue(edge.Start != this);
                    yield return edge.Start;
                }
            }
        }
    }


    /// <summary>
    /// 辺
    /// </summary>
    public class REdge
    {
        public enum VertexType
        {
            Start,
            End,
        }

        private List<RPolygon> polygons = new List<RPolygon>();

        private RVertex[] vertices = new RVertex[2];

        /// <summary>
        /// 開始点
        /// </summary>
        public RVertex Start => GetVertex(VertexType.Start);

        /// <summary>
        /// 終了点
        /// </summary>
        public RVertex End => GetVertex(VertexType.End);

        /// <summary>
        /// 接続面
        /// </summary>
        public IReadOnlyList<RPolygon> Polygons => polygons;

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
    }

    /// <summary>
    /// 多角形
    /// </summary>
    public class RPolygon
    {
        readonly List<RVertex> vertices = new List<RVertex>();

        readonly List<REdge> edges = new List<REdge>();

        private readonly PLATEAUCityObjectGroup cityObjectGroup = null;

        /// <summary>
        /// 構成辺. 時計回りに接続順
        /// </summary>
        public IReadOnlyList<REdge> Edges => edges;

        /// <summary>
        /// 構成頂点
        /// </summary>
        public IReadOnlyList<RVertex> Vertices => vertices;

        /// <summary>
        /// 対応するCityObjectGroup
        /// </summary>
        public PLATEAUCityObjectGroup CityObjectGroup => cityObjectGroup;

        /// <summary>
        /// 親グラフ
        /// </summary>
        public RGraph Graph { get; set; }
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
