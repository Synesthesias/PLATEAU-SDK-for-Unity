using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.CommonDataStructure;
using UnityEngine;
using TReadOnlyDict = System.Collections.ObjectModel.ReadOnlyDictionary<PLATEAU.Util.CityObjectTypeExtension.KeyType, PLATEAU.CityGML.CityObjectType>;
using TDict = System.Collections.Generic.Dictionary<PLATEAU.Util.CityObjectTypeExtension.KeyType, PLATEAU.CityGML.CityObjectType>;

namespace PLATEAU.Util
{
    /// <summary>
    /// <see cref="CityObjectType"/> を分類します。
    /// </summary>
    /// 
    /// 補足:
    /// CityObjectType の一覧は CityObject.cs に定義された通りですが、
    /// それらを gmlタイプと LOD によって分類します。
    internal static class CityObjectTypeExtension
    {
        // TODO 全体的に名前を分かりやすくする
        private static readonly TReadOnlyDict dict;
        private static readonly ReadOnlyDictionary<CityObjectType, string> displayDict;

        public static CityObjectType GetFlags(GmlType gmlType, int lod)
        {
            return dict[Key(gmlType, lod)];
        }

        public static CityObjectType GetFlags(GmlType gmlType, MinMax<int> lodRange)
        {
            CityObjectType ret = 0;
            for (int i = lodRange.Min; i <= lodRange.Max; i++)
            {
                ret |= GetFlags(gmlType, i);
            }

            return ret;
        }

        static CityObjectTypeExtension()
        {
            // displayDict の初期化
            displayDict = new ReadOnlyDictionary<CityObjectType, string>(
                new Dictionary<CityObjectType, string>
                {
                    { CityObjectType.COT_GenericCityObject, "汎用都市オブジェクト" },
                    { CityObjectType.COT_Building , "建築物"},
                    { CityObjectType.COT_Room , "(仕様外)部屋"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BuildingInstallation, "建築付属物"},
                    { CityObjectType.COT_BuildingFurniture, "(仕様外)建築設備"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Door, "出入口(ドア等)"}, // 国交省仕様書を見るとDoorの定義で「ドア」とは書いてなく、「開口部のうち人や物の出入りを目的とするもの」と書いてあるので「ドア」より「出入口」が近いと判断
                    { CityObjectType.COT_Window, "窓"},
                    { CityObjectType.COT_CityFurniture, "都市設備"},
                    { CityObjectType.COT_Track, "(仕様外)Track"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Road , "道路"},
                    { CityObjectType.COT_Railway, "(仕様外)鉄道"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_Square, "(仕様外)Square" }, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_PlantCover, "植生範囲" },
                    { CityObjectType.COT_SolitaryVegetationObject, "単独木"},
                    { CityObjectType.COT_WaterBody, "水域"},
                    { CityObjectType.COT_ReliefFeature, "地形"},
                    { CityObjectType.COT_ReliefComponent, "地形コンポーネント"},
                    { CityObjectType.COT_TINRelief, "地形(不規則三角網)"},
                    { CityObjectType.COT_MassPointRelief, "地形(点群)"},
                    { CityObjectType.COT_BreaklineRelief, "(仕様外)BreakLineRelief"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_RasterRelief, "(仕様外)RasterRelief"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_LandUse, "土地利用"},
                    { CityObjectType.COT_Tunnel, "(仕様外)Tunnel"}, // 国交省の仕様書には定義は見当たらない
                    { CityObjectType.COT_Bridge, "(仕様外)Bridge"}, // 国交省の仕様書には定義は見当たらない
                    { CityObjectType.COT_BridgeConstructionElement, "(仕様外)BridgeConstructionElement"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BridgeInstallation, "(仕様外)BridgeInstallation"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BridgePart, "(仕様外)BridgePart"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_BuildingPart, "建築物部分"},
                    { CityObjectType.COT_WallSurface, "壁面"},
                    { CityObjectType.COT_RoofSurface, "建築物上部(屋根等)"}, // 国交省の仕様書の定義では「屋根」とは書いておらず、「建築物の上部を覆う構造物」と書いてあります。
                    { CityObjectType.COT_GroundSurface, "建築物底面" },
                    { CityObjectType.COT_ClosureSurface, "便宜上閉じる扱いにした開口部"},
                    { CityObjectType.COT_FloorSurface, "(仕様外)FloorSurface"}, // 国交省の仕様書には見当たらず、代わりに「OuterFloorSurface」という名前になっている
                    { CityObjectType.COT_InteriorWallSurface, "(仕様外)InteriorWallSurface"}, // 国交省の仕様書には見当たらない
                    { CityObjectType.COT_CeilingSurface, "(仕様外)CeilingSurface"}, // 国交省の仕様書には見当たらず、代わりに「OuterCeilingSurce」という名前になっている
                    { CityObjectType.COT_CityObjectGroup, "都市オブジェクトの集まり"},
                    { CityObjectType.COT_OuterCeilingSurface, "建築物外側のうち天井にもなる部分"},
                    { CityObjectType.COT_OuterFloorSurface, "通行可能屋根"},
                    { CityObjectType.COT_TransportationObject, "交通"},
                    { CityObjectType.COT_IntBuildingInstallation, "建築物付属設備"}, // FIXME : 国交省の仕様書だと名前に「Int」は付いてないけど、これで合っているのか？
                    
                }
            );

