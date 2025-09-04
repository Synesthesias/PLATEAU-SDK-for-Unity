using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.Geometries
{
    public static class CoordinateReferenceFactory
    {

        public const int DEFAULT_EPSG = 6697;

        /// <summary>
        /// 平面直角座標系への変換が必要なGMLファイルかどうかを返します。
        /// 取得失敗時のデフォルト値はtrueです。
        /// </summary>
        public static bool IsPolarCoordinateSystem(int Epsg)
        {
            var result = NativeMethods.plateau_geometry_utils_is_polar_coordinate_system(Epsg, out var isPolar);
            DLLUtil.CheckDllError(result);
            return isPolar;
        }

        internal static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_utils_is_polar_coordinate_system(
                int epsg,
                [MarshalAs(UnmanagedType.U1)] out bool outBool);
        }
    }
}

