using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnWay))]
    public class RnDataWay : IPrimitiveData
    {
        // #TODO : 消える予定
        // 自分自身を表すId
        [field: SerializeField]
        public RnID<RnDataWay> MyId { get; set; }

        // LineStringの向きが逆かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.IsReversed))]
        public bool IsReversed { get; set; }

        // 法線計算用. 進行方向左側が道かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.IsRightSide))]
        public bool IsRightSide { get; set; }

        // 頂点
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.LineString))]
        public RnID<RnDataLineString> LineString { get; set; }

    }
}