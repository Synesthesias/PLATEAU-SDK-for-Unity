using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLATEAU.CityGML;

namespace PLATEAU.Util.CityObjectTypeExtensions
{
    /// <summary>
    /// <see cref="CityObjectType"/> を分かりやすい日本語文字に変換します。
    /// </summary>
    internal static class CityObjectTypeDisplay
    {
        private static readonly ReadOnlyDictionary<CityObjectType, string> displayDict;

        static CityObjectTypeDisplay()
        {
            displayDict = GenerateDisplayDict();
        }
        
        /// <summary>
        /// <see cref="CityObjectType"/> を分かりやすい日本語文字にして返します。
        /// </summary>
        public static string ToDisplay(this CityObjectType typeFlags)
        {
            if(displayDict.TryGetValue(typeFlags, out var val))
            {
                return val;
            }

            return $"不明なタイプ 0x{Convert.ToString((long)typeFlags, 16)}";
        }

        private static ReadOnlyDictionary<CityObjectType, string> GenerateDisplayDict()
        {
            return new ReadOnlyDictionary<CityObjectType, string>(
                new Dictionary<CityObjectType, string>
                {
                    { CityObjectType.COT_GenericCityObject, "GenericCityObject(汎用都市オブジェクト)" },
                    { CityObjectType.COT_Building , "Building(建築物)"},
                    { CityObjectType.COT_Room , "(仕様外)Room(部屋)"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BuildingInstallation, "BuildingInstallation(建築付属物)"},
                    { CityObjectType.COT_BuildingFurniture, "(仕様外)BuildingFurniture(建築設備)"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Door, "Door(出入口)"}, // 国交省仕様書を見るとDoorの定義で「ドア」とは書いてなく、「開口部のうち人や物の出入りを目的とするもの」と書いてあるので「ドア」より「出入口」が近いと判断
                    { CityObjectType.COT_Window, "Window(窓)"},
                    { CityObjectType.COT_CityFurniture, "CityFurniture(都市設備)"},
                    { CityObjectType.COT_Track, "(仕様外)Track"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Road , "Road(道路)"},
                    { CityObjectType.COT_Railway, "(仕様外)Railway"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Square, "(仕様外)Square" }, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_PlantCover, "PlantCover(植生範囲)" },
                    { CityObjectType.COT_SolitaryVegetationObject, "SolitaryVegetationObject(単独木)"},
                    { CityObjectType.COT_WaterBody, "WaterBody(水域)"},
                    { CityObjectType.COT_ReliefFeature, "ReliefFeature(地形)"},
                    { CityObjectType.COT_ReliefComponent, "ReliefComponent(地形コンポーネント)"},
                    { CityObjectType.COT_TINRelief, "TINRelief(地形/不規則三角網)"},
                    { CityObjectType.COT_MassPointRelief, "MassPointRelief(地形/点群)"},
                    { CityObjectType.COT_BreaklineRelief, "(仕様外)BreakLineRelief"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_RasterRelief, "(仕様外)RasterRelief"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_LandUse, "(LandUse)土地利用"},
                    { CityObjectType.COT_Tunnel, "(仕様外)Tunnel"}, // 国交省の仕様書には定義は見当たらない
                    { CityObjectType.COT_Bridge, "(仕様外)Bridge"}, // 国交省の仕様書には定義は見当たらない
                    { CityObjectType.COT_BridgeConstructionElement, "(仕様外)BridgeConstructionElement"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BridgeInstallation, "(仕様外)BridgeInstallation"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BridgePart, "(仕様外)BridgePart"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BuildingPart, "(BuildingPart)建築物部分"},
                    { CityObjectType.COT_WallSurface, "WallSurface(壁面)"},
                    { CityObjectType.COT_RoofSurface, "RoofSurface(屋根面)"}, // 国交省の仕様書の定義では「屋根」とは書いておらず、「建築物の上部を覆う構造物」と書いてあります。
                    { CityObjectType.COT_GroundSurface, "GroundSurface(地面)" },
                    { CityObjectType.COT_ClosureSurface, "ClosureSurface(閉口部)"},
                    { CityObjectType.COT_FloorSurface, "(仕様外)FloorSurface"}, // 国交省の仕様書には見当たらず、代わりに「OuterFloorSurface」という名前になっている
                    { CityObjectType.COT_InteriorWallSurface, "(仕様外)InteriorWallSurface"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_CeilingSurface, "(仕様外)CeilingSurface"}, // 国交省の仕様書には見当たらず、代わりに「OuterCeilingSurce」という名前になっている
                    { CityObjectType.COT_CityObjectGroup, "CityObjectGroup(都市オブジェクトの集まり)"},
                    { CityObjectType.COT_OuterCeilingSurface, "OuterCeilingSurface(開口部の天井)"},
                    { CityObjectType.COT_OuterFloorSurface, "OuterFloorSurface(開口部の底面)"},
                    { CityObjectType.COT_TransportationObject, "TransportationObject(交通)"},
                    { CityObjectType.COT_IntBuildingInstallation, "IntBuildingInstallation(建築物付属設備)"}, // FIXME : 国交省の仕様書だと名前に「Int」は付いてないけど、これで合っているのか？
                }
            );
        }
    }
}