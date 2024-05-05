using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkModel
    {
        public const float Epsilon = float.Epsilon;

        public List<RoadNetworkLink> Links { get; } = new List<RoadNetworkLink>();

        public List<RoadNetworkNode> Nodes { get; } = new List<RoadNetworkNode>();

        public void DebugIdentify()
        {
            for (var i = 0; i < Nodes.Count; i++)
                Nodes[i].DebugId = i;

            for (var i = 0; i < Links.Count; i++)
                Links[i].DebugId = i;

            var allLanes = Links.SelectMany(l => l.AllLanes).Distinct().ToList();
            for (var i = 0; i < allLanes.Count; i++)
                allLanes[i].DebugId = i;

            var allWays = allLanes.SelectMany(l => l.AllBorders.Concat(l.BothWays)).Distinct().ToList();
            for (var i = 0; i < allWays.Count; i++)
                allWays[i].DebugId = i;

            var allLineStrings = allWays.Select(w => w.LineString).Distinct().ToList();
            for (var i = 0; i < allLineStrings.Count; i++)
                allLineStrings[i].DebugId = i;
        }
    }
}