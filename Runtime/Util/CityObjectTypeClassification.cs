using System;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using TReadOnlyDict = System.Collections.ObjectModel.ReadOnlyDictionary<PLATEAU.Util.CityObjectTypeClassification.KeyType, PLATEAU.CityGML.CityObjectType>;
using TDict = System.Collections.Generic.Dictionary<PLATEAU.Util.CityObjectTypeClassification.KeyType, PLATEAU.CityGML.CityObjectType>;

namespace PLATEAU.Util
{
    /// <summary>
    /// <see cref="CityObjectType"/> を分類します。
    /// </summary>
    /// 
    /// 補足:
    /// CityObjectType の一覧は CityObject.cs に定義された通りですが、
    /// それらを gmlタイプと LOD によって分類します。
    internal static class CityObjectTypeClassification
    {
        private static readonly TReadOnlyDict dict;

        public static CityObjectType GetFlags(GmlType gmlType, int lod)
        {
            return dict[Key(gmlType, lod)];
        }

        static CityObjectTypeClassification()
        {
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
            SetFlags(d, GmlType.Building, 3,
                CityObjectType.COT_Door |
                CityObjectType.COT_Window
            );
            SetFlags(d, GmlType.Building, 2,
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface |
                CityObjectType.COT_GroundSurface |
                CityObjectType.COT_ClosureSurface |
                CityObjectType.COT_OuterFloorSurface |
                CityObjectType.COT_OuterCeilingSurface
            );
            SetFlags(d, GmlType.Transport, 3,
                CityObjectType.COT_TransportationObject
            );
            SetFlags(d, GmlType.Transport, 2, d[Key(GmlType.Transport, 3)]);
            
            // 辞書を保存します。
            dict = new TReadOnlyDict(d);
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
        }
    }
}