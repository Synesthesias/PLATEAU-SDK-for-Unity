using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnRoadBase))]
    public class RnDataRoadBase : IPrimitiveData
    {
        // TODO : 互換性のために残しておく. シミュレーター側で使っているようなので.使われなくなったら消す
        [Obsolete("UseTargetTrans")]
        public PLATEAUCityObjectGroup TargetTran => TargetTrans.FirstOrDefault();

        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public List<PLATEAUCityObjectGroup> TargetTrans { get; set; } = new List<PLATEAUCityObjectGroup>();

        [field: SerializeField]
        [RoadNetworkSerializeMember("sideWalks")]
        public List<RnID<RnDataSideWalk>> SideWalks { get; set; } = new();
    }
}