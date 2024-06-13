﻿using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        private List<RoadNetworkLink> links = new List<RoadNetworkLink>();
        private List<RoadNetworkNode> nodes = new List<RoadNetworkNode>();

        public IReadOnlyList<RoadNetworkLink> Links => links;

        public IReadOnlyList<RoadNetworkNode> Nodes => nodes;

        // #TODO : 一時的にモデル内部に用意する(ビルドするたびにリセットされないように)
        // シリアライズ用フィールド
        [field: SerializeField] private RoadNetworkStorage Storage { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
        public void AddLink(RoadNetworkLink link)
        {
            if (links.Contains(link))
                return;

            link.ParentModel = this;
            links.Add(link);
        }

        public void RemoveLink(RoadNetworkLink link)
        {
            if (links.Remove(link))
                link.ParentModel = null;
        }

        public void AddNode(RoadNetworkNode node)
        {
            if (Nodes.Contains(node))
                return;

            node.ParentModel = this;
            nodes.Add(node);
        }

        public void RemoveNode(RoadNetworkNode node)
        {
            if (nodes.Remove(node))
                node.ParentModel = null;
        }

        /// <summary>
        /// レーンの削除
        /// </summary>
        /// <param name="lane"></param>
        public void RemoveLane(RoadNetworkLane lane)
        {
            foreach (var l in links)
            {
                l.RemoveLane(lane);
            }
        }

        /// <summary>
        /// レーンの入れ替え
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void ReplaceLane(RoadNetworkLane before, RoadNetworkLane after)
        {
            foreach (var l in links)
                l.ReplaceLane(before, after);
        }

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

        //public void DebugIdentify()
        //{
        //    for (var i = 0; i < Nodes.Count; i++)
        //        Nodes[i].MyId = new RnID<RoadNetworkDataNode>(i);

        //    for (var i = 0; i < Links.Count; i++)
        //        Links[i].MyId = new RnID<RoadNetworkDataLink>(i);

        //    var allLanes = CollectAllLanes().ToList();
        //    for (var i = 0; i < allLanes.Count; i++)
        //        allLanes[i].MyId = new RnID<RoadNetworkDataLane>(i);

        //    var allWays = CollectAllWays().ToList();
        //    for (var i = 0; i < allWays.Count; i++)
        //        allWays[i].MyId = new RnID<RoadNetworkDataWay>(i);

        //    var allLineStrings = CollectAllLineStrings().ToList();
        //    for (var i = 0; i < allLineStrings.Count; i++)
        //        allLineStrings[i].MyId = new RnID<RoadNetworkDataLineString>(i);
        //}
        public void Serialize()
        {
            var serializer = new RoadNetworkSerializer();
            Storage = serializer.Serialize(this);
        }

        public void Deserialize()
        {
            var serializer = new RoadNetworkSerializer();
            var model = serializer.Deserialize(Storage);
            foreach (var l in model.Links)
                AddLink(l);
            foreach (var n in model.Nodes)
                AddNode(n);
        }
    }
}