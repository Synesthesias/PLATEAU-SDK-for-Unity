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

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        public RoadNetworkNode NextNode { get; private set; }

        public RoadNetworkNode PrevNode { get; private set; }

        // 本線レーン
        public List<RoadNetworkLane> MainLanes { get; } = new List<RoadNetworkLane>();

        // 右折レーン
        public List<RoadNetworkLane> RightLanes { get; } = new List<RoadNetworkLane>();

        // 左折レーン
        public List<RoadNetworkLane> LeftLanes { get; } = new List<RoadNetworkLane>();

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
            ret.MainLanes.Add(lane);
            return ret;
        }

        public static RoadNetworkLink CreateOneLaneLink(PLATEAUCityObjectGroup targetTran, RoadNetworkLane lane)
        {
            var ret = new RoadNetworkLink(targetTran);
            ret.MainLanes.Add(lane);
            return ret;
        }

    }
}