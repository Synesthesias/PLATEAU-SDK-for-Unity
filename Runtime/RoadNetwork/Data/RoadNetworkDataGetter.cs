using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// ���H�l�b�g���[�N��RoadNetworkData�����n���̃f�[�^���܂Ƃ߂Ď���Ă���@�\��񋟂���N���X
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

            Test();
        }

        private PrimitiveDataStorage primStorage;

        /// <summary>
        /// �f�[�^�̎擾�֐��Q
        /// </summary>
        /// <param name="data"></param>
        
        public IReadOnlyList<RoadNetworkDataNode> GetNodes()
        {
            return primStorage.Nodes.DataList;
        }
        public IReadOnlyList<RoadNetworkDataTrack> GetTracks()
        {
            return primStorage.Tracks.DataList;
        }
        public IReadOnlyList<RoadNetworkDataLink> GetLinks()
        {
            return primStorage.Links.DataList;
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
        /// �f�[�^�̎擾�֐��Q
        /// </summary>
        /// <param name="data"></param>

        public void Test()
        {
            TestNull(nameof(RoadNetworkDataNode), GetNodes());
            TestNull(nameof(RoadNetworkDataTrack), GetTracks());
            TestNull(nameof(RoadNetworkDataLink), GetLinks());
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
