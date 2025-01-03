using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork
{
    internal class RoadNetworkAddSystemContext
    {
        public RnModel RoadNetwork { get; private set; }
        public RoadNetworkSkeletonData SkeletonData { get; private set; }

        public RoadNetworkAddSystemContext(PLATEAURnStructureModel structureModel)
        {
            SkeletonData = new RoadNetworkSkeletonData(structureModel);
            RoadNetwork = structureModel.RoadNetwork;
        }
    }
}
