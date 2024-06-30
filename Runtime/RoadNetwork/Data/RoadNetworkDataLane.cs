using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadNetwork.Data
{

    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkLane))]
    public class RoadNetworkDataLane : IPrimitiveData
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RoadNetworkDataLane> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Parent { get; set; }

        // 連結しているレーン(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.NextLanes))]
        public List<RnID<RoadNetworkDataLane>> NextLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 連結しているレーン(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.PrevLanes))]
        public List<RnID<RoadNetworkDataLane>> PrevLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 境界線(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.PrevBorder))]
        public RnID<RoadNetworkDataWay> PrevBorder { get; set; }

        // 境界線(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.NextBorder))]
        public RnID<RoadNetworkDataWay> NextBorder { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.LeftWay))]
        public RnID<RoadNetworkDataWay> LeftWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.RightWay))]
        public RnID<RoadNetworkDataWay> RightWay { get; set; }

    }

}