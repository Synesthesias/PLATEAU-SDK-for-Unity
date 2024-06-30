using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork.Data
{

    [Serializable, RoadNetworkSerializeData(typeof(RnLane))]
    public class RnDataLane : IPrimitiveData
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RnDataLane> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Parent { get; set; }

        // 連結しているレーン(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.NextLanes))]
        public List<RnID<RnDataLane>> NextLanes { get; set; } = new List<RnID<RnDataLane>>();

        // 連結しているレーン(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.PrevLanes))]
        public List<RnID<RnDataLane>> PrevLanes { get; set; } = new List<RnID<RnDataLane>>();

        // 境界線(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.PrevBorder))]
        public RnID<RnDataWay> PrevBorder { get; set; }

        // 境界線(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.NextBorder))]
        public RnID<RnDataWay> NextBorder { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.LeftWay))]
        public RnID<RnDataWay> LeftWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnLane.RightWay))]
        public RnID<RnDataWay> RightWay { get; set; }

    }

}