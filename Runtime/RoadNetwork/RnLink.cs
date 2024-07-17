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

        // 双方向フラグ
        public bool IsBothWay { get; set; } = true;
        //----------------------------------
        // end: フィールド
        //----------------------------------

        // 本線レーン(参照のみ)
        // 追加/削除はAddMainLane/RemoveMainLaneを使うこと
        public IReadOnlyList<RnLane> MainLanes => mainLanes;

        // 全レーン
        public override IEnumerable<RnLane> AllLanes => MainLanes;

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

        public void RemoveMainLane(RnLane lane)
        {
            RemoveLane(mainLanes, lane);
        }


        public void RemoveLane(RnLane lane)
        {
            RnEx.RemoveLane(mainLanes, lane);
        }

        public void ReplaceLane(RnLane before, RnLane after)
        {
            RnEx.ReplaceLane(mainLanes, before, after);
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