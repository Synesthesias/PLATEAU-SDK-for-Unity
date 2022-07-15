﻿using System;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.CommonDataStructure;
using TReadOnlyDict = System.Collections.ObjectModel.ReadOnlyDictionary<PLATEAU.Util.CityObjectTypeExtensions.CityObjectTypeClassification.KeyType, PLATEAU.CityGML.CityObjectType>;
using TDict = System.Collections.Generic.Dictionary<PLATEAU.Util.CityObjectTypeExtensions.CityObjectTypeClassification.KeyType, PLATEAU.CityGML.CityObjectType>;

namespace PLATEAU.Util.CityObjectTypeExtensions
{
    /// <summary>
    /// <see cref="CityObjectType"/> を分類します。
    /// (GmlType, LOD) の値の組から、その組で使用上ありえる <see cref="CityObjectType"/> のフラグ群（ビット列）を返します。
    ///
    /// 注意:
    /// このクラスは未完成です。
    /// GmlType が建築物の場合のみ正しい値を返します。
    /// それ以外の GmlType については、今のところ利用ニーズがないので分類を作っておらず、0を返します。
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

        public static ulong GetFlags(GmlType gmlType, MinMax<int> lodRange)
        {
            ulong flags = 0;
            for (int i = lodRange.Min; i <= lodRange.Max; i++)
            {
                flags |= (ulong)GetFlags(gmlType, i);
            }

            return flags;
        }

        static CityObjectTypeClassification()
        {
            dict = new TReadOnlyDict(GenerateDictionary());
        }

        private static TDict GenerateDictionary()
        {
            var d = new TDict();
            
            // (gmlタイプ, lod） の組について、とりうる組み合わせを全て列挙して Key とします。
            // Value は初期値として 0 (フラグ全部 false のEnumとみなされます) にします。
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
            SetFlags(d, GmlType.Building, 0,
                CityObjectType.COT_Building);
            SetFlags(d, GmlType.Building, 1,
                d[Key(GmlType.Building, 0)]);
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
            
            // TODO　以下に SetFlags の続きを記載します。
            // 現状、正しく登録できているのは GmlType.Building だけです。

            return d;
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
        
        
        /// <summary>
        /// 分類のキーであるインナー構造体です。
        /// </summary>
        public readonly struct KeyType : IEquatable<KeyType>
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