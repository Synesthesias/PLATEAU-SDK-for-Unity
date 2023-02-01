using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.Native
{
    /// <summary>
    /// C++側の std::string を扱います。
    /// </summary>
    
    // TODO 今までは、DLL側から string を受け取るためにかなり面倒な手続きを踏んでいました。
    //      例えば、stringを返す関数ごとに、P/Invoke で文字アドレスと文字列長の両方を受け渡す P/Invoke関数をいちいち作っていました。
    //      そのような面倒な部分を NativeString に置き換えればコード簡略化できそうです。ただし寿命に要注意です。
    //      加えて string の配列となるとさらに複雑になっていますが、こちらも NativeVectorString に置き換えることでシンプルになりそうです。
    
    internal class NativeString 
    {
        public IntPtr Handle { get; }

        public NativeString(IntPtr handle)
        {
            Handle = handle;
        }

        public static NativeString Create()
        {
            var ptr = DLLUtil.PtrOfNewInstance(NativeMethods.plateau_create_string);
            return new NativeString(ptr);
        }

        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_string);
        }
        
        public override string ToString()
        {
            int strSize = DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_string_get_size);
            var charPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, NativeMethods.plateau_string_get_char_ptr);
            string str = DLLUtil.ReadUtf8Str(charPtr, strSize);
            return str;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_string(
                out IntPtr newStringPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_string(
                [In] IntPtr stringPtr);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_string_get_size(
                [In] IntPtr nativeStringPtr,
                out int outSize);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_string_get_char_ptr(
                [In] IntPtr nativeStringPtr,
                out IntPtr outCharPtr);
        }
    }
}
