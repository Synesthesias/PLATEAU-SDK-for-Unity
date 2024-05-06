using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
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
        public RnId<RoadNetworkDataNode> MyId { get; set; }

        //// レーンを構成する道
        //// 左側が若いインデックスになる
        //[SerializeField]
        public List<RoadNetworkWay> Ways { get; set; } = new List<RoadNetworkWay>();

        // 他レーンとの境界線
        public List<RoadNetworkWay> Borders { get; set; } = new List<RoadNetworkWay>();

        // 車線
        public List<RoadNetworkLane> Lanes { get; set; } = new List<RoadNetworkLane>();

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}