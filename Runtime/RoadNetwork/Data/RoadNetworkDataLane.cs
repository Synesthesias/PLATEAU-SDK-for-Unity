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
        // 自分自身を表すId
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.MyId))]
        public RnId<RoadNetworkDataLane> MyId { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.ParentLink))]
        public RnId<RoadNetworkDataLink> ParentLink { get; set; }

        // 連結しているレーン(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.NextLanes))]
        public List<RnId<RoadNetworkDataLane>> NextLanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();

        // 連結しているレーン(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.PrevLanes))]
        public List<RnId<RoadNetworkDataLane>> PrevLanes { get; set; } = new List<RnId<RoadNetworkDataLane>>();

        // 境界線(下流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.PrevBorder))]
        public RnId<RoadNetworkDataWay> PrevBorder { get; set; }

        // 境界線(上流)
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.NextBorder))]
        public RnId<RoadNetworkDataWay> NextBorder { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.LeftWay))]
        public RnId<RoadNetworkDataWay> LeftWay { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLane.RightWay))]
        public RnId<RoadNetworkDataWay> RightWay { get; set; }

    }

}