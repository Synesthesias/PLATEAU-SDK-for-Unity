using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PLATEAU.RoadNetwork.Data.PrimitiveDataStorage;

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

        /// <summary>
        /// ID生成に必要な機能を提供する
        /// 注意　現在は信号制御器関係のみあつあう
        /// </summary>
        /// <typeparam name="TPrimType"></typeparam>
        /// <returns></returns>
        public IRnIDGeneratable CreateIdGenerator<TPrimType>()
            where TPrimType : IPrimitiveData
        {
            if (typeof(TPrimType) == typeof(RnDataRoadBase))
            {
                return new IDGeneratorProxy<RnDataRoadBase>(primStorage.RoadBases);
            }
            else if (typeof(TPrimType) == typeof(RnDataTrafficLightController))
            {
                return new IDGeneratorProxy<RnDataTrafficLightController>(primStorage.TrafficLightControllers);
            }
            else if (typeof(TPrimType) == typeof(RnDataTrafficLight))
            {
                return new IDGeneratorProxy<RnDataTrafficLight>(primStorage.TrafficLights);
            }
            else if (typeof(TPrimType) == typeof(RnDataTrafficSignalPattern))
            {
                return new IDGeneratorProxy<RnDataTrafficSignalPattern>(primStorage.TrafficSignalPatterns);
            }
            else if (typeof(TPrimType) == typeof(RnDataTrafficSignalPhase))
            {
                return new IDGeneratorProxy<RnDataTrafficSignalPhase>(primStorage.TrafficSignalPhases);
            }

            Debug.LogError("Invalid type");
            return null;
        }

        /// <summary>
        /// 代理でID生成を行うためのクラス
        /// </summary>
        /// <typeparam name="TPrimType"></typeparam>
        private class IDGeneratorProxy<TPrimType> : IRnIDGeneratable
            where TPrimType : IPrimitiveData
        {
            public IDGeneratorProxy(PrimitiveStorage<TPrimType> primStorage)
            {
                this.primStorage = primStorage;
            }

            private PrimitiveStorage<TPrimType> primStorage;

        }

        private PrimitiveDataStorage primStorage;

    }
}
