using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkWay))]
    public class RoadNetworkDataWay : IPrimitiveData
    {
        // 自分自身を表すId
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkWay.MyId))]
        public RnId<RoadNetworkDataWay> MyId { get; set; }

        // LineStringの向きが逆かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkWay.IsReversed))]
        public bool IsReversed { get; set; }

        // 法線計算用. 進行方向左側が道かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkWay.IsRightSide))]
        public bool IsRightSide { get; set; }

        // 頂点
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkWay.LineString))]
        public RnId<RoadNetworkDataLineString> LineString { get; set; }

    }
}