using PLATEAU.PolygonMesh;
using System;

namespace PLATEAU.CityAdjust.MaterialAdjust.Executor.Process
{
    /// <summary>
    /// マテリアル分け用のメッシュ粒度の選択肢です。
    /// インポート用の粒度選択肢とは異なります。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    public enum MAGranularity
    {
        /// <summary>
        /// 結合粒度を変化させない
        /// </summary>
        DoNotChange,
        /// <summary>
        /// 最小地物単位(LOD2, LOD3の各部品)
        /// </summary>
        PerAtomicFeatureObject,
        /// <summary>
        /// 主要地物単位(建築物、道路等)
        /// </summary>
        PerPrimaryFeatureObject,
        /// <summary>
        /// すべて1つに結合
        /// </summary>
        CombineAll
    }
    
    public static class MAMeshGranularityExtension
    {
        public static string ToJapaneseString(this MAGranularity granularity)
        {
            switch (granularity)
            {
                case MAGranularity.DoNotChange:
                    return "変更しない";
                case MAGranularity.PerAtomicFeatureObject:
                    return "最小地物単位(壁面,屋根面等)";
                case MAGranularity.PerPrimaryFeatureObject:
                    return "主要地物単位(建築物,道路等)";
                case MAGranularity.CombineAll:
                    return "すべて1つに結合";
                default:
                    throw new ArgumentOutOfRangeException(nameof(granularity));
            }
        }

        /// <summary>
        /// MAGranularity を MeshGranularity に変換
        /// </summary>
        public static MeshGranularity ToNativeGranularity(this MAGranularity granularity)
        {
            return granularity switch
            {
                MAGranularity.CombineAll => MeshGranularity.PerCityModelArea,
                MAGranularity.PerPrimaryFeatureObject => MeshGranularity.PerPrimaryFeatureObject,
                MAGranularity.PerAtomicFeatureObject => MeshGranularity.PerAtomicFeatureObject,
                MAGranularity.DoNotChange => throw new NotImplementedException("未実装"), // TODO
                _ => throw new Exception("unknown granularity.")
            };
        }

        /// <summary>
        /// <see cref="MeshGranularity"/>を<see cref="MAGranularity"/>に変換
        /// </summary>
        public static MAGranularity ToMAGranularity(this MeshGranularity granularity)
        {
            return granularity switch
            {
                MeshGranularity.PerAtomicFeatureObject => MAGranularity.PerAtomicFeatureObject,
                MeshGranularity.PerPrimaryFeatureObject => MAGranularity.PerPrimaryFeatureObject,
                MeshGranularity.PerCityModelArea => MAGranularity.CombineAll,
                _ => throw new Exception("unknown granularity.")
            };
        }
    }
}