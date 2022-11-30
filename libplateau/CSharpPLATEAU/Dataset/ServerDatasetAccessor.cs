using System;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    public class ServerDatasetAccessor
    {
        public IntPtr Handle { get; }
        public ServerDatasetAccessor(IntPtr ptr)
        {
            Handle = ptr;
        }

        public static ServerDatasetAccessor Create(string datasetId)
        {
            var result = NativeMethods.plateau_create_server_dataset_accessor(out var newPtr, datasetId);
            DLLUtil.CheckDllError(result);
            return new ServerDatasetAccessor(newPtr);
        }

        public NativeVectorDatasetMetadataGroup GetMetadataGroup()
        {
            var ret = NativeVectorDatasetMetadataGroup.Create();
            var result = NativeMethods.plateau_server_dataset_accessor_get_dataset_metadata_group(
                Handle, ret.Handle);
            DLLUtil.CheckDllError(result);
            return ret;
        }

        public void SetDatasetID(string datasetID)
        {
            var result = NativeMethods.plateau_server_dataset_accessor_set_dataset_id(Handle, datasetID);
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 実行前に SetDatasetID が実行されていることが前提です。
        /// </summary>
        public NativeVectorMeshCode MeshCodes
        {
            get
            {
                var meshCodes = NativeVectorMeshCode.Create();
                var result = NativeMethods.plateau_server_dataset_accessor_get_mesh_codes(Handle, meshCodes.Handle);
                DLLUtil.CheckDllError(result);
                return meshCodes;
            }
        }

        public NativeVectorGmlFile GetGmlFiles(Extent extent, PredefinedCityModelPackage package)
        {
            var gmlFiles = NativeVectorGmlFile.Create();
            var result = NativeMethods.plateau_server_dataset_accessor_get_gml_files(
                Handle, gmlFiles.Handle, extent, package);
            DLLUtil.CheckDllError(result);
            return gmlFiles;
        }

        public PredefinedCityModelPackage Packages
        {
            get
            {
                return DLLUtil.GetNativeValue<PredefinedCityModelPackage>(Handle,
                    NativeMethods.plateau_server_dataset_accessor_get_packages);
            }
        }

        public void Dispose()
        {
            // TODO ExecNativeVoidFunc を適用できる箇所は他にもあるので置き換える
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_server_dataset_accessor);
        }
    }
}
