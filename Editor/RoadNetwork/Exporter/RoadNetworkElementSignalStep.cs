using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    public class RoadNetworkElementSignalStep : RoadNetworkElement
    {
        public static readonly string IDPrefix = "SignalStep";

        public RoadNetworkElementSignalController Controller;

        public List<RoadNetworkElementSignalLight> SignalLights;

        public string PatternID;

        public int Order;

        public int Duration;

        public List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)> LinkPairsGreen = new List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)>();

        public List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)> LinkPairsYellow = new List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)>();

        public List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)> LinkPairsRed = new List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)>();

        public RoadNetworkElementSignalStep(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
        }

        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        public string GetColor(List<(RoadNetworkElementLink In, RoadNetworkElementLink Out)> pair)
        {
            var ret = "";

            foreach (var p in pair)
            {
                if (ret != "")
                {
                    ret += ":";
                }

                if (p.In == null || p.Out == null)
                {
                    continue;
                }

                ret += p.In.ID + "->" + p.Out.ID;
            }

            return ret;
        }

        public string GetSignalLights()
        {
            var ret = SignalLights[0].ID;

            for (int i = 1; i < SignalLights.Count; i++)
            {
                ret += ":" + SignalLights[i].ID;
            }

            return ret;
        }

        public int GetTypeMask()
        {
            return -1;
        }

        public GeoJSON.Net.Geometry.Position GetGeometory()
        {
            return Controller.GetGeometory();
        }
    }
}