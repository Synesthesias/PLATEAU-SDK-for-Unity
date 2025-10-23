using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.PolygonMesh
{
    /// <summary>
    /// <see cref="CityModel"/> から<see cref="Model"/>を抽出します。
    /// </summary>
    public static class MeshExtractor
    {

        /// <summary>
        /// <see cref="CityModel"/> から <see cref="Model"/> を抽出します。
        /// 結果は <paramref name="outModel"/> に格納されます。
        /// 通常、<paramref name="outModel"/> には new したばかりの Model を渡してください。
        /// </summary>
        public static void Extract(ref Model outModel, CityModel cityModel, MeshExtractOptions options)
        {
            var result = NativeMethods.plateau_mesh_extractor_extract(
                cityModel.Handle, options, outModel.Handle
            );
            DLLUtil.CheckDllError(result);
        }


        /// <summary>
        /// <see cref="CityModel"/> から範囲内の <see cref="Model"/> を抽出します。
        /// 結果は <paramref name="outModel"/> に格納されます。
        /// 通常、<paramref name="outModel"/> には new したばかりの Model を渡してください。
        /// </summary>
        public static void ExtractInExtents(ref Model outModel, CityModel cityModel, MeshExtractOptions options, List<Extent> extents,
            LogCallbacks logCallbacks = null, DllLogLevel logLevel = DllLogLevel.Error)
        {
            if (logCallbacks == null)
            {
                logCallbacks = LogCallbacks.StdOut;
            }
            var nativeExtents = NativeVectorExtent.Create();
            foreach (var extent in extents)
            {
                nativeExtents.Add(extent);
            }

            var result = NativeMethods.plateau_mesh_extractor_extract_in_extents_with_log(
                cityModel.Handle, options, nativeExtents.Handle, outModel.Handle, logLevel,
                logCallbacks.LogErrorFuncPtr, logCallbacks.LogWarnFuncPtr, logCallbacks.LogInfoFuncPtr
            );
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_extractor_extract(
                [In] IntPtr cityModelPtr,
                MeshExtractOptions options,
                [In] IntPtr outModelPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_extractor_extract_in_extents(
                [In] IntPtr cityModelPtr,
                MeshExtractOptions options,
                [In] IntPtr extentsPtr,
                [In] IntPtr outModelPtr);
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_extractor_extract_in_extents_with_log(
                [In] IntPtr cityModelPtr,
                MeshExtractOptions options,
                [In] IntPtr extentsPtr,
                [In] IntPtr outModelPtr,
                DllLogLevel logLevel,
                [In] IntPtr logErrorCallback,
                [In] IntPtr logWarnCallback,
                [In] IntPtr logInfoCallback
                );
        }
    }
}
