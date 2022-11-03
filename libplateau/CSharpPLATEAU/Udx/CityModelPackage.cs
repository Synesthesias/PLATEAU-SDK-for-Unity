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

    public static class PredefinedCityModelPackageExtension
    {
        public static string ToJapaneseName(this PredefinedCityModelPackage package)
        {
            switch (package)
            {
                case PredefinedCityModelPackage.Building:
                    return "建築物";
                case PredefinedCityModelPackage.Road:
                    return "道路";
                case PredefinedCityModelPackage.UrbanPlanningDecision:
                    return "都市計画決定情報";
                case PredefinedCityModelPackage.LandUse:
                    return "土地利用";
                case PredefinedCityModelPackage.CityFurniture:
                    return "都市設備";
                case PredefinedCityModelPackage.Vegetation:
                    return "植生";
                case PredefinedCityModelPackage.Relief:
                    return "土地起伏";
                case PredefinedCityModelPackage.DisasterRisk:
                    return "災害リスク";
                case PredefinedCityModelPackage.Unknown:
                    return "その他";
                case PredefinedCityModelPackage.None:
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
