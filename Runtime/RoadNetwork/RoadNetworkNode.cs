using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 交差点
    /// </summary>
    [Serializable]
    public class RoadNetworkNode
    {
        // 構成する頂点
        // ポリゴン的に時計回りの順に格納されている
        public List<Vector3> vertices = new List<Vector3>();

        //// レーンを構成する道
        //// 左側が若いインデックスになる
        //[SerializeField]
        public List<RoadNetworkWay> ways = new List<RoadNetworkWay>();

        // 他レーンとの境界線
        public List<RoadNetworkWay> borders = new List<RoadNetworkWay>();

        // 車線
        public List<RoadNetworkLane> lanes = new List<RoadNetworkLane>();

    }
}