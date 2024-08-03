using PLATEAU.RoadNetwork.Data;
using System;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnBlock : ARnParts<RnBlock>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // 識別Id. シリアライズ用.ランタイムでは使用しないこと
        public RnID<RnDataBlock> MyId { get; set; }

        // 親リンク
        public RnRoad ParentLink { get; set; }

        // 所属レーンタイプ(0:レーン, 1:トラック)
        public int LaneType { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
    }
}