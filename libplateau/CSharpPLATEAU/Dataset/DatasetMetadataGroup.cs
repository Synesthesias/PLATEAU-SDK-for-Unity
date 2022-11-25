using System;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    public class DatasetMetadataGroup : PInvokeDisposable
    {
        public DatasetMetadataGroup(IntPtr ptr) : base(ptr)
        {
            
        }
        
        public static DatasetMetadataGroup Create()
        {
            var result = NativeMethods.plateau_create_dataset_metadata_group(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            return new DatasetMetadataGroup(outPtr);
        }

        public string ID
        {
            get
            {
                ThrowIfDisposed();
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_group_get_id);
            }
        }

        public string Title
        {
            get
            {
                ThrowIfDisposed();
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_group_get_title);
            }
        }

        public NativeVectorDatasetMetadata Datasets
        {
            get
            {
                var result = NativeMethods.plateau_dataset_metadata_group_get_datasets(Handle, out var vectorPtr);
                DLLUtil.CheckDllError(result);
                return new NativeVectorDatasetMetadata(vectorPtr);
            }
        }

        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_dataset_metadata_group(Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}
