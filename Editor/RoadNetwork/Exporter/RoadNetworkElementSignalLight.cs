using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    public class RoadNetworkElementSignalLight : RoadNetworkElement
    {
        public static readonly string IDPrefix = "SignalLight";

        public RoadNetworkElementSignalController Controller;

        public RoadNetworkElementLink Link;

        public RoadNetworkElementSignalLight(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
        }

        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        public string GetLaneType()
        {
            return "Lane";
        }

        public string GetLanePos()
        {
            return "-1";
        }

        public string GetDistance()
        {
            return "0";
        }

        public GeoJSON.Net.Geometry.Position GetGeometory()
        {
            return Controller.GetGeometory();
        }
    }
}