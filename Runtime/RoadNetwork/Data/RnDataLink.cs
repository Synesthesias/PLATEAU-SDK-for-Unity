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

        // 即性情報
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnLinkAttribute RnLinkAttribute { get; set; }
    }
}