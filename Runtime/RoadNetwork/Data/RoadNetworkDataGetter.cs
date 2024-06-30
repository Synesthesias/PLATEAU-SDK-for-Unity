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

        public IReadOnlyList<RoadNetworkDataLane> GetLanes()
        {
            return primStorage.Lanes.DataList;
        }
        public IReadOnlyList<RoadNetworkDataBlock> GetBlocks()
        {
            return primStorage.Blocks.DataList;
        }
        public IReadOnlyList<RoadNetworkDataWay> GetWays()
        {
            return primStorage.Ways.DataList;
        }
        public IReadOnlyList<RoadNetworkDataLineString> GetLineStrings()
        {
            return primStorage.LineStrings.DataList;
        }
        public IReadOnlyList<RoadNetworkDataPoint> GetPoints()
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
            TestNull(nameof(RoadNetworkDataLane), GetLanes());
            TestNull(nameof(RoadNetworkDataBlock), GetBlocks());
            TestNull(nameof(RoadNetworkDataWay), GetWays());
            TestNull(nameof(RoadNetworkDataLineString), GetLineStrings());
            TestNull(nameof(RoadNetworkDataPoint), GetPoints());

            void TestNull(string name, object a)
            {
                if (a != null)
                    return;
                Debug.Log(name + " is null.");
            }
        }

    }
}
