using PLATEAU.CityGML;

namespace PLATEAU.IO
{
    /// <summary>
    /// 都市モデル内の地物について「主要である」かどうかを判断するための静的メソッドを提供します。
    /// LOD2,3の細分化された地物ではない地物は全て「主要である」とみなされます。
    /// </summary>
    public static class PrimaryCityObjectTypes
    {
        /// <summary>
        /// 主要な都市オブジェクトタイプのマスク
        /// </summary>
        public const CityObjectType PrimaryTypeMask =
            ~(
                // LOD3建築物の部品
                CityObjectType.COT_Door |
                CityObjectType.COT_Window |
                // LOD2建築物の部品
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface |
                CityObjectType.COT_GroundSurface |
                CityObjectType.COT_ClosureSurface |
                CityObjectType.COT_OuterFloorSurface |
                CityObjectType.COT_OuterCeilingSurface |
                // LOD2,3交通
                CityObjectType.COT_TransportationObject
            );

        /// <summary>
        /// 都市オブジェクトタイプが主要であるかどうかを取得します。
        /// </summary>
        /// <param name="type">都市オブジェクトタイプ</param>
        /// <returns>主要である場合true, そうでなければfalse</returns>
        public static bool IsPrimary(CityObjectType type)
        {
            return PrimaryTypeMask.HasFlag(type);
        }
    }
}
