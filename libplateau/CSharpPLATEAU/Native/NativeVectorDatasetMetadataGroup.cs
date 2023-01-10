using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    public class NativeVectorDatasetMetadataGroup : NativeVectorDisposableBase<DatasetMetadataGroup>
    {
        private NativeVectorDatasetMetadataGroup(IntPtr handle) : base(handle)
        {
        }

        public static NativeVectorDatasetMetadataGroup Create()
        {
            return new NativeVectorDatasetMetadataGroup(
                DLLUtil.PtrOfNewInstance(
                    NativeMethods.plateau_create_vector_dataset_metadata_group
                )
            );
        }

        protected override void DisposeNative()
        {
            ThrowIfDisposed();
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_vector_dataset_metadata_group);
        }

        public override DatasetMetadataGroup At(int index)
        {
            ThrowIfDisposed();
            var elementPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_dataset_metadata_group_get_pointer);
            return new DatasetMetadataGroup(elementPtr);
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_dataset_metadata_group_count);
                return count;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_dataset_metadata_group(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_dataset_metadata_group(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_dataset_metadata_group_get_pointer(
                [In] IntPtr vectorPtr,
                out IntPtr outGmlFilePtr,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_dataset_metadata_group_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
