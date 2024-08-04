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
        private List<RnRoad> roads = new List<RnRoad>();

        private List<RnIntersection> intersections = new List<RnIntersection>();

        private List<RnLineString> sideWalks = new List<RnLineString>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnRoad> Roads => roads;

        public IReadOnlyList<RnIntersection> Intersections => intersections;

        public IReadOnlyList<RnLineString> SideWalks => sideWalks;

        public void AddRoad(RnRoad link)
        {
            if (roads.Contains(link))
                return;

            link.ParentModel = this;
            roads.Add(link);
        }

        public void RemoveRoad(RnRoad link)
        {
            if (roads.Remove(link))
                link.ParentModel = null;
        }

        public void AddIntersection(RnIntersection intersection)
        {
            if (Intersections.Contains(intersection))
                return;

            intersection.ParentModel = this;
            intersections.Add(intersection);
        }

        public void RemoveIntersection(RnIntersection intersection)
        {
            if (intersections.Remove(intersection))
                intersection.ParentModel = null;
        }

        /// <summary>
        /// レーンの削除
        /// </summary>
        /// <param name="lane"></param>
        public void RemoveLane(RnLane lane)
        {
            foreach (var l in roads)
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
            foreach (var l in roads)
                l.ReplaceLane(before, after);
        }

        /// <summary>
        /// Intersection/Roadのレーンを全て取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> CollectAllLanes()
        {
            // Laneは重複しないはず
            return Roads.SelectMany(l => l.AllLanes).Concat(Intersections.SelectMany(n => n.Lanes));
        }

        /// <summary>
        /// Intersection/RoadのWayを全て取得
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
            foreach (var l in model.Roads)
                AddRoad(l);
            foreach (var n in model.Intersections)
                AddIntersection(n);
        }

        public RoadNetworkDataGetter CreateGetter(RoadNetworkStorage storage)
        {
            return new RoadNetworkDataGetter(storage);
        }

        public void SplitLaneByWidth(float roadWidth, out List<ulong> failedRoads)
        {
            failedRoads = new List<ulong>();
            var visitedRoads = new HashSet<RnRoad>();
            foreach (var link in Roads)
            {
                if (visitedRoads.Contains(link))
                    continue;

                try
                {
                    var linkGroup = link.CreateRoadGroup();
                    foreach (var l in linkGroup.Roads)
                        visitedRoads.Add(l);

                    linkGroup.Align();
                    if (linkGroup.IsValid == false)
                        continue;

                    if (linkGroup.Roads.Any(l => l.MainLanes.Count != 1))
                        continue;

                    if (linkGroup.Roads.Any(l => l.MainLanes[0].HasBothBorder == false))
                        continue;

                    var width = linkGroup.Roads.Select(l => l.MainLanes[0].CalcWidth()).Min();
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
                    failedRoads.Add(link.DebugMyId);
                }
            }
        }
    }
}