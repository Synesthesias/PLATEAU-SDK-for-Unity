using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;
using PLATEAU.Interop;

namespace PLATEAU.Native
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

        public override DatasetMetadata At(int index)
        {
            var elementPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_dataset_metadata_get_pointer);
            return new DatasetMetadata(elementPtr);
        }

        public override int Length
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_dataset_metadata_count);
                return count;
            }
        }

        /// <summary>
        /// 取扱注意:
        /// 通常は <see cref="NativeVectorDatasetMetadataGroup"/> の廃棄時に廃棄されるので呼ぶ必要はありません。
        /// <see cref="NativeVectorDatasetMetadataGroup"/> を介さないインスタンスのみ廃棄してください。
        /// </summary>
        public void Dispose()
        {
            var result = NativeMethods.plateau_delete_vector_dataset_metadata(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_dataset_metadata(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_dataset_metadata(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_dataset_metadata_get_pointer(
                [In] IntPtr vectorPtr,
                out IntPtr outDatasetMetadataPtr,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_dataset_metadata_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
