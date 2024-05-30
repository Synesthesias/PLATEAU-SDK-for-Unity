using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLink
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RoadNetworkDataLink> MyId { get; set; }

        // 自分が所属するRoadNetworkModel
        public RoadNetworkModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        public RoadNetworkNode NextNode { get; set; }

        public RoadNetworkNode PrevNode { get; set; }

        private List<RoadNetworkLane> mainLanes = new List<RoadNetworkLane>();
        private List<RoadNetworkLane> leftLanes = new List<RoadNetworkLane>();
        private List<RoadNetworkLane> rightLanes = new List<RoadNetworkLane>();

        // 本線レーン(参照のみ)
        // 追加/削除はAddMainLane/RemoveMainLaneを使うこと
        public IReadOnlyList<RoadNetworkLane> MainLanes => mainLanes;

        // 右折レーン
        // 追加/削除はAddLeftLane/RemoveLeftLaneを使うこと
        public IReadOnlyList<RoadNetworkLane> RightLanes => rightLanes;

        // 左折レーン
        // 追加/削除はAddRightLane/RemoveRightLaneを使うこと
        public IReadOnlyList<RoadNetworkLane> LeftLanes => leftLanes;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 全レーン
        public IEnumerable<RoadNetworkLane> AllLanes => MainLanes.Concat(LeftLanes).Concat(RightLanes);

        public RoadNetworkLink() { }

        public RoadNetworkLink(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public void AddMainLane(RoadNetworkLane lane)
        {
            AddLane(mainLanes, lane);
        }

        public void AddLeftLane(RoadNetworkLane lane)
        {
            AddLane(leftLanes, lane);
        }

        public void AddRightLane(RoadNetworkLane lane)
        {
            AddLane(rightLanes, lane);
        }

        public void RemoveMainLane(RoadNetworkLane lane)
        {
            RemoveLane(mainLanes, lane);
        }

        public void RemoveLeftLane(RoadNetworkLane lane)
        {
            RemoveLane(leftLanes, lane);
        }

        public void RemoveRightLane(RoadNetworkLane lane)
        {
            RemoveLane(rightLanes, lane);
        }

        /// <summary>
        /// lanesにlaneを追加する. ParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void AddLane(List<RoadNetworkLane> lanes, RoadNetworkLane lane)
        {
            if (lanes.Contains(lane))
                return;
            lane.ParentLink = this;
            lanes.Add(lane);
        }

        /// <summary>
        /// lanesからlaneを削除するParentLink情報も更新する
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        private void RemoveLane(List<RoadNetworkLane> lanes, RoadNetworkLane lane)
        {
            if (lanes.Remove(lane))
                lane.ParentLink = null;
        }

        /// <summary>
        /// 完全に孤立したリンクを作成
        /// </summary>
        /// <param name="targetTran"></param>
        /// <param name="lineString"></param>
        /// <returns></returns>
        public static RoadNetworkLink CreateIsolatedLink(PLATEAUCityObjectGroup targetTran, RoadNetworkLineString lineString)
        {
            var way = new RoadNetworkWay(lineString);
            var lane = RoadNetworkLane.CreateOneWayLane(way);
            var ret = new RoadNetworkLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

        public static RoadNetworkLink CreateOneLaneLink(PLATEAUCityObjectGroup targetTran, RoadNetworkLane lane)
        {
            var ret = new RoadNetworkLink(targetTran);
            ret.AddMainLane(lane);
            return ret;
        }

    }
}