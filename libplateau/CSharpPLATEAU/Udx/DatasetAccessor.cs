﻿using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
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


        public NativeVectorGmlFile GetGmlFiles(Extent extent, PredefinedCityModelPackage package)
        {
            var gmlFiles = NativeVectorGmlFile.Create();
            var result = NativeMethods.plateau_i_dataset_accessor_get_gml_files(
                Handle, extent, package, gmlFiles.Handle);
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

        public PredefinedCityModelPackage Packages =>
            DLLUtil.GetNativeValue<PredefinedCityModelPackage>(Handle,
                NativeMethods.plateau_i_dataset_accessor_get_packages);
    }
}
