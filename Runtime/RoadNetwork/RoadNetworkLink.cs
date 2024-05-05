using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkLink
    {
        public RoadNetworkNode NextNode { get; private set; }

        public RoadNetworkNode PrevNode { get; private set; }

        // 本線レーン
        public List<RoadNetworkLane> MainLanes { get; } = new List<RoadNetworkLane>();

        // 右折レーン
        public List<RoadNetworkLane> RightLanes { get; } = new List<RoadNetworkLane>();

        // 左折レーン
        public List<RoadNetworkLane> LeftLanes { get; } = new List<RoadNetworkLane>();

        // 全レーン
        public IEnumerable<RoadNetworkLane> AllLanes => MainLanes.Concat(LeftLanes).Concat(RightLanes);

        /// <summary>
        /// 完全に孤立したリンクを作成
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        public static RoadNetworkLink CreateIsolatedLink(RoadNetworkLineString lineString)
        {
            var way = new RoadNetworkWay(lineString);
            var lane = RoadNetworkLane.CreateOneWayLane(way);
            var ret = new RoadNetworkLink();
            ret.MainLanes.Add(lane);
            return ret;
        }

        public static RoadNetworkLink CreateOneLaneLink(RoadNetworkLane lane)
        {
            var ret = new RoadNetworkLink();
            ret.MainLanes.Add(lane);
            return ret;
        }

    }
}