using System;
using System.Collections.Generic;

namespace PLATEAU.Dataset
{
    // ここを変更したら、PredefinedCityModelPackage.All() も変更する必要があることに注意してください。
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
        //! 交通(鉄道) : rwy
        Railway = 1u << 8,
        //! 交通(航路) : wwy
        Waterway = 1u << 9,
        //! 水部 : wtr
        WaterBody = 1u << 10,
        //! 橋梁　 : brid
        Bridge = 1u << 11,
        //! 徒歩道 : trk
        Track = 1u << 12,
        //! 広場 : squr
        Square = 1u << 13,
        //! トンネル : tun
        Tunnel = 1u << 14,
        //! 地下埋設物 : unf
        UndergroundFacility = 1u << 15,
        //! 地下街 : ubld
        UndergroundBuilding = 1u << 16,
        //! 区域 : area 
        Area = 1u << 17,
        //! その他の構造物 : cons 
        OtherConstruction = 1u << 18,
        //! 汎用都市: gen
        Generic = 1u << 19,
        //! その他
        Unknown = 1u << 31
    }

    public static class PredefinedCityModelPackageExtension
    {
        /// <summary>
        /// <see cref="PredefinedCityModelPackage"/> の各パッケージのビットを立てたものを返します。
        /// </summary>
        public static PredefinedCityModelPackage All()
        {
            uint ret = ~(~0u << 20); // 0～19桁目のフラグを立てます。 (0から数えて)
            ret |= (uint)PredefinedCityModelPackage.Unknown; // Unknownのフラグを立てます。
            return (PredefinedCityModelPackage)ret;
        }
        
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
                        case PredefinedCityModelPackage.Railway:
                            names.Add("鉄道");
                            break;
                        case PredefinedCityModelPackage.Waterway:
                            names.Add("航路");
                            break;
                        case PredefinedCityModelPackage.WaterBody:
                            names.Add("水部");
                            break;
                        case PredefinedCityModelPackage.Bridge:
                            names.Add("橋梁");
                            break;
                        case PredefinedCityModelPackage.Track:
                            names.Add("徒歩道");
                            break;
                        case PredefinedCityModelPackage.Square:
                            names.Add("広場");
                            break;
                        case PredefinedCityModelPackage.Tunnel:
                            names.Add("トンネル");
                            break;
                        case PredefinedCityModelPackage.UndergroundFacility:
                            names.Add("地下埋設物");
                            break;
                        case PredefinedCityModelPackage.UndergroundBuilding:
                            names.Add("地下街");
                            break;
                        case PredefinedCityModelPackage.Area:
                            names.Add("区域");
                            break;
                        case PredefinedCityModelPackage.OtherConstruction:
                            names.Add("その他の構造物");
                            break;
                        case PredefinedCityModelPackage.Generic:
                            names.Add("汎用都市");
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
