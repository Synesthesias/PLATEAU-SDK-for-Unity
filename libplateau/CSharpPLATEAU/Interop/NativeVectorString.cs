using System;
using System.Linq;
using PLATEAU.Dataset;

namespace PLATEAU.Interop
{
    /// <summary>
    /// C++側の vector{string} を扱います。
    /// </summary>
    public class NativeVectorString : NativeVectorDisposableBase<NativeString>
    {
        public NativeVectorString(IntPtr handle) : base(handle)
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
    }
}
