using System;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkTrack
    {
        // 識別Id(負数の場合は設定されていない). デバッグ用なので参照ポインタが割にはしないこと
        public int DebugId { get; set; } = -1;

    }
}