using System;
using PLATEAU.Dataset;

namespace PLATEAU.Interop
{
    public class NativeVectorDatasetMetadataGroup : NativeVectorBase<DatasetMetadataGroup>
    {
        public NativeVectorDatasetMetadataGroup(IntPtr handle) : base(handle)
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
    }
}
