using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkBlock
    {
        // 識別Id(負数の場合は設定されていない). デバッグ用なので参照ポインタが割にはしないこと
        public int DebugId { get; set; } = -1;

        // 親リンク
        public RoadNetworkLink ParentLink { get; set; }

        // 所属レーンタイプ(0:レーン, 1:トラック)
        public int LaneType { get; set; }

    }
}