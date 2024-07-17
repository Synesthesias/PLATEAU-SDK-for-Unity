using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    //[Serializable]
    public class RnLink : RnRoadBase
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        public RnRoadBase Next { get; set; }

        public RnRoadBase Prev { get; set; }

        private List<RnLane> mainLanes = new List<RnLane>();
        private List<RnLane> leftLanes = new List<RnLane>();
        private List<RnLane> rightLanes = new List<RnLane>();

        // 双方向フラグ
        public bool IsBothWay { get; set; } = true;
        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 本線レーン(参照のみ)
        // 追加/削除はAddMainLane/RemoveMainLaneを使うこと
        public IReadOnlyList<RnLane> MainLanes => mainLanes;

        // 右折専用レーン
        // 追加/削除はAddLeftLane/RemoveLeftLaneを使うこと
        public IReadOnlyList<RnLane> RightLanes => rightLanes;

        // 左折専用レーン
        // 追加/削除はAddRightLane/RemoveRightLaneを使うこと
        public IReadOnlyList<RnLane> LeftLanes => leftLanes;

        // 全レーン
        public override IEnumerable<RnLane> AllLanes => MainLanes.Concat(LeftLanes).Concat(RightLanes);

        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            if (Next != null)
                yield return Next;
            if (Prev != null)
                yield return Prev;
        }

        public RnLink() { }

        public RnLink(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public void AddMainLane(RnLane lane)
        {
            AddLane(mainLanes, lane);
        }

        public void AddLeftLane(RnLane lane)
        {
            AddLane(leftLanes, lane);
        }

        public void AddRightLane(RnLane lane)
        {
            AddLane(rightLanes, lane);
        }

        public void RemoveMainLane(RnLane lane)
        {
            RemoveLane(mainLanes, lane);
        }

        public void RemoveLeftLane(RnLane lane)
        {
            RemoveLane(leftLanes, lane);
        }

        public void RemoveRightLane(RnLane lane)
        {
            RemoveLane(rightLanes, lane);
        }

        public void RemoveLane(RnLane lane)
        {
            RnEx.RemoveLane(mainLanes, lane);
            RnEx.RemoveLane(leftLanes, lane);
            RnEx.RemoveLane(rightLanes, lane);
        }

        public void ReplaceLane(RnLane before, RnLane after)
        {
            RnEx.ReplaceLane(mainLanes, before, after);
            RnEx.ReplaceLane(leftLanes, before, after);
            RnEx.ReplaceLane(rightLanes, before, after);
        }

        /// <summary>
        /// lanesにlaneを追加する. ParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void AddLane(List<RnLane> lanes, RnLane lane)
        {
            if (lanes.Contains(lane))
                return;
            lane.Parent = this;
            lanes.Add(lane);
        }

        /// <summary>
        /// lanesからlaneを削除するParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void RemoveLane(List<RnLane> lanes, RnLane lane)
        {
            if (lanes.Remove(lane))
                lane.Parent = null;
        }


        public void SplitLane(RnLane lane, int splitNum)
        {
            if (AllLanes.Any(l => l == lane) == false)
                return;

            if (splitNum <= 1)
                return;

            if ((lane?.HasBothBorder ?? false) == false)
                return;

            var index = mainLanes.IndexOf(lane);
            if (index < 0)
                return;


            // 左->右になるように並び替える
            List<RnWay> SplitBorder(RnLaneBorderType borderType)
            {
                var border = lane.GetBorder(borderType);
                var ways = border.Split(splitNum, true);
                if (lane.GetBorderDir(borderType) == RnLaneBorderDir.Right2Left)
                {
                    ways.Reverse();
                    for (var i = 0; i < ways.Count; i++)
                        ways[i] = ways[i].ReversedWay();
                }
                return ways;
            }
            // Borderの中心を開始点とする
            var prevSubBorders = SplitBorder(RnLaneBorderType.Prev);
            var nextSubBorders = SplitBorder(RnLaneBorderType.Next);
            if (prevSubBorders.Count < splitNum || nextSubBorders.Count < splitNum)
            {
                Debug.LogWarning("SplitLane: splitNum is too large");
                //return;
            }
            RemoveLane(lane);

            var lefts = lane.LeftWay.Vertices.ToList();
            var rights = lane.RightWay.Vertices.ToList();
            var leftWay = lane.LeftWay;
            foreach (var i in Enumerable.Range(0, splitNum))
            {
                var p2 = (i + 1f) / splitNum;
                RnWay r = new RnWay(lane.RightWay.LineString, lane.RightWay.IsReversed, true);
                if (i != splitNum - 1)
                {
                    var points = new List<RnPoint>();
                    void AddPoint(RnPoint p)
                    {
                        if (points.Any() && (points.Last().Vertex - p.Vertex).sqrMagnitude < 0.001f)
                            return;
                        points.Add(p);
                    }


                    AddPoint(prevSubBorders[i].Points.Last());

                    var segments = GeoGraphEx.GetInnerLerpSegments(lefts, rights, AxisPlane.Xz, p2);
                    foreach (var s in segments)
                    {
                        AddPoint(new RnPoint(s.Segment.Start));
                    }

                    AddPoint(nextSubBorders[i].Points.Last());
                    var rightLine = RnLineString.Create(points);
                    r = new RnWay(rightLine, false, true);
                }
                var l = new RnWay(leftWay.LineString, leftWay.IsReversed, false);
                var newLane = new RnLane(l, r, prevSubBorders[i], nextSubBorders[i]);
                AddMainLane(newLane);
                leftWay = r;
            }
        }

        // ---------------
        // Static Methods
        // ---------------
        /// <summary>
        /// 完全に孤立したリンクを作成
        /// </summary>
        /// <param name="targetTran"></param>
        /// <param name="way"></param>
        /// <returns></returns>
        public static RnLink CreateIsolatedLink(PLATEAUCityObjectGroup targetTran, RnWay way)
        {
            var lane = RnLane.CreateOneWayLane(way);
            var ret = new RnLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

        public static RnLink CreateOneLaneLink(PLATEAUCityObjectGroup targetTran, RnLane lane)
        {
            var ret = new RnLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

    }
}