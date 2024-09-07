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

        private List<RnSideWalk> sideWalks = new List<RnSideWalk>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnRoad> Roads => roads;

        public IReadOnlyList<RnIntersection> Intersections => intersections;

        public IReadOnlyList<RnSideWalk> SideWalks => sideWalks;

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
            return Roads.SelectMany(l => l.AllLanes);
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

        /// <summary>
        /// roadをIntersectionに変換
        /// </summary>
        /// <param name="road"></param>
        public void Convert2Intersection(RnRoad road)
        {
            var intersection = new RnIntersection(road.TargetTran);

            road.TryGetMergedSideWay(null, out var leftWay, out var rightWay);
            if (leftWay != null)
                intersection.AddEdge(null, leftWay);
            foreach (var lane in road.MainLanes)
            {
                lane.AlignBorder();
                intersection.AddEdge(lane.GetNextRoad(), lane.NextBorder);
            }

            if (rightWay != null)
                intersection.AddEdge(null, rightWay.ReversedWay());
            foreach (var lane in road.MainLanes)
            {
                intersection.AddEdge(lane.GetPrevRoad(), lane.PrevBorder.ReversedWay());
            }

            AddIntersection(intersection);
            // 旧Roadの削除
            road.DisConnect(true);
        }

        public void Convert2Road(RnIntersection intersection, RnRoadBase prev, RnRoadBase next)
        {
            var neighbors = intersection
                .Neighbors
                .Where(n => n.Road == prev || n.Road == next)
                .ToList();

            var road = new RnRoad(intersection.TargetTran);
            road.SetPrevNext(prev, next);

            // #TODO : 

            AddRoad(road);
            intersection.DisConnect(true);
        }

        /// <summary>
        /// 歩道情報追加
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sideWalk"></param>
        public void AddSideWalk(RnSideWalk sideWalk)
        {
            if (sideWalks.Contains(sideWalk))
                return;
            sideWalks.Add(sideWalk);
        }

        public void AddRoadBase(RnRoadBase roadBase)
        {
            if (roadBase is RnRoad road)
                AddRoad(road);
            else if (roadBase is RnIntersection intersection)
                AddIntersection(intersection);
        }

        /// <summary>
        /// prev/next間に道路を作成
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <param name="lane"></param>
        public void CreateRoadBetweenIntersection(RnIntersection prev, RnIntersection next, RnLane lane)
        {
            var road = RnRoad.CreateOneLaneRoad(null, lane);
            road.SetPrevNext(prev, next);

            // intersectionに隣接情報追加
            prev.AddEdge(road, lane.PrevBorder);
            next.AddEdge(road, lane.NextBorder);
            AddRoad(road);
        }

        public RoadNetworkStorage Serialize(bool createEmptyCheck = true)
        {
            // シリアライズ前に一度全レーンに対して中央線を作成する
            foreach (var road in Roads)
            {
                foreach (var l in road.AllLanes)
                    l.CreateCenterWay();
            }

            if (createEmptyCheck)
            {
                CreateEmptyRoadBetweenInteraction();
                CreateEmptyIntersectionBetweenRoad();
            }

            var serializer = new RoadNetworkSerializer();
            var ret = serializer.Serialize(this);

            // 自分は元に戻す
            if (createEmptyCheck)
            {
                RemoveEmptyRoadBetweenIntersection();
                RemoveEmptyIntersectionBetweenRoad();
            }

            return ret;
        }

        public void Deserialize(RoadNetworkStorage storage, bool removeEmptyCheck = true)
        {
            var serializer = new RoadNetworkSerializer();
            var model = serializer.Deserialize(storage);
            CopyFrom(model);
            if (removeEmptyCheck)
            {
                RemoveEmptyRoadBetweenIntersection();
                RemoveEmptyIntersectionBetweenRoad();
            }
        }

        /// <summary>
        /// 道路同士が直接つながっている状況において、空の交差点を作成する
        /// </summary>
        public void CreateEmptyIntersectionBetweenRoad()
        {
            HashSet<RnRoad> visited = new();
            var visitedRoads = new HashSet<RnRoad>();
            foreach (var link in Roads)
            {
                if (visitedRoads.Contains(link))
                    continue;

                try
                {
                    var roadGroup = link.CreateRoadGroup();
                    foreach (var r in roadGroup.Roads)
                        visitedRoads.Add(r);

                    if (roadGroup.Roads.Count <= 1)
                        continue;

                    // 整列させる
                    roadGroup.Align();
                    for (var i = 0; i < roadGroup.Roads.Count - 1; i++)
                    {
                        var prev = roadGroup.Roads[i];
                        var next = roadGroup.Roads[i + 1];
                        var borders = prev.GetBorderWays(RnLaneBorderType.Next).ToList();
                        var intersection = RnIntersection.CreateEmptyIntersection(borders, prev, next);

                        prev?.SetPrevNext(prev.Prev, intersection);
                        next?.SetPrevNext(intersection, next.Next);
                        AddIntersection(intersection);
                    }

                }
                catch (Exception e)
                {
                }
            }
        }

        public void RemoveEmptyIntersectionBetweenRoad()
        {
            HashSet<RnIntersection> remove = new();
            foreach (var intersection in Intersections)
            {
                if (intersection.IsEmptyIntersection == false)
                    continue;
                var roads = intersection.Edges
                    .Select(e => e.Road as RnRoad)
                    .Where(n => n != null)
                    .ToHashSet();

                foreach (var r in roads)
                {
                    var other = roads.FirstOrDefault(n => n != r);
                    if (r.Prev == intersection)
                        r.SetPrevNext(other, r.Next);
                    if (r.Next == intersection)
                        r.SetPrevNext(r.Prev, other);
                }

                remove.Add(intersection);
            }

            foreach (var r in remove)
                r.DisConnect(true);
        }

        /// <summary>
        /// 交差点同士が直接つながっている状況において、交差点間のリンクを作成する
        /// </summary>
        public void CreateEmptyRoadBetweenInteraction()
        {
            HashSet<RnIntersection> visited = new();
            foreach (var inter in Intersections)
            {
                if (visited.Contains(inter))
                    continue;
                visited.Add(inter);

                foreach (var neighbor in inter.Neighbors)
                {
                    if (neighbor.Border == null)
                        continue;

                    if ((neighbor.Road is RnIntersection other) == false)
                        continue;

                    if (visited.Contains(other))
                        continue;

                    var otherNeighbor =
                        other.Neighbors.FirstOrDefault(n => n.Border.IsSameLine(neighbor.Border) && n.Road == inter);
                    if (otherNeighbor == null)
                        continue;

                    neighbor.Border.GetLerpPoint(0.5f, out var pos);
                    var p = new RnPoint(pos);
                    var emptyWay = new RnWay(RnLineString.Create(Enumerable.Repeat(p, 2), false));
                    var emptyLane = RnLane.CreateEmptyLane(neighbor.Border, emptyWay);
                    var emptyRoad = RnRoad.CreateOneLaneRoad(null, emptyLane);
                    emptyRoad.SetPrevNext(inter, other);
                    neighbor.Road = emptyRoad;
                    otherNeighbor.Road = emptyRoad;
                    AddRoad(emptyRoad);
                }
            }
        }

        /// <summary>
        /// 直接つながった交差点間に挿入された空リンクを削除する
        /// </summary>
        public void RemoveEmptyRoadBetweenIntersection()
        {
            HashSet<RnRoad> remove = new();
            foreach (var road in Roads)
            {
                if (road.IsEmptyRoad == false)
                    continue;

                var inter1 = road.Next as RnIntersection;
                var inter2 = road.Prev as RnIntersection;
                foreach (var n in inter1.Neighbors.Where(n => n.Road == road))
                    n.Road = inter2;
                foreach (var n in inter2.Neighbors.Where(n => n.Road == road))
                    n.Road = inter1;
                road.SetPrevNext(null, null);
                remove.Add(road);
            }

            foreach (var r in remove)
                RemoveRoad(r);
        }


        public void ReBuildIntersectionTracks()
        {
            foreach (var intersection in Intersections)
            {
                intersection.BuildTracks();
            }
        }

        public void Clear()
        {
            sideWalks.Clear();
            roads.Clear();
            intersections.Clear();
        }

        /// <summary>
        /// modelからコピーしてくる
        /// </summary>
        /// <param name="model"></param>
        public void CopyFrom(RnModel model)
        {
            // 自身は削除
            Clear();

            // 道路/交差点/歩道をコピー
            foreach (var l in model.Roads)
                AddRoad(l);

            foreach (var n in model.Intersections)
                AddIntersection(n);

            foreach (var n in model.SideWalks)
                AddSideWalk(n);
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

                    if (linkGroup.Roads.Any(l => l.MainLanes[0].HasBothBorder == false))
                        continue;

                    // #TODO : 中央線がある場合の処理

                    if (linkGroup.Roads.Any(l => l.MainLanes.Count != 1))
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