using System;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// GMLファイル群から利用可能なファイル、メッシュコード、LODを検索します。
    /// C++側では、IDatasetAccessorは LocalDatasetAccessor と ServerDatasetAccessor の
    /// 基底クラスになっています。
    /// このクラスは IDatasetAccessor のポインタを保持します。
    /// </summary>
    public class DatasetAccessor
    {
        public IntPtr Handle { get; private set; }
        public DatasetAccessor(IntPtr handle)
        {
            Handle = handle;
        }


        public NativeVectorGmlFile GetGmlFiles(PredefinedCityModelPackage package)
        {
            var gmlFiles = NativeVectorGmlFile.Create();
            var result = NativeMethods.plateau_i_dataset_accessor_get_gml_files(
                Handle, package, gmlFiles.Handle);
            DLLUtil.CheckDllError(result);
            return gmlFiles;
        }

        public NativeVectorMeshCode MeshCodes
        {
            get
            {
                var meshCodes = NativeVectorMeshCode.Create();
                var result = NativeMethods.plateau_i_dataset_accessor_get_mesh_codes(
                    Handle, meshCodes.Handle);
                DLLUtil.CheckDllError(result);
                return meshCodes;
            }
        }

        public int GetMaxLod(MeshCode meshCode, PredefinedCityModelPackage package)
        {
            var result = NativeMethods.plateau_i_dataset_accessor_get_max_lod(
                Handle, out int maxLod, meshCode, package);
            DLLUtil.CheckDllError(result);
            return maxLod;
        }

        /// <summary>
        /// <see cref="LocalDatasetAccessor"/> の場合:
        /// 存在するパッケージ種をフラグ形式で返します。
        /// 
        /// <see cref="ServerDatasetAccessor"/> の場合:
        /// <see cref="GetGmlFiles"/> したことのある <see cref="Extent"/> に関して、
        /// 存在するパッケージ種をフラグ形式で返します。
        /// <see cref="GetGmlFiles"/> を実行した後でないと None が返ります。
        /// </summary>
        public PredefinedCityModelPackage Packages =>
            DLLUtil.GetNativeValue<PredefinedCityModelPackage>(Handle,
                NativeMethods.plateau_i_dataset_accessor_get_packages);

        /// <summary>
        /// gmlのパスが "udx/(featureType)/aaa.gml" として、
        /// (featureType) の部分を <see cref="PredefinedCityModelPackage"/> に変換します。
        /// </summary>
        public static PredefinedCityModelPackage FeatureTypeToPackage(string featureType)
        {
            var featureTypeUtf8 = DLLUtil.StrToUtf8Bytes(featureType);
            var result = NativeMethods.plateau_udx_sub_folder_feature_type_to_package(
                featureTypeUtf8, out var package);
            DLLUtil.CheckDllError(result);
            return package;
        }
    }
}
