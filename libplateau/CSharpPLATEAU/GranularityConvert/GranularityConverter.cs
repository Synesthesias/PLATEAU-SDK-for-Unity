using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// 分割結合で使う粒度設定です。
    /// </summary>
    public enum ConvertGranularity
    {
        /// <summary>
        /// 最小地物単位(LOD2, LOD3の各部品)
        /// </summary>
        PerAtomicFeatureObject,
        /// <summary>
        /// 主要地物単位(建築物、道路等)
        /// </summary>
        PerPrimaryFeatureObject,
        /// <summary>
        /// 都市モデル地域単位(GMLファイル内のすべてを結合)
        /// </summary>
        PerCityModelArea,
        
        /// <summary>
        /// 主要地物内のマテリアルごと
        /// </summary>
        MaterialInPrimary
    }
    
    
    public class GranularityConverter
    {
        /// <summary>
        /// srcModelの粒度を変換したModelを新しく作って返します。
        /// </summary>
        public Model Convert(Model srcModel, GranularityConvertOption option)
        {
            var apiResult = NativeMethods.plateau_granularity_converter_convert(
                srcModel.Handle, out IntPtr outModelPtr, option);
            DLLUtil.CheckDllError(apiResult);
            return new Model(outModelPtr);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_granularity_converter_convert(
                [In] IntPtr srcModelPtr,
                out IntPtr outModelPtr,
                [In] GranularityConvertOption option
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GranularityConvertOption
    {
        public ConvertGranularity Granularity;
        public int GridCount; // 地域単位でのみ利用


        public GranularityConvertOption(ConvertGranularity granularity, int gridCount)
        {
            if (gridCount < 0) throw new ArgumentOutOfRangeException();
            this.Granularity = granularity;
            this.GridCount = gridCount;
        }
        
        /// <summary>
        /// 後方互換性のために古い型の引数のコンストラクタを残しておきます。
        /// </summary>
        [Obsolete("Please use ConvertGranularity instead of MeshGranularity.")]
        public GranularityConvertOption(MeshGranularity meshGranularity, int gridCount) : this(
            meshGranularity.ToConvertGranularity(), gridCount)
        { }
    }
}
