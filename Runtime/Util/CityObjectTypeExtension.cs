using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var pair in dict)
            {
                Debug.Log($"{pair.Key} => {Convert.ToString((long)pair.Value,2)}");
            }
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
                    Debug.Log("type: " + (CityObjectType)(1ul << shiftCount) + "/n0b" + Convert.ToString(1 << shiftCount, 2));
                    yield return (CityObjectType)(1ul << shiftCount);
                }
                // Debug.Log(Convert.ToString((long)flags,2));
                flags >>= 1;
                shiftCount++;
                if (shiftCount > 99999)
                {
                    throw new Exception("無限ループのフェイルセーフが発動しました。");
                }
            }
            // Debug.Log("foreach type end.");
        }

        /// <summary>
        /// 引数をビット列（フラグの集まり）として見ます。
        /// 右のフラグから順に確認し、立っているフラグに対応する CityObjectType を配列で返します。
        /// </summary>
        public static CityObjectType[] ToTypeArray(this CityObjectType typeFlags)
        {
            return typeFlags.ForEachTypes().ToArray();
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