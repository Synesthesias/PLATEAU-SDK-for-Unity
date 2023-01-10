using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML{

    /// <summary>
    /// テクスチャとテクスチャマッピングを紐付けます。
    /// <see cref="AppearanceTargetDefinition{T}.Appearance"/> でテクスチャ情報を取得できます。
    /// <see cref="GetCoordinate"/>(i) で i番目のテクスチャマッピング (<see cref="TextureCoordinates"/>) を取得できます。
    /// <see cref="AppearanceTarget"/> によって保持されます。
    /// </summary>
    public class TextureTargetDefinition : AppearanceTargetDefinition<Texture>
    {
        private TextureCoordinates[] cachedCoords;

        public TextureTargetDefinition(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// 保持するテクスチャマッピングの数です。
        /// </summary>
        public int TexCoordinatesCount
        {
            get
            {
                int count = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_texture_target_definition_get_texture_coordinates_count);
                return count;
            }
        }

        /// <summary>
        /// <paramref name="index"/> 番目のテクスチャマッピングを取得します。
        /// </summary>
        public TextureCoordinates GetCoordinate(int index)
        {
            var ret = DLLUtil.ArrayCache(ref this.cachedCoords, index, TexCoordinatesCount, () =>
            {
                IntPtr coordPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                    NativeMethods.plateau_texture_target_definition_get_texture_coordinates);
                return new TextureCoordinates(coordPtr);
            });
            return ret;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_target_definition_get_texture_coordinates_count(
                [In] IntPtr handle,
                out int count);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_texture_target_definition_get_texture_coordinates(
                [In] IntPtr handle,
                out IntPtr outTexCoords,
                int index);
        }
    }
}
