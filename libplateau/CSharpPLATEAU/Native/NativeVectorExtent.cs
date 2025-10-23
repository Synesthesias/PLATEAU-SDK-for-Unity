using System;
using System.Runtime.InteropServices;
using PLATEAU.Dataset;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    public class NativeVectorExtent : NativeVectorDisposableBase<Extent>
    {
        private NativeVectorExtent(IntPtr ptr) : base(ptr)
        {
        }

        public static NativeVectorExtent Create()
        {
            var result = NativeMethods.plateau_create_vector_extent(out var ptr);
            DLLUtil.CheckDllError(result);
            return new NativeVectorExtent(ptr);
        }

        public override Extent At(int index)
        {
            ThrowIfDisposed();
            var meshCode = DLLUtil.GetNativeValue<Extent>(Handle, index,
                NativeMethods.plateau_vector_extent_get_value);
            return meshCode;
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_extent_count);
                return count;
            }
        }

        public void Add(Extent extent)
        {
            var result = NativeMethods.plateau_vector_extent_push_back_value(
                Handle, extent);
            DLLUtil.CheckDllError(result);
        }


        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_delete_vector_extent(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_extent(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_extent(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_extent_get_value(
                [In] IntPtr vectorPtr,
                out Extent outExtent,
                int index);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_extent_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_extent_push_back_value(
                [In] IntPtr handle,
                [In] Extent extent);
        }
    }
}
