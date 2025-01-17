using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// 道路ネットワークのRoadNetworkData○○系統のデータをまとめて取ってくる機能を提供するクラス
    /// </summary>
    public class RoadNetworkDataGetter
    {
        internal RoadNetworkDataGetter(RoadNetworkStorage storage)
        {
            primStorage = storage.PrimitiveDataStorage;

            //RoadNetworkDataNode
            //RoadNetworkDataTrack
            //RoadNetworkDataLink
            //RoadNetworkDataLane
            //RoadNetworkDataBlock
            //RoadNetworkDataWay
            //RoadNetworkDataLineString
            //RoadNetworkDataPoint

            Validate();
        }

        private PrimitiveDataStorage primStorage;


        /// <summary>
        /// 取得したデータ群からIDテーブルを生成する
        /// 注意　要素数の増減があった時には再度テーブルを作成する必要がある
        /// 
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<_Type, RnID<_Type>> GenerateIdTable<_Type>(IReadOnlyList<_Type> dataList)
            where _Type : IPrimitiveData
        {
            Dictionary<_Type, RnID<_Type>> table = new Dictionary<_Type, RnID<_Type>>(dataList.Count);
            for (int i = 0; i < dataList.Count; i++)
            {
                var g = GetIDGeneratable<_Type>();
                table.Add(dataList[i], new RnID<_Type>(i, g));
            }
            return table;
        }

        /// <summary>
        /// データの取得関数群
        /// </summary>
        /// <param name="data"></param>

        public IReadOnlyList<RnDataRoadBase> GetRoadBases()
        {
            return primStorage.RoadBases.DataList;
        }

        public IReadOnlyList<RnDataLane> GetLanes()
        {
            return primStorage.Lanes.DataList;
        }
        public IReadOnlyList<RnDataWay> GetWays()
        {
            return primStorage.Ways.DataList;
        }
        public IReadOnlyList<RnDataLineString> GetLineStrings()
        {
            return primStorage.LineStrings.DataList;
        }
        public IReadOnlyList<RnDataPoint> GetPoints()
        {
            return primStorage.Points.DataList;
        }

        public IReadOnlyList<RnDataTrafficLightController> GetTrafficLightController()
        {
            return primStorage.TrafficLightControllers.DataList;
        }

        public IReadOnlyList<RnDataTrafficLight> GetTrafficLights()
        {
            return primStorage.TrafficLights.DataList;
        }

        public IReadOnlyList<RnDataTrafficSignalPattern> GetTrafficSignalPattern()
        {
            return primStorage.TrafficSignalPatterns.DataList;
        }

        public IReadOnlyList<RnDataTrafficSignalPhase> GetTrafficSignalPhase()
        {
            return primStorage.TrafficSignalPhases.DataList;
        }

        /// <summary>
        /// データ検証
        /// </summary>
        /// <param name="data"></param>
        public void Validate()
        {
            TestNull(nameof(RnRoadBase), GetRoadBases());
            TestNull(nameof(RnDataLane), GetLanes());
            TestNull(nameof(RnDataWay), GetWays());
            TestNull(nameof(RnDataLineString), GetLineStrings());
            TestNull(nameof(RnDataPoint), GetPoints());
            TestNull(nameof(RnDataTrafficLightController), GetTrafficLightController());
            TestNull(nameof(RnDataTrafficLight), GetTrafficLights());
            TestNull(nameof(RnDataTrafficSignalPattern), GetTrafficSignalPattern());
            TestNull(nameof(RnDataTrafficSignalPhase), GetTrafficSignalPhase());

            void TestNull(string name, object a)
            {
                if (a != null)
                    return;
                Debug.Log(name + " is null.");
            }
        }

        private IRnIDGeneratable GetIDGeneratable<_Type>()
            where _Type : IPrimitiveData
        {
            switch (typeof(_Type).Name)
            {
                case nameof(RnDataRoadBase):
                    return primStorage.RoadBases;
                case nameof(RnDataLane):
                    return primStorage.Lanes;
                case nameof(RnDataWay):
                    return primStorage.Ways;
                case nameof(RnDataLineString):
                    return primStorage.LineStrings;
                case nameof(RnDataPoint):
                    return primStorage.Points;
                case nameof(RnDataTrafficLightController):
                    return primStorage.TrafficLightControllers;
                case nameof(RnDataTrafficLight):
                    return primStorage.TrafficLights;
                case nameof(RnDataTrafficSignalPattern):
                    return primStorage.TrafficSignalPatterns;
                case nameof(RnDataTrafficSignalPhase):
                    return primStorage.TrafficSignalPhases;
            }

            Assert.IsTrue(false, "未対応の型");   // 未対応の型
            return null;
        }
    }
}
