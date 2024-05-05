using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkBlock
    {
        // 親リンク
        public RoadNetworkLink ParentLink { get; set; }

        // 所属レーンタイプ(0:レーン, 1:トラック)
        public int LaneType { get; set; }

    }
}