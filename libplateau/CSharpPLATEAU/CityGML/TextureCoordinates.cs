using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// テクスチャのマッピングに関する情報です。
    /// テクスチャ座標と <see cref="LinearRing"/> の頂点を紐付けます。
    /// </summary>
    public class TextureCoordinates : Object
    {
        internal TextureCoordinates(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// 保持するテクスチャ座標の数です。
        /// <see cref="LinearRing"/> の頂点とテクスチャ座標が1対1で対応するので、
        /// この値は <see cref="LinearRing"/> の頂点数と同じになります。
        /// </summary>
        public int Vec2CoordsCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_texture_coordinates_count);
                return count;
            }
        }

        /// <summary>
        /// <see cref="LinearRing"/> の <paramref name="index"/> 番目の頂点に対応する
        /// テクスチャ座標を返します。
        /// </summary>
        public PlateauVector2f GetVec2Coord(int index)
        {
            var ret = DLLUtil.GetNativeValue<PlateauVector2f>(Handle, index,
                NativeMethods.plateau_texture_coordinates_get_coordinate);
            return ret;
        }

        /// <summary>
        /// このテクスチャマッピングの対象となる <see cref="LinearRing"/> のIDを返します。
        /// </summary>
        public string TargetLinearRingId =>
            DLLUtil.GetNativeString(Handle,
                NativeMethods.plateau_texture_coordinates_get_target_linear_ring_id);

        /// <summary>
        /// 引数で与えられた <see cref="LinearRing"/> が
        /// このテクスチャマッピングの対象かどうか判定します。
        /// </summary>
        public bool IsRingTarget(LinearRing ring)
        {
            APIResult result = NativeMethods.plateau_texture_coordinates_is_ring_target(
                Handle, out bool isTarget, ring.Handle);
            DLLUtil.CheckDllError(result);
            return isTarget;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_coordinates_get_coordinate(
                [In] IntPtr handle,
                [Out] out PlateauVector2f outCoord,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_coordinates_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_coordinates_get_target_linear_ring_id(
                [In] IntPtr handle,
                out IntPtr strPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_coordinates_is_ring_target(
                [In] IntPtr handle,
                [MarshalAs(UnmanagedType.U1)] out bool outIsTarget,
                [In] IntPtr ringHandle);
        }
    }
}
