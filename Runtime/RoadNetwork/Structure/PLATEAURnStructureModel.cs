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

        /// <summary>
        /// 道路ネットワークのデータを設定するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataSetter GetRoadNetworkDataSetter()
        {
            return new RoadNetworkDataSetter(storage);
        }

        /// <summary>
        /// 道路ネットワークのデータを取得するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataGetter GetRoadNetworkDataGetter()
        {
            return new RoadNetworkDataGetter(storage);
        }
    }
}