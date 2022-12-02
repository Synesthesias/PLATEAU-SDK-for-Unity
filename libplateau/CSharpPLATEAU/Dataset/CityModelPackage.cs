using System;
using System.Collections.Generic;

namespace PLATEAU.Dataset
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
        /// <summary>
        /// Package を日本語名にして返します。
        /// Package として複数のフラグが立っている場合、それらの日本語名を "," で繋いで返します。
        /// </summary>
        public static string ToJapaneseName(this PredefinedCityModelPackage package)
        {
            ulong flags = (ulong)package;
            if (flags == 0) return "なし";
            int i = 0;
            var names = new List<string>();
            while (flags != 0)
            {
                var flag = (PredefinedCityModelPackage)((flags & 1) << i);
                if (flag != 0)
                {
                    switch (flag)
                    {
                        case PredefinedCityModelPackage.Building:
                            names.Add( "建築物");
                            break;
                        case PredefinedCityModelPackage.Road:
                            names.Add( "道路");
                            break;
                        case PredefinedCityModelPackage.UrbanPlanningDecision:
                            names.Add( "都市計画決定情報");
                            break;
                        case PredefinedCityModelPackage.LandUse:
                            names.Add( "土地利用");
                            break;
                        case PredefinedCityModelPackage.CityFurniture:
                            names.Add( "都市設備");
                            break;
                        case PredefinedCityModelPackage.Vegetation:
                            names.Add( "植生");
                            break;
                        case PredefinedCityModelPackage.Relief:
                            names.Add( "土地起伏");
                            break;
                        case PredefinedCityModelPackage.DisasterRisk:
                            names.Add( "災害リスク");
                            break;
                        case PredefinedCityModelPackage.Unknown:
                            names.Add( "その他");
                            break;
                        default:
                            names.Add("不明");
                            break;
                        // throw new NotImplementedException();
                    }
                }
                
                flags >>= 1;
                i++;
            }

            return string.Join(", ", names);
        }
    }
}
