using System;

namespace PLATEAU.Udx
{
    [Flags]
    public enum PredefinedCityModelPackage : uint
    {
        None = 0,
        //! 建築物
        Building = 1u,
        //! 道路
        Road = 1u << 1,
        //! 都市計画決定情報
        UrbanPlanningDecision = 1u << 2,
        //! 土地利用
        LandUse = 1u << 3,
        //! 都市設備
        CityFurniture = 1u << 4,
        //! 植生
        Vegetation = 1u << 5,
        //! 起伏
        Relief = 1u << 6,
        //! 災害リスク
        DisasterRisk = 1u << 7,
        //! その他
        Unknown = 1u << 31
    }
}
