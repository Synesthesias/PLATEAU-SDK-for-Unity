using System;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Data
{
    [Serializable]
    public class RoadNetworkDataLink : IPrimitiveData
    {
        public RnNodeId nextNodeId;
        public RnNodeId prevNodeId;
        public List<RnLaneId> mainLaneIds = new List<RnLaneId>();
        public List<RnLaneId> leftLaneIds = new List<RnLaneId>();
        public List<RnLaneId> rightLaneIds = new List<RnLaneId>();
    }
}