using PLATEAU.CityInfo;
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
    public class RnModel
    {
        public const float Epsilon = float.Epsilon;

        //----------------------------------
        // start: フィールド
        //----------------------------------
        [SerializeField]
        private List<RnLink> links = new List<RnLink>();

        [SerializeField]
        private List<RnNode> nodes = new List<RnNode>();

        [SerializeField]
        private List<RnLineString> sideWalks = new List<RnLineString>();

        // #TODO : 一時的にモデル内部に用意する(ビルドするたびにリセットされないように)
        // シリアライズ用フィールド
        [field: SerializeField] private RoadNetworkStorage Storage { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnLink> Links => links;

        public IReadOnlyList<RnNode> Nodes => nodes;

        public IReadOnlyList<RnLineString> SideWalks => sideWalks;

        public void AddLink(RnLink link)
        {
            if (links.Contains(link))
                return;

            link.ParentModel = this;
            links.Add(link);
        }

        public void RemoveLink(RnLink link)
        {
            if (links.Remove(link))
                link.ParentModel = null;
        }

        public void AddNode(RnNode node)
        {
            if (Nodes.Contains(node))
                return;

            node.ParentModel = this;
            nodes.Add(node);
        }

        public void RemoveNode(RnNode node)
        {
            if (nodes.Remove(node))
                node.ParentModel = null;
        }

        /// <summary>
        /// レーンの削除
        /// </summary>
        /// <param name="lane"></param>
        public void RemoveLane(RnLane lane)
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
        public void ReplaceLane(RnLane before, RnLane after)
        {
            foreach (var l in links)
                l.ReplaceLane(before, after);
        }

        /// <summary>
        /// Node/Linkのレーンを全て取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> CollectAllLanes()
        {
            // Laneは重複しないはず
            return Links.SelectMany(l => l.AllLanes).Concat(Nodes.SelectMany(n => n.Lanes));
        }

        /// <summary>
        /// Node/LinkのWayを全て取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnWay> CollectAllWays()
        {
            return CollectAllLanes().SelectMany(l => l.AllBorders.Concat(l.BothWays)).Distinct();
        }

        // #TODO : 実際はもっとある
        public IEnumerable<RnLineString> CollectAllLineStrings()
        {
            return CollectAllWays().Select(w => w.LineString).Distinct();
        }

        public void AddWalkRoad(RnLineString walkRoad)
        {
            if (sideWalks.Contains(walkRoad))
                return;

            sideWalks.Add(walkRoad);
        }

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

        public RoadNetworkDataGetter CreateGetter()
        {
            return new RoadNetworkDataGetter(Storage);
        }

        public void SplitLaneByWidth(float roadWidth, out List<ulong> failedLinks)
        {
            failedLinks = new List<ulong>();
            var visitedLinks = new HashSet<RnLink>();
            foreach (var link in Links)
            {
                if (visitedLinks.Contains(link))
                    continue;

                try
                {
                    var linkGroup = link.CreateLinkGroup();
                    foreach (var l in linkGroup.Links)
                        visitedLinks.Add(l);

                    linkGroup.Align();
                    if (linkGroup.IsValid == false)
                        continue;

                    if (linkGroup.Links.Any(l => l.MainLanes.Count != 1))
                        continue;

                    if (linkGroup.Links.Any(l => l.MainLanes[0].HasBothBorder == false))
                        continue;

                    var width = linkGroup.Links.Select(l => l.MainLanes[0].CalcWidth()).Min();
                    var num = (int)(width / roadWidth);
                    if (num <= 1)
                        continue;

                    var leftLaneCount = (num + 1) / 2;
                    var rightLaneCount = num - leftLaneCount;
                    linkGroup.SetLaneCount(leftLaneCount, rightLaneCount);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    failedLinks.Add(link.DebugMyId);
                }
            }
        }
    }
}