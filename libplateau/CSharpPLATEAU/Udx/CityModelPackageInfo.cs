using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
{
    /// <summary>
    /// 都市モデルインポート時のパッケージごとの設定です。
    /// </summary>
    public class CityModelPackageInfo : PInvokeDisposable
    {
        public CityModelPackageInfo(IntPtr handle) : base(handle)
        {
        }

        public static CityModelPackageInfo Create(bool hasAppearance, int minLOD, int maxLOD)
        {
            var apiResult = NativeMethods.plateau_create_city_model_package_info(
                out var ptr, hasAppearance, minLOD, maxLOD);
            DLLUtil.CheckDllError(apiResult);
            return new CityModelPackageInfo(ptr);
        }

        public static (bool hasAppearance, int minLOD, int maxLOD) GetPredefined(PredefinedCityModelPackage package)
        {
            var apiResult = NativeMethods.plateau_city_model_package_info_get_predefined(
                package,
                out bool hasAppearance, out int minLOD, out int maxLOD);
            DLLUtil.CheckDllError(apiResult);
            return (hasAppearance, minLOD, maxLOD);
        } 

        public bool HasAppearance
        {
            get => DLLUtil.GetNativeValue<bool>(Handle,
                NativeMethods.plateau_city_model_package_info_get_has_appearance);
        }

        public int MinLOD
        {
            get => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_city_model_package_info_get_min_lod);
        }

        public int MaxLOD
        {
            get => DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_city_model_package_info_get_max_lod);
        }

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_city_model_package_info(Handle);
        }
    }
}
