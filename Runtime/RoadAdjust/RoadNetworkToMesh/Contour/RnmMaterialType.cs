namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュ化したときに、どのマテリアルを割り当てるかです。
    /// </summary>
    public enum RnmMaterialType
    {
        CarLane, // 車道
        SideWalk, // 歩道
        MedianLane, // 中央分離帯
    }
}