using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
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

        //----------------------------------
        // start: フィールド
        //----------------------------------

        public List<RoadNetworkLink> Links { get; } = new List<RoadNetworkLink>();

        public List<RoadNetworkNode> Nodes { get; } = new List<RoadNetworkNode>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // #TODO : 実際はもっとある
        public IEnumerable<RoadNetworkLane> CollectAllLanes()
        {
            return Links.SelectMany(l => l.AllLanes).Distinct();
        }

        // #TODO : 実際はもっとある
        public IEnumerable<RoadNetworkWay> CollectAllWays()
        {
            return CollectAllLanes().SelectMany(l => l.AllBorders.Concat(l.BothWays)).Distinct();
        }

        // #TODO : 実際はもっとある
        public IEnumerable<RoadNetworkLineString> CollectAllLineStrings()
        {
            return CollectAllWays().Select(w => w.LineString).Distinct();
        }

        public void DebugIdentify()
        {
            for (var i = 0; i < Nodes.Count; i++)
                Nodes[i].MyId = new RnID<RoadNetworkDataNode>(i);

            for (var i = 0; i < Links.Count; i++)
                Links[i].MyId = new RnID<RoadNetworkDataLink>(i);

            var allLanes = CollectAllLanes().ToList();
            for (var i = 0; i < allLanes.Count; i++)
                allLanes[i].MyId = new RnID<RoadNetworkDataLane>(i);

            var allWays = CollectAllWays().ToList();
            for (var i = 0; i < allWays.Count; i++)
                allWays[i].MyId = new RnID<RoadNetworkDataWay>(i);

            var allLineStrings = CollectAllLineStrings().ToList();
            for (var i = 0; i < allLineStrings.Count; i++)
                allLineStrings[i].MyId = new RnID<RoadNetworkDataLineString>(i);
        }
    }
}