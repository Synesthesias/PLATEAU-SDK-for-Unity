using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadNetwork.Structure
{
    // #NOTE : Editorが重いのでSerialize対象にしない
    public class RnModel
    {
        public const float Epsilon = float.Epsilon;

        //----------------------------------
        // start: フィールド
        //----------------------------------


        // #NOTE : Editorが重いのでSerialize対象にしない
        private List<RnRoad> links = new List<RnRoad>();

        private List<RnIntersection> nodes = new List<RnIntersection>();

        private List<RnLineString> sideWalks = new List<RnLineString>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnRoad> Links => links;

        public IReadOnlyList<RnIntersection> Nodes => nodes;

        public IReadOnlyList<RnLineString> SideWalks => sideWalks;

        public void AddLink(RnRoad link)
        {
            if (links.Contains(link))
                return;

            link.ParentModel = this;
            links.Add(link);
        }

        public void RemoveLink(RnRoad link)
        {
            if (links.Remove(link))
                link.ParentModel = null;
        }

        public void AddNode(RnIntersection node)
        {
            if (Nodes.Contains(node))
                return;

            node.ParentModel = this;
            nodes.Add(node);
        }

        public void RemoveNode(RnIntersection node)
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

        public RoadNetworkStorage Serialize()
        {
            var serializer = new RoadNetworkSerializer();
            return serializer.Serialize(this);
        }

        public void Deserialize(RoadNetworkStorage storage)
        {
            var serializer = new RoadNetworkSerializer();
            var model = serializer.Deserialize(storage);
            foreach (var l in model.Links)
                AddLink(l);
            foreach (var n in model.Nodes)
                AddNode(n);
        }

        public RoadNetworkDataGetter CreateGetter(RoadNetworkStorage storage)
        {
            return new RoadNetworkDataGetter(storage);
        }

        public void SplitLaneByWidth(float roadWidth, out List<ulong> failedLinks)
        {
            failedLinks = new List<ulong>();
            var visitedLinks = new HashSet<RnRoad>();
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
                    //Debug.LogException(e);
                    failedLinks.Add(link.DebugMyId);
                }
            }
        }
    }
}