using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// 既存の道路ネットワークのデータ構造を変更する機能を提供するクラス
    /// </summary>
    public class RoadNetworkDataSetter
    {
        internal RoadNetworkDataSetter(RoadNetworkStorage storage)
        {
            primStorage = storage.PrimitiveDataStorage;
        }

        /// <summary>
        /// 信号制御器の設定を行う
        /// </summary>
        /// <param name="traffics"></param>
        /// <returns></returns>
        public bool SetTrafficSignalLightController(
            IReadOnlyCollection<RnDataTrafficLightController> traffics,
            IReadOnlyCollection<RnDataTrafficSignalPattern> signalPatterns,
            IReadOnlyCollection<RnDataTrafficSignalPhase> signalPhases)
        {
            Debug.Log("Called SetTrafficSignalLightController()");

            // 整合性のチェック
            if (traffics == null || signalPatterns == null || signalPhases == null)
            {
                Debug.LogError("traffics, signalPatterns, signalPhases is null");
                return false;
            }

            //...
            return true;
        }

        private PrimitiveDataStorage primStorage;


    }
}
