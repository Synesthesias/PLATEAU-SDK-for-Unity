using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交差点
    /// </summary>
    [Serializable]
    public class RoadNetworkNode
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RoadNetworkDataNode> MyId { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 隣接情報
        public List<RoadNetworkNeighbor> Neighbors { get; set; } = new List<RoadNetworkNeighbor>();

        // 車線
        public List<RoadNetworkTrack> Tracks { get; set; } = new List<RoadNetworkTrack>();

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public RoadNetworkNode() { }

        public RoadNetworkNode(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        public Vector3 GetCenterPoint()
        {
            var ret = Neighbors.SelectMany(n => n.Border.Vertices).Aggregate(Vector3.zero, (a, b) => a + b);
            var cnt = Neighbors.Sum(n => n.Border.Count);
            return ret / cnt;
        }
    }
}