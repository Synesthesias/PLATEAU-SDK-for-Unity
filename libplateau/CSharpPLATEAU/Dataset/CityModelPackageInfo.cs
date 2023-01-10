using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// 都市モデルインポート時のパッケージごとの設定です。
    /// </summary>
    public class CityModelPackageInfo : PInvokeDisposable
    {
        private CityModelPackageInfo(IntPtr handle) : base(handle)
        {
        }

        public static CityModelPackageInfo Create(bool hasAppearance, int minLOD, int maxLOD)
        {
            var apiResult = NativeMethods.plateau_create_city_model_package_info(
                out var ptr, hasAppearance, minLOD, maxLOD);
            DLLUtil.CheckDllError(apiResult);
            return new CityModelPackageInfo(ptr);
        }

        /// <summary>
        /// 与えられたパッケージ種別に対して、仕様上存在する可能性のあるLODの範囲と、仕様上テクスチャが存在する可能性があるかをタプルで返します。
        /// </summary>
        public static (bool hasAppearance, int minLOD, int maxLOD) GetPredefined(PredefinedCityModelPackage package)
        {
            var apiResult = NativeMethods.plateau_city_model_package_info_get_predefined(
                package,
                out bool hasAppearance, out int minLOD, out int maxLOD);
            DLLUtil.CheckDllError(apiResult);
            return (hasAppearance, minLOD, maxLOD);
        } 

        public bool HasAppearance =>
            DLLUtil.GetNativeValue<bool>(Handle,
                NativeMethods.plateau_city_model_package_info_get_has_appearance);

        public int MinLOD => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_city_model_package_info_get_min_lod);

        public int MaxLOD => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_city_model_package_info_get_max_lod);

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_city_model_package_info(Handle);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_city_model_package_info(
                out IntPtr outPackageInfoPtr,
                [MarshalAs(UnmanagedType.U1)] bool hasAppearance, int minLOD, int maxLOD);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_city_model_package_info(
                [In] IntPtr packageInfoPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_package_info_get_has_appearance(
                [In] IntPtr packageInfoPtr,
                [MarshalAs(UnmanagedType.U1)] out bool outHasAppearance);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_package_info_get_min_lod(
                [In] IntPtr packageInfoPtr,
                out int outMinLOD);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_package_info_get_max_lod(
                [In] IntPtr packageInfoPtr,
                out int outMaxLOD);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_city_model_package_info_get_predefined(
                PredefinedCityModelPackage package,
                [MarshalAs(UnmanagedType.U1)] out bool outHasAppearance,
                out int outMinLOD, out int outMaxLOD);
        }
    }
}
