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
        /// 内部の実装や整合性のチェックは実装途中
        /// </summary>
        /// <param name="traffics"></param>
        /// <returns></returns>
        public bool SetTrafficSignalLightController(
            IReadOnlyCollection<RnDataTrafficLightController> traffics,
            IReadOnlyCollection<RnDataTrafficLight> lights,
            IReadOnlyCollection<RnDataTrafficSignalPattern> signalPatterns,
            IReadOnlyCollection<RnDataTrafficSignalPhase> signalPhases)
        {
            Debug.Log("Called SetTrafficSignalLightController()");

            // 整合性のチェック
            if (traffics == null || lights == null || signalPatterns == null || signalPhases == null)
            {
                Debug.LogError("traffics, signalPatterns, signalPhases is null");
                return false;
            }

            //...
            bool isSuccess = false;
            foreach (var traffic in traffics)
            {
                isSuccess = RnDataTrafficLightController.IsValid(traffic);
                if (!isSuccess)
                {
                    Debug.LogError("TrafficSignalLightController is invalid");
                    return false;
                }
            }
            return true;
        }

        private PrimitiveDataStorage primStorage;

    }
}
