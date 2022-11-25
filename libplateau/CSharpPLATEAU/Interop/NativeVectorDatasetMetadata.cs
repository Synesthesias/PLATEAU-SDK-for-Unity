using System;
using PLATEAU.Dataset;

namespace PLATEAU.Interop
{
    public class NativeVectorDatasetMetadata : NativeVectorBase<DatasetMetadata>
    {
        public NativeVectorDatasetMetadata(IntPtr handle) : base(handle)
        {
        }

        public static NativeVectorDatasetMetadata Create()
        {
            return new NativeVectorDatasetMetadata(
                DLLUtil.PtrOfNewInstance(
                    NativeMethods.plateau_create_vector_dataset_metadata
                )
            );
        }

        protected override void DisposeNative()
        {
            ThrowIfDisposed();
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_vector_dataset_metadata);
        }

        public override DatasetMetadata At(int index)
        {
            ThrowIfDisposed();
            var elementPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_dataset_metadata_get_pointer);
            return new DatasetMetadata(elementPtr);
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_dataset_metadata_count);
                return count;
            }
        }
    }
}
