using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PLATEAU.RoadNetwork.Data;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// ノードを表すクラス
    /// </summary>
    public class RoadNetworkElementNode : RoadNetworkElement
    {
        /// <summary>
        /// ノード識別子のプレフィックス
        /// </summary>
        public static readonly string IDPrefix = "Node";

        /// <summary>
        /// 道路ネットワーク上のインデックス
        /// </summary>
        public int RoadNetworkIndex { get; private set; } = -1;

        /// <summary>
        /// ノードの座標
        /// IsVertualがtrueの場合は、この座標が使用される。
        /// </summary>
        public Vector3 Coord;

        /// <summary>
        /// ノードに所属するトラックのリスト
        /// </summary>
        public List<RoadNetworkElementTrack> Tracks = new List<RoadNetworkElementTrack>();

        /// <summary>
        /// 元となる道路ネットワーク上の交差点
        /// </summary>
        public RnDataIntersection OriginNode
        {
            get
            {
                if (RoadNetworkIndex < 0)
                {
                    return null;
                }

                return roadNetworkContext.RoadNetworkGetter.GetRoadBases()[RoadNetworkIndex] as RnDataIntersection;
            }
        }

        /// <summary>
        /// ノードが仮想ノードかどうかを返します
        /// </summary>
        public bool IsVirtual
        {
            get
            {
                return RoadNetworkIndex < 0;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        /// <param name="id">ノードのID</param>
        /// <param name="index">道路ネットワークのインデックス</param>
        public RoadNetworkElementNode(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
            RoadNetworkIndex = index;
        }

        /// <summary>
        /// ノードの識別子を生成します
        /// </summary>
        /// <param name="id">元のID</param>
        /// <returns>生成されたID</returns>
        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        /// <summary>
        /// ノードに属するするトラックを生成します
        /// </summary>
        /// <param name="links">リンクのリスト</param>
        public void GenerateTrack(List<RoadNetworkElementLink> links)
        {
            Tracks = new List<RoadNetworkElementTrack>();

            if (OriginNode == null)
            {
                return;
            }

            foreach (var track in OriginNode.Tracks)
            {
                Tracks.Add(new RoadNetworkElementTrack(roadNetworkContext, ID.Replace(IDPrefix, ""), track, Tracks, links));
            }
        }

        /// <summary>
        /// ジオメトリ情報を取得します
        /// </summary>
        /// <returns>ジオメトリ情報</returns>
        public GeoJSON.Net.Geometry.Position GetGeometory()
        {
            Vector3 coord = Coord;

            if (!IsVirtual && OriginNode != null)
            {
                coord = GetPosition();
            }

            var geoCoord = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(coord.x, coord.y, coord.z));

            return new GeoJSON.Net.Geometry.Position(geoCoord.Latitude, geoCoord.Longitude);
        }

        /// <summary>
        /// ノードの位置を取得します
        /// </summary>
        /// <returns>ノードの位置</returns>
        public Vector3 GetPosition()
        {
            var coods = new List<Vector3>();

            var all_ways = roadNetworkContext.RoadNetworkGetter.GetWays();

            var all_linestrings = roadNetworkContext.RoadNetworkGetter.GetLineStrings();

            var all_points = roadNetworkContext.RoadNetworkGetter.GetPoints();

            var neighbors = OriginNode.Neighbors;

            var ways = neighbors.Select(n => n.Border).ToList();

            foreach (var way in ways)
            {
                if (!way.IsValid) continue;

                var linestring = all_ways[way.ID].LineString;

                var line = all_linestrings[linestring.ID].Points;

                foreach (var point in line)
                {
                    if (!point.IsValid) continue;

                    var vertex = all_points[point.ID].Vertex;

                    coods.Add(new Vector3(vertex.x, vertex.y, vertex.z));
                }
            }

            // 重心を求める
            var coord = new Vector3(coods.Average(c => c.x), coods.Average(c => c.y), coods.Average(c => c.z));

            return coord;
        }
    }
}