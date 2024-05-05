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
    }
}