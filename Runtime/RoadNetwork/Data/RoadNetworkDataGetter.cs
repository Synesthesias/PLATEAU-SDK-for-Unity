using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public IReadOnlyList<RnDataBlock> GetBlocks()
        {
            return primStorage.Blocks.DataList;
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

        /// <summary>
        /// データ検証
        /// </summary>
        /// <param name="data"></param>
        public void Validate()
        {
            TestNull(nameof(RnRoadBase), GetRoadBases());
            TestNull(nameof(RnDataLane), GetLanes());
            TestNull(nameof(RnDataBlock), GetBlocks());
            TestNull(nameof(RnDataWay), GetWays());
            TestNull(nameof(RnDataLineString), GetLineStrings());
            TestNull(nameof(RnDataPoint), GetPoints());

            void TestNull(string name, object a)
            {
                if (a != null)
                    return;
                Debug.Log(name + " is null.");
            }
        }

    }
}
