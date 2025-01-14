using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;

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
