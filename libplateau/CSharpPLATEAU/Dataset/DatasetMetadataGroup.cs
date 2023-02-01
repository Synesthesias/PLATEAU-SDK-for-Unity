using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    public class DatasetMetadataGroup
    {
        private IntPtr Handle { get; }
        public DatasetMetadataGroup(IntPtr ptr)
        {
            Handle = ptr;
        }
        
        public static DatasetMetadataGroup Create()
        {
            var result = NativeMethods.plateau_create_dataset_metadata_group(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            return new DatasetMetadataGroup(outPtr);
        }

        public string ID => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_group_get_id);

        public string Title => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_group_get_title);

        public NativeVectorDatasetMetadata Datasets
        {
            get
            {
                var result = NativeMethods.plateau_dataset_metadata_group_get_datasets(Handle, out var vectorPtr);
                DLLUtil.CheckDllError(result);
                return new NativeVectorDatasetMetadata(vectorPtr);
            }
        }

        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_dataset_metadata_group);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_dataset_metadata_group(
                out IntPtr outDatasetMetadataGroupPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_dataset_metadata_group(
                [In] IntPtr datasetMetadataGroupPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_group_get_id(
                [In] IntPtr handle,
                out IntPtr outStrPtr,
                out int strLength);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_group_get_title(
                [In] IntPtr handle,
                out IntPtr outStrPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_group_get_datasets(
                [In] IntPtr handle,
                out IntPtr nativeVectorDatasetMetadataPtr);
        }
    }
}