            // (gmlタイプ, lod） で、とりうる組み合わせを全て列挙して Key とします。
            // Value は 0 (フラグ全部 false のEnumとみなされます) とします。
            var d = new TDict();
            var gmlTypes = Enum.GetValues(typeof(GmlType)).OfType<GmlType>();
            foreach (var gmlType in gmlTypes)
            {
                var lodRange = gmlType.PossibleLodRange();
                for (int i = lodRange.Min; i <= lodRange.Max; i++)
                {
                    var key = new KeyType(i, gmlType);
                    var initialFlags = (CityObjectType)0;
                    d.Add(key, initialFlags);
                }
            }
             
            // 辞書に値を登録します。
            SetFlags(d, GmlType.Building, 2,
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface |
                CityObjectType.COT_GroundSurface |
                CityObjectType.COT_ClosureSurface |
                CityObjectType.COT_OuterFloorSurface |
                CityObjectType.COT_OuterCeilingSurface
            );
            SetFlags(d, GmlType.Building, 3,
                CityObjectType.COT_Door |
                CityObjectType.COT_Window |
                d[Key(GmlType.Building, 2)]
            );
            SetFlags(d, GmlType.Transport, 3,
                CityObjectType.COT_TransportationObject
            );
            SetFlags(d, GmlType.Transport, 2, d[Key(GmlType.Transport, 3)]);
            
            // 辞書を保存します。
            dict = new TReadOnlyDict(d);
        }

        /// <summary>
        /// 引数をビット列（フラグの集まり）として見ます。
        /// 右のフラグから順に確認し、立っているフラグに対応する CityObjectType を順番に返します。
        /// </summary>
        public static IEnumerable<CityObjectType> ForEachTypes(this CityObjectType typeFlags)
        {
            ulong flags = (ulong)typeFlags;
            int shiftCount = 0;
            while (flags != 0)
            {
                if ((flags & 1) == 1)
                {
                    yield return (CityObjectType)(1ul << shiftCount);
                }
                flags >>= 1;
                shiftCount++;
                if (shiftCount > 99999)
                {
                    throw new Exception("無限ループのフェイルセーフが発動しました。");
                }
            }
        }

        /// <summary>
        /// 引数をビット列（フラグの集まり）として見ます。
        /// 右のフラグから順に確認し、立っているフラグに対応する CityObjectType を配列で返します。
        /// </summary>
        public static CityObjectType[] ToTypeArray(this CityObjectType typeFlags)
        {
            return typeFlags.ForEachTypes().ToArray();
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

        private static void SetFlags(TDict d, GmlType gmlType, int lod, CityObjectType cityObjTypeFlags)
        {
            var key = Key(gmlType, lod);
            d[key] = cityObjTypeFlags;
        }

        private static KeyType Key(GmlType gmlType, int lod)
        {
            return new KeyType(lod, gmlType);
        }
        
        

        public struct KeyType : IEquatable<KeyType>
        {
            private readonly int lod;
            private readonly GmlType gmlType;

            public KeyType(int lod, GmlType gmlType)
            {
                this.lod = lod;
                this.gmlType = gmlType;
            }

            public bool Equals(KeyType other)
            {
                return this.lod == other.lod && this.gmlType == other.gmlType;
            }

            public override string ToString()
            {
                return $"gmlType = {this.gmlType.ToString()} , lod = {this.lod}";
            }
        }
    }
}