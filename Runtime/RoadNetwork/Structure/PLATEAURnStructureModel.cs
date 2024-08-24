using PLATEAU.RoadNetwork.Data;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    public class PLATEAURnStructureModel : MonoBehaviour, IRoadNetworkObject
    {
        // シリアライズ用フィールド
        [SerializeField]
        private RoadNetworkStorage storage;

        public RnModel RoadNetwork { get; set; }


        public void Serialize()
        {
            if (RoadNetwork == null)
                return;
            storage = RoadNetwork.Serialize();
        }

        public void Deserialize()
        {
            RoadNetwork ??= new RnModel();
            RoadNetwork.Deserialize(storage);
        }

        public RoadNetworkDataGetter GetRoadNetworkDataGetter()
        {
            return new RoadNetworkDataGetter(storage);
        }
    }
}