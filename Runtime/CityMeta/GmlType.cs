using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CommonDataStructure;
using static PLATEAU.CityMeta.GmlType;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// 地物タイプです。
    /// gml元データのファイル構造における udx/(地物タイプ) の(地物タイプ)に対応します。
    /// </summary>
    internal enum GmlType
    {
        /// <summary> 建築物 </summary>
        Building,
        /// <summary> 道路 </summary>
        Transport,
        /// <summary> 植生 </summary>
        Vegetation,
        /// <summary> 都市設備 </summary>
        CityFurniture,
        /// <summary> 起伏（地形情報） </summary>
        DigitalElevationModel,
        /// <summary> その他（都市計画決定情報、土地利用(luse, land use)、災害リスク） </summary>
        Etc
    }

    /// <summary>
    /// <see cref="GmlType"/> の Enum 型、 接頭辞（フォルダ名）、ディスプレイ文字列　を変換します。
    /// </summary>
    internal static class GmlTypeConvert
    {
        /// <summary>
        /// 接頭辞は主にフォルダ名に使われます。Enum型と接頭辞の対応辞書です。
        /// </summary>
        private static readonly IReadOnlyDictionary<GmlType, string> prefixDict = new Dictionary<GmlType, string>
        {
            { Building, "bldg" },
            { Transport, "trans" },
            { Vegetation, "veg" },
            { CityFurniture, "frn" },
            { DigitalElevationModel, "dem" }
        };

        /// <summary>
        /// ディスプレイ文字列は GUI で利用されます。Enum型とその分かりやすい単語の対応辞書です。
        /// </summary>
        private static readonly IReadOnlyDictionary<GmlType, string> displayDict = new Dictionary<GmlType, string>
        {
            { Building, "建築物" },
            { Transport, "道路" },
            { Vegetation, "植生" },
            { CityFurniture, "都市設備" },
            { DigitalElevationModel, "起伏(地形情報)" },
            { Etc, "その他" }
        };

        /// <summary>
        /// 地物タイプごとに、存在しうるLODの範囲が仕様書で定められています。その範囲の辞書です。
        /// 国交省仕様書 Ver2 の 5ページ目を参照してください。
        /// <see href="https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_doc_0001_ver02.pdf"/>
        /// </summary>
        private static readonly IReadOnlyDictionary<GmlType, ReadOnlyMinMax<int>> lodRangeDict = new Dictionary<GmlType, ReadOnlyMinMax<int>>
        {
            { Building, new ReadOnlyMinMax<int>(0,3) },
            { Transport, new ReadOnlyMinMax<int>(1,3) },
            { Vegetation, new ReadOnlyMinMax<int>(1,3) },
            { CityFurniture, new ReadOnlyMinMax<int>(1,3) },
            { DigitalElevationModel, new ReadOnlyMinMax<int>(1,3) },
            
            // 国交省仕様書 v2 の5ページによると、「その他」に属する 都市計画決定情報、土地利用、災害リスク のLODはすべて 1 です。
            // しかし実際のデータを見ると他のLODも含まれているため、Etc は全LODを想定します。
            { Etc, new ReadOnlyMinMax<int>(0,3) }
        };
        
        /// <summary>
        /// <see cref="GmlType"/> を接頭辞に変換する拡張メソッドです。
        /// 接頭辞はフォルダ名などに使われます。
        /// ただし、<see cref="GmlType.Etc"/> は接頭辞がないので例外を投げます。
        /// </summary>
        /// <param name="t">地物タイプ Enum型</param>
        /// <returns>地物タイプ string型(接頭辞)</returns>
        public static string ToPrefix(this GmlType t)
        {
            if (t == Etc)
            {
                throw new ArgumentException($"{nameof(Etc)} does not have prefix.");
            }

            if (prefixDict.TryGetValue(t, out string prefix))
            {
                return prefix;
            }

            throw new ArgumentOutOfRangeException($"{nameof(t)}", "Unknown type.");
        }

        /// <summary>
        /// 接頭辞から <see cref="GmlType"/> を返します。
        /// 辞書にない接頭辞の場合は <see cref="GmlType.Etc"/> を返します。
        /// </summary>
        /// <param name="prefix">地物タイプ string型(接頭辞)</param>
        /// <returns>地物タイプ Enum型</returns>
        public static GmlType ToEnum(string prefix)
        {
            foreach (var pair in prefixDict)
            {
                if (prefix == pair.Value)
                {
                    return pair.Key;
                }
            }

            return Etc;
        }

        /// <summary>
        /// <see cref="GmlType"/> を人間にとって分かりやすい文字に変換します。
        /// </summary>
        /// <param name="t">地物タイプ Enum型</param>
        /// <returns>説明形式の文字</returns>
        public static string ToDisplay(this GmlType t)
        {
            if (displayDict.TryGetValue(t, out string display))
            {
                return display;
            }

            throw new ArgumentOutOfRangeException($"{nameof(t)}", "Unknown Type.");
        }

        /// <summary>
        /// その地物タイプについて、仕様上存在しうるLODの範囲を返します。
        /// </summary>
        public static ReadOnlyMinMax<int> PossibleLodRange(this GmlType t)
        {
            return lodRangeDict[t];
        }

        /// <summary>
        /// key   が 各 <see cref="GmlType"/> であり、
        /// value が new T() である
        /// 辞書を構築して返します。
        /// </summary>
        public static Dictionary<GmlType, T> ComposeTypeDict<T>() where T : new()
        {
            return Enum.GetValues(typeof(GmlType))
                .OfType<GmlType>()
                .ToDictionary(t => t, _ => new T());
        }
        
        /// <summary>
        /// <see cref="ComposeTypeDict{T}()"/> の初期値を指定する版です。
        /// </summary>
        public static Dictionary<GmlType, T> ComposeTypeDict<T>(T initialVal)
        {
            return Enum.GetValues(typeof(GmlType))
                .OfType<GmlType>()
                .ToDictionary(t => t, _ => initialVal);
        }

    }
}