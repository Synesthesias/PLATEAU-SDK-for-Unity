using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using System;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    
    public static class MeshGranularityExtension
    {
        public static string ToJapaneseString(this ConvertGranularity granularity)
        {
            switch (granularity)
            {
                case GranularityConvert.ConvertGranularity.PerAtomicFeatureObject:
                    return "最小地物単位(壁面,屋根面等)";
                case GranularityConvert.ConvertGranularity.MaterialInPrimary:
                    return "主要地物内のマテリアルごと";
                case GranularityConvert.ConvertGranularity.PerPrimaryFeatureObject:
                    return "主要地物単位(建築物,道路等)";
                case GranularityConvert.ConvertGranularity.PerCityModelArea:
                    return "地域単位";
                default:
                    throw new ArgumentOutOfRangeException(nameof(granularity));
            }
        }

        /// <summary>
        /// MAGranularity を MeshGranularity に変換
        /// </summary>
        public static MeshGranularity ToMeshGranularity(this ConvertGranularity granularity)
        {
            return granularity switch
            {
                ConvertGranularity.PerCityModelArea => MeshGranularity.PerCityModelArea,
                ConvertGranularity.PerPrimaryFeatureObject => MeshGranularity.PerPrimaryFeatureObject,
                ConvertGranularity.MaterialInPrimary => MeshGranularity.PerAtomicFeatureObject,
                ConvertGranularity.PerAtomicFeatureObject => MeshGranularity.PerAtomicFeatureObject,
                _ => throw new Exception("unknown granularity.")
            };
        }

        /// <summary>
        /// <see cref="MeshGranularity"/>を<see cref="ConvertGranularity"/>に変換
        /// </summary>
        public static ConvertGranularity ToConvertGranularity(this MeshGranularity granularity)
        {
            return granularity switch
            {
                MeshGranularity.PerAtomicFeatureObject => ConvertGranularity.PerAtomicFeatureObject,
                MeshGranularity.PerPrimaryFeatureObject => ConvertGranularity.PerPrimaryFeatureObject,
                MeshGranularity.PerCityModelArea => ConvertGranularity.PerCityModelArea,
                _ => throw new Exception("unknown granularity.")
            };
        }
    }
}