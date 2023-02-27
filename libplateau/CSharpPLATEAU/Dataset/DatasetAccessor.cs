using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// GMLファイル群から利用可能なファイル、メッシュコード、LODを検索します。
    /// C++の内部ではこれは基底クラスとなっており、継承によりローカル向けとサーバー向けの両方に対応しています。
    /// このクラスのポインタ (Handle) の具体的な型がローカル向けとサーバー向けのどちらであるかは、
    /// <see cref="DatasetSource"/> の初期化時に指定し、
    /// <see cref="DatasetSource.Accessor"/> でその型である DatasetAccessor を取得します。 
    /// </summary>
    public class DatasetAccessor : PInvokeDisposable
    {
        /// <summary> handle は C++側の基底クラス (IDatasetAccessor) のポインタです。 </summary>
        public DatasetAccessor(IntPtr handle) : base(handle)
        {
        }

        public NativeVectorGmlFile GetGmlFilesForPackage(PredefinedCityModelPackage package)
        {
            var gmlFiles = NativeVectorGmlFile.Create();
            var result = NativeMethods.plateau_i_dataset_accessor_get_gml_files(
                Handle, package, gmlFiles.Handle);
            DLLUtil.CheckDllError(result);
            return gmlFiles;
        }

        public NativeVectorGmlFile GetGmlFiles()
        {
            // GetGmlFilesForPackages を各パッケージごとに呼びだすことで全GmlFileを取得します。
            var packages = Enum.GetValues(typeof(PredefinedCityModelPackage)).OfType<PredefinedCityModelPackage>();
            var ret = NativeVectorGmlFile.Create();
            foreach (var package in packages)
            {
                if (package == PredefinedCityModelPackage.None) continue;
                using (var gmlFiles = GetGmlFilesForPackage(package))
                {
                    ret.AddCopyOf(gmlFiles);
                }
            }

            return ret;
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

        /// <summary>
        /// ローカルの場合:
        /// 存在するパッケージ種をフラグ形式で返します。
        /// 
        /// サーバーの場合:
        /// <see cref="GetGmlFilesForPackage"/> したことのある <see cref="Extent"/> に関して、
        /// 存在するパッケージ種をフラグ形式で返します。
        /// <see cref="GetGmlFilesForPackage"/> を実行した後でないと None が返ります。
        /// </summary>
        public PredefinedCityModelPackage Packages =>
            DLLUtil.GetNativeValue<PredefinedCityModelPackage>(Handle,
                NativeMethods.plateau_i_dataset_accessor_get_packages);

        public PlateauVector3d CalculateCenterPoint(GeoReference geoReference)
        {
            var result = NativeMethods.plateau_i_dataset_accessor_calculate_center_point(
                Handle, geoReference.Handle, out var centerPoint);
            DLLUtil.CheckDllError(result);
            return centerPoint;
        }

        public DatasetAccessor FilterByMeshCodes(IEnumerable<MeshCode> meshCodes)
        {
            var nativeMeshCodes = NativeVectorMeshCode.Create();
            foreach (var meshCode in meshCodes)
            {
                nativeMeshCodes.Add(meshCode);
            }

            var result = NativeMethods.plateau_i_dataset_accessor_filter_by_mesh_codes(
                Handle, nativeMeshCodes.Handle, out var filteredPtr);
            DLLUtil.CheckDllError(result);
            nativeMeshCodes.Dispose();
            return new DatasetAccessor(filteredPtr);
        }

        /// <summary>
        /// gmlのパスが "udx/(featureType)/aaa.gml" として、
        /// (featureType) の部分を <see cref="PredefinedCityModelPackage"/> に変換します。
        /// </summary>
        public static PredefinedCityModelPackage FeatureTypeToPackage(string featureType)
        {
            var featureTypeUtf8 = DLLUtil.StrToUtf8Bytes(featureType);
            var result = GmlFile.NativeMethods.plateau_udx_sub_folder_feature_type_to_package(
                featureTypeUtf8, out var package);
            DLLUtil.CheckDllError(result);
            return package;
        }

        protected override void DisposeNative()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_i_dataset_accessor);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_i_dataset_accessor(
                [In] IntPtr accessorPtr);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_i_dataset_accessor_get_gml_files(
                [In] IntPtr accessorPtr,
                PredefinedCityModelPackage package,
                [In] IntPtr refVectorGmlFilePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_i_dataset_accessor_get_mesh_codes(
                [In] IntPtr accessorPtr,
                [In,Out] IntPtr refVectorMeshCodePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_i_dataset_accessor_get_packages(
                [In] IntPtr accessorPtr,
                out PredefinedCityModelPackage outPackageFlags);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_i_dataset_accessor_calculate_center_point(
                [In] IntPtr accessorPtr,
                [In] IntPtr geoReferencePtr,
                out PlateauVector3d outCenterPoint);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_i_dataset_accessor_filter_by_mesh_codes(
                [In] IntPtr accessorPtr,
                [In] IntPtr nativeVectorMeshCodePtr,
                out IntPtr outFilteredAccessorPtr);
        }
    }
}
