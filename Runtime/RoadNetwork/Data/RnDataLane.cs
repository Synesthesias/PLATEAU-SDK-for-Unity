using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{

    [Serializable, RoadNetworkSerializeData(typeof(RnLane))]
    public class RnDataLane : IPrimitiveData
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnID<RnDataRoadBase> Parent { get; set; }

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

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public RnLaneAttribute Attributes { get; set; }

        // 中央線
        [field: SerializeField]
        [RoadNetworkSerializeMember("centerWay")]
        public RnID<RnDataWay> CenterWay { get; set; }

        // #TODO : IsReversedに変更予定.
        //       : 既存のデータの後方互換の為以下を入れるがそのテストを行った後にする
        //       : [field : FormerlySerializedAs("IsReverse")]
        // 親Roadと逆方向(右車線等)
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public bool IsReverse { get; set; }

    }

}