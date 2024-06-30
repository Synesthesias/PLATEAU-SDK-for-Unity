using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnLink))]
    public class RnDataLink : RnDataRoadBase
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RnDataLink> MyId { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Next { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Prev { get; set; }

        // 本線レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("mainLanes")]
        public List<RnID<RnDataLane>> MainLanes { get; set; } = new List<RnID<RnDataLane>>();

        // 右折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("rightLanes")]
        public List<RnID<RnDataLane>> RightLanes { get; set; } = new List<RnID<RnDataLane>>();

        // 左折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("leftLanes")]
        public List<RnID<RnDataLane>> LeftLanes { get; set; } = new List<RnID<RnDataLane>>();

        // 双方向フラグ
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public bool IsBothWay { get; set; } = true;
    }
}