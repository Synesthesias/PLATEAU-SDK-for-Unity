using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable, RoadNetworkSerializeData(typeof(RnRoadBase))]
    public class RnDataRoadBase : IPrimitiveData
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember("sideWalks")]
        public List<RnID<RnDataSideWalk>> SideWalks { get; set; } = new();
    }
}