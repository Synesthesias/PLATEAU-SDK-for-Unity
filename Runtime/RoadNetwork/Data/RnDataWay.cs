using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnWay))]
    public class RnDataWay : IPrimitiveData
    {
        // LineStringの向きが逆かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.IsReversed))]
        public bool IsReversed { get; set; }

        // 法線計算用. 法線が進行方向に対して逆かどうか
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.IsReverseNormal))]
        public bool IsReverseNormal { get; set; }

        // 頂点
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnWay.LineString))]
        public RnID<RnDataLineString> LineString { get; set; }

        /// <summary>
        /// 同じ線分かどうか(向きや法線の逆転は考慮しない)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameLine(RnDataWay other)
        {
            return LineString.IsValid && LineString == other.LineString;
        }
    }
}