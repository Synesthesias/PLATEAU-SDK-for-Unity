using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.PolygonMesh
{
    /// <summary>
    /// <see cref="CityModel"/> から<see cref="Model"/>を抽出します。
    /// </summary>
    public static class TileExtractor
    {


        /// <summary>
        /// <see cref="CityModel"/> からタイル分割された <see cref="Model"/> を抽出します。
        /// Grid分割する場合は、<paramref name="options"/> の <see cref="MeshExtractOptions.GridCountOfSide"/> を2以上に 
        /// <see cref="MeshExtractOptions.HighestLodOnly"/> をtrueに設定してください。
        /// 結果は <paramref name="outModel"/> に格納されます。
        /// 通常、<paramref name="outModel"/> には new したばかりの Model を渡してください。
        /// </summary>
        public static void ExtractWithGrid(ref Model outModel, CityModel cityModel, MeshExtractOptions options, List<Extent> extents)
        {
            var nativeExtents = NativeVectorExtent.Create();
            foreach (var extent in extents)
            {
                nativeExtents.Add(extent);
            }

            var result = NativeMethods.plateau_tile_extractor_extract_with_grid(
                cityModel.Handle, options, nativeExtents.Handle, outModel.Handle
            );
            nativeExtents.Dispose();
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 複数の<see cref="CityModel"/> のリストから、範囲内の <see cref="Model"/> を抽出します。
        /// </summary>
        /// <param name="outModel"></param>
        /// <param name="cityModels"></param>
        /// <param name="options"></param>
        /// <param name="extents"></param>
        public static void ExtractWithCombine(ref Model outModel, List<CityModel> cityModels, MeshExtractOptions options, List<Extent> extents)
        {
            var nativeExtents = NativeVectorExtent.Create();
            foreach (var extent in extents)
            {
                nativeExtents.Add(extent);
            }

            int cityModelCount = cityModels.Count;
            IntPtr[] nativePtrs = cityModels.Select(model => model.Handle).ToArray();
            var result = NativeMethods.plateau_tile_extractor_extract_with_combine(
                nativePtrs, cityModelCount, options, nativeExtents.Handle, outModel.Handle
            );
            nativeExtents.Dispose();
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_tile_extractor_extract_with_grid(
                [In] IntPtr cityModelPtr,
                MeshExtractOptions options,
                [In] IntPtr extentsPtr,
                [In] IntPtr outModelPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_tile_extractor_extract_with_combine(
            [In] IntPtr[] cityModelPtrs,
            [In] int cityModelCount,
            MeshExtractOptions options,
            [In] IntPtr extentsPtr,
            [In] IntPtr outModelPtr);
        }
    }
}
