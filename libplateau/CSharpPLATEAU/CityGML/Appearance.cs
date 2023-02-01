using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{

    /// <summary>
    /// <see cref="Texture"/> , <see cref="Material"/> の基底クラスです。
    /// </summary>
    public class Appearance : Object
    {
        internal Appearance(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// Texture なら "Texture" という文字列を返します。
        /// </summary>
        public string Type =>
            DLLUtil.GetNativeString(Handle,
                NativeMethods.plateau_appearance_get_type);

        public bool IsFront =>
            DLLUtil.GetNativeValue<bool>(Handle,
                NativeMethods.plateau_appearance_get_is_front);

        /// <summary>
        /// テーマ名を配列で返します。
        /// </summary>
        public string[] Themes =>
            DLLUtil.GetNativeStringArrayByPtr(
                Handle,
                NativeMethods.plateau_appearance_get_themes_count,
                NativeMethods.plateau_appearance_get_themes);

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_get_type(
                [In] IntPtr handle,
                out IntPtr strPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_get_is_front(
                [In] IntPtr handle,
                [MarshalAs(UnmanagedType.U1)] out bool outIsFront);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_get_themes_count(
                [In] IntPtr handle,
                out int count);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_get_themes(
                [In] IntPtr handle,
                [In, Out] IntPtr[] outStrPointers,
                [Out] int[] outStrSizes);
        }
    }
}
