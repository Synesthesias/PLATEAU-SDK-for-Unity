using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// メッシュコードと国土基本図の図郭のC++上の親クラスです。
    /// </summary>
    public class GridCode : PInvokeDisposable
    {
        private GridCode(IntPtr handle, bool autoDispose = true) : base(handle, autoDispose)
        {
        }

        /// <summary>
        /// コード文字列からグリッドコードを生成します。
        /// コードが不正である場合は例外がスローされます。
        /// </summary>
        public static GridCode Create(string code, bool autoDispose = true)
        {
            var result = NativeMethods.plateau_grid_code_parse(code, out var gridCodePtr);
            DLLUtil.CheckDllError(result);
            return new GridCode(gridCodePtr, autoDispose);
        }
        
        public static GridCode CopyFrom(IntPtr otherGridCodePtr)
        {
            // C++で現にあるインスタンスのアドレスをC#と紐付ます
            var other = new GridCode(otherGridCodePtr, false);
            // コピーします
            return Create(other.StringCode);
        }
        
        public Extent Extent
        {
            get
            {
                ThrowIfInvalid();
                Extent value = new Extent();
                APIResult result = NativeMethods.plateau_grid_code_get_extent(Handle, ref value);
                DLLUtil.CheckDllError(result);
                return value;
            }
        }

        public string StringCode
        {
            get
            {
                ThrowIfInvalid();
                return DLLUtil.GetNativeStringByValue(
                    Handle,
                    NativeMethods.plateau_grid_code_get_string_code_size,
                    NativeMethods.plateau_grid_code_get_string_code
                );
            }
        }

        public override string ToString()
        {
            return StringCode;
        }
        
        public bool IsLargestLevel
        {
            get
            {
                var result = NativeMethods.plateau_grid_code_is_largest_level(Handle, out bool isLargestLevel);
                DLLUtil.CheckDllError(result);
                return isLargestLevel;
            }
        }
        
        public bool IsSmallerThanNormalGml
        {
            get
            {
                var result = NativeMethods.plateau_grid_code_is_smaller_than_normal_gml(Handle, out bool isSmallerThanNormalGml);
                DLLUtil.CheckDllError(result);
                return isSmallerThanNormalGml;
            }
        }
        
        public bool IsNormalGmlLevel
        {
            get
            {
                var result = NativeMethods.plateau_grid_code_is_normal_gml_level(Handle, out bool isNormalGmlLevel);
                DLLUtil.CheckDllError(result);
                return isNormalGmlLevel;
            }
        }
        
        /// <summary>
        /// 1段階上のレベルのグリッドコードに変換します。
        /// </summary>
        /// <returns>1段階上のレベルのグリッドコードオブジェクト</returns>
        public GridCode Upper()
        {
            var result = NativeMethods.plateau_grid_code_upper(Handle, out var upperGridCodePtr);
            DLLUtil.CheckDllError(result);
            return new GridCode(upperGridCodePtr);
        }
        
        public bool IsValid
        {
            get
            {
                var result = NativeMethods.plateau_grid_code_is_valid(Handle, out bool resultIsValid);
                DLLUtil.CheckDllError(result);
                return resultIsValid;
            }
        }
        
        private void ThrowIfInvalid()
        {
            if (IsValid) return;
            throw new Exception("Invalid GridCode: " + StringCode);
        }

        protected override void DisposeNative()
        {
            var result = NativeMethods.plateau_grid_code_delete(Handle);
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_parse(
                [In] string code,
                out IntPtr gridCodePtr
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_get_extent(
                [In] IntPtr gridCode,
                [In, Out] ref Extent outExtent);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_delete(
                [In] IntPtr gridCodePtr
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_is_valid(
                [In] IntPtr gridCodePtr,
                [MarshalAs(UnmanagedType.U1)] out bool outIsValid);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_get_string_code_size(
                [In] IntPtr gridCodePtr,
                out int strSize
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_is_largest_level(
                [In] IntPtr gridCodePtr,
                [MarshalAs(UnmanagedType.U1)] out bool outIsLargestLevel
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_is_smaller_than_normal_gml(
                [In] IntPtr gridCodePtr,
                [MarshalAs(UnmanagedType.U1)] out bool isSmallerThanNormalGml
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_is_normal_gml_level(
                [In] IntPtr gridCodePtr,
                [MarshalAs(UnmanagedType.U1)] out bool isNormalGmlLevel
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_upper(
                [In] IntPtr gridCodePtr,
                out IntPtr outUpperGridCodePtr
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_grid_code_get_string_code(
                [In] IntPtr gridCodePtr,
                [In, Out] IntPtr outStringCode
            );
        }
    }
}
