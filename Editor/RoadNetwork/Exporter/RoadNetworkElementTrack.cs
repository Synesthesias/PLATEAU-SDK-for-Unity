using GeoJSON.Net.Geometry;
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
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークのトラックを表すクラス
    /// </summary>
    public class RoadNetworkElementTrack : RoadNetworkElement
    {
        public static readonly string IDPrefix = "Track";

        public RnDataTrack OriginTrack { get; private set; } = null;

        private const int TrackResolution = 10; // Trackの解像度

        public int Order { get; private set; } = 0;

        public float UpDistance = 0.0f;

        public float DownDistance = 0.0f;

        public RoadNetworkElementLink UpLink = null;

        public RoadNetworkElementLink DownLink = null;

        public int UpLane = 0;

        public int DownLane = 0;

        public RoadNetworkElementTrack(RoadNetworkContext context, string id, RnDataTrack track, List<RoadNetworkElementTrack> tracks, List<RoadNetworkElementLink> links) : base(context, IDPrefix + id)
        {
            ID = IDPrefix + id;

            OriginTrack = track;

            var allLanes = roadNetworkContext.RoadNetworkGetter.GetLanes();

            var allWays = roadNetworkContext.RoadNetworkGetter.GetWays();

            foreach (var link in links)
            {
                var lanes = link.GetOriginLanes();

                for (int i = 0; i < lanes.Count; i++)
                {
                    var lane = lanes[i];

                    if (allWays[OriginTrack.FromBorder.ID].LineString.ID == allWays[allLanes[lanes[i].ID].NextBorder.ID].LineString.ID ||
                        allWays[OriginTrack.FromBorder.ID].LineString.ID == allWays[allLanes[lanes[i].ID].PrevBorder.ID].LineString.ID)
                    {
                        UpLink = link;

                        UpLane = i;
                    }

                    if (allWays[OriginTrack.ToBorder.ID].LineString.ID == allWays[allLanes[lanes[i].ID].NextBorder.ID].LineString.ID ||
                        allWays[OriginTrack.ToBorder.ID].LineString.ID == allWays[allLanes[lanes[i].ID].PrevBorder.ID].LineString.ID)
                    {
                        DownLink = link;

                        DownLane = i;
                    }
                }
            }

            if (UpLink == null || DownLink == null)
            {
                Debug.LogError("Link not found " + ID);

                return;
            }

            var uplink = UpLink.ID.Replace(RoadNetworkElementLink.IDPrefix, "").Split('_');
            var downlink = DownLink.ID.Replace(RoadNetworkElementLink.IDPrefix, "").Split('_');

            Order = tracks.Where(t => t.UpLink == UpLink && t.DownLink == DownLink).ToList().Count;

            ID += "_" + uplink[0] + "_" + downlink[0] + "_" + Order;
        }

        public List<GeoJSON.Net.Geometry.Position> GetGeometory()
        {
            var coods = new List<GeoJSON.Net.Geometry.Position>();

            for (int i = 0; i < TrackResolution; i++)
            {
                var point = UnityEngine.Splines.SplineUtility.EvaluatePosition(OriginTrack.Spline, (float)i / TrackResolution);

                var cood = point;

                var geoCood = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(cood.x, cood.y, cood.z));

                coods.Add(new GeoJSON.Net.Geometry.Position(geoCood.Latitude, geoCood.Longitude));
            }

            return coods;
        }

        public float GetLength()
        {
            return UnityEngine.Splines.SplineUtility.CalculateLength(OriginTrack.Spline, Matrix4x4.identity);
        }
    }
}