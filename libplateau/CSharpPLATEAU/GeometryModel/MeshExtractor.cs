using System;
using System.Threading;
using PLATEAU.CityGML;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
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
    }
}