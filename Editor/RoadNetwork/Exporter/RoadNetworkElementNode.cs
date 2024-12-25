using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// シミュレーション用道路ネットワークのノードを表すクラス
    /// </summary>
    public class RoadNetworkElementNode : RoadNetworkElement
    {
        public static readonly string IDPrefix = "Node";

        public int RoadNetworkIndex { get; private set; } = -1;

        public Vector3 Coord;

        public List<RoadNetworkElementTrack> Tracks = new List<RoadNetworkElementTrack>();

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

        public PLATEAUCityObjectGroup OriginTran
        {
            get
            {
                return OriginNode?.TargetTran;
            }
        }

        public Mesh Mesh
        {
            get
            {
                return OriginTran?.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        public bool IsVirtual
        {
            get
            {
                return RoadNetworkIndex < 0;
            }
        }

        public RoadNetworkElementNode(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
            RoadNetworkIndex = index;
        }

        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

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