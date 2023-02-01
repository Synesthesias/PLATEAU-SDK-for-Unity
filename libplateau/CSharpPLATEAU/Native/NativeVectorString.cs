using System;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    /// <summary>
    /// C++側の vector{string} を扱います。
    /// </summary>
    internal class NativeVectorString : NativeVectorDisposableBase<NativeString>
    {
        private NativeVectorString(IntPtr handle) : base(handle)
        {
        }

        public static NativeVectorString Create()
        {
            return new NativeVectorString(
                DLLUtil.PtrOfNewInstance(
                    NativeMethods.plateau_create_vector_string
                )
            );
        }

        protected override void DisposeNative()
        {
            ThrowIfDisposed();
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_vector_string);
        }

        public override NativeString At(int index)
        {
            ThrowIfDisposed();
            var elementPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_vector_string_get_pointer);
            var nativeStr = new NativeString(elementPtr);
            return nativeStr;
        }

        public override int Length
        {
            get
            {
                ThrowIfDisposed();
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_vector_string_count);
                return count;
            }
        }

        public string[] ToCSharpArray()
        {
            return this.Select(nativeStr => nativeStr.ToString()).ToArray();
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_vector_string(
                out IntPtr outVectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_vector_string(
                [In] IntPtr vectorPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_string_get_pointer(
                [In] IntPtr vectorPtr,
                out IntPtr outStringPtr,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_vector_string_count(
                [In] IntPtr handle,
                out int outCount);
        }
    }
}
