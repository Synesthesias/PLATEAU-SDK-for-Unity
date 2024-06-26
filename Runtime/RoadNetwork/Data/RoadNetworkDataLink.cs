using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkLink))]
    public class RoadNetworkDataLink : IPrimitiveData
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RoadNetworkDataLink> MyId { get; set; }

        // 対象のtranオブジェクト
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RoadNetworkDataNode> NextNode { get; set; }

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RoadNetworkDataNode> PrevNode { get; set; }

        // 本線レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("mainLanes")]
        public List<RnID<RoadNetworkDataLane>> MainLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 右折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("rightLanes")]
        public List<RnID<RoadNetworkDataLane>> RightLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 左折レーン
        [field: SerializeField]
        [RoadNetworkSerializeMember("leftLanes")]
        public List<RnID<RoadNetworkDataLane>> LeftLanes { get; set; } = new List<RnID<RoadNetworkDataLane>>();

        // 双方向フラグ
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public bool IsBothWay { get; set; } = true;
    }
}