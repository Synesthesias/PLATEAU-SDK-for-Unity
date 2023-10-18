using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;

namespace PLATEAU.GranularityConvert
{
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
        public MeshGranularity Granularity;
        public int GridCount; // 地域単位でのみ利用


        public GranularityConvertOption(MeshGranularity granularity, int gridCount)
        {
            if (gridCount < 0) throw new ArgumentOutOfRangeException();
            this.Granularity = granularity;
            this.GridCount = gridCount;
        }
    }
}
