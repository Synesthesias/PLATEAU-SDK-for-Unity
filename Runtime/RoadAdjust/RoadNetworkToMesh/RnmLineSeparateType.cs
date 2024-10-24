namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary> 道路変更で車線ごとにゲームオブジェクトを分けるかどうかです。RnmはRoad Network to Meshの略です。 </summary>
    public enum RnmLineSeparateType
    {
        /// <summary> 車線で分けない </summary>
        Combine,
        /// <summary> 車線で分ける </summary>
        Separate
    }
}