using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public bool SetTrafficSignalLightController(in RnTrafficLightDataSet dataSet)
        {
            Debug.Log("Called SetTrafficSignalLightController()");

            var isSuc = dataSet.IsValid();
            if (!isSuc)
            {
                Debug.LogError("Invalid data");
                return false;
            }

            // 既存データをクリア
            primStorage.TrafficLightControllers.ClearAll();
            primStorage.TrafficLights.ClearAll();
            primStorage.TrafficSignalPatterns.ClearAll();
            primStorage.TrafficSignalPhases.ClearAll();

            primStorage.TrafficLightControllers.WriteNew(dataSet.Controllers.ToArray());
            primStorage.TrafficLights.WriteNew(dataSet.Lights.ToArray());
            primStorage.TrafficSignalPatterns.WriteNew(dataSet.SignalPatterns.ToArray());
            primStorage.TrafficSignalPhases.WriteNew(dataSet.SignalPhases.ToArray());

            return true;
        }

        private PrimitiveDataStorage primStorage;

    }
}
