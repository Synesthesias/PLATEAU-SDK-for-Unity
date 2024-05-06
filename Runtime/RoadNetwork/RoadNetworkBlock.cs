using PLATEAU.RoadNetwork.Data;
using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkBlock
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnId<RoadNetworkDataBlock> MyId { get; set; }

        // 親リンク
        public RoadNetworkLink ParentLink { get; set; }

        // 所属レーンタイプ(0:レーン, 1:トラック)
        public int LaneType { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}