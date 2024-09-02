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
            // シリアライズ前に一度全レーンに対して中央線を作成する
            foreach (var road in RoadNetwork.Roads)
            {
                foreach (var l in road.AllLanes)
                    l.CreateCenterWay();
            }
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