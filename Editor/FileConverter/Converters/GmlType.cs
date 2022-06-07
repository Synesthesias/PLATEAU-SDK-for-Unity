using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Codice.Client.Common;
using static PlateauUnitySDK.Editor.FileConverter.Converters.GmlType;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    public enum GmlType
    {
        Building,
        Transport,
        Vegetation,
        CityFurniture,
        DigitalElevationModel,
        Etc
    }

    public static class GmlTypeConvert
    {
        private static readonly Dictionary<GmlType, string> prefixDict = new Dictionary<GmlType, string>()
        {
            { Building, "bldg" },
            { Transport, "trans" },
            { Vegetation, "veg" },
            { CityFurniture, "frn" },
            { DigitalElevationModel, "dem" }
        };

        private static readonly Dictionary<GmlType, string> displayDict = new Dictionary<GmlType, string>()
        {
            { Building, "建築物" },
            { Transport, "道路" },
            { Vegetation, "植生" },
            { CityFurniture, "都市設備" },
            { DigitalElevationModel, "起伏(地形情報)" },
            { Etc, "その他" }
        };
        
        /// <summary>
        /// <see cref="GmlType"/> を接頭辞に変換します。
        /// 接頭辞はフォルダ名などに使われます。
        /// ただし、<see cref="GmlType.Etc"/> は接頭辞がないので例外を投げます。
        /// </summary>
        public static string ToPrefix(GmlType t)
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
        public static GmlType FromPrefix(string prefix)
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
        public static string ToDisplay(GmlType t)
        {
            if (displayDict.TryGetValue(t, out string display))
            {
                return display;
            }

            throw new ArgumentOutOfRangeException($"{nameof(t)}", "Unknown Type.");
        }

    }
}