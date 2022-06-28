using System;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    public enum TextureWrapMode
    {
        WM_None,
        WM_Wrap,        // 繰り返し
        WM_Mirror,      // ミラーの繰り返し
        WM_Clamp,       // the texture is clamped to its edges
        WM_Border       // the resulting color is specified by the borderColor element (RGBA)
    }

    /// <summary>
    /// テクスチャ情報です。
    /// <see cref="Url"/> と <see cref="TextureWrapMode"/> を保持します。
    /// <see cref="TextureTargetDefinition"/> によって保持されます。
    /// </summary>
    public class Texture : Appearance
    {
        internal Texture(IntPtr handle) : base(handle)
        {
        }

        /// <summary> テクスチャのURLを返します。 </summary>
        public string Url =>
            DLLUtil.GetNativeString(Handle,
                NativeMethods.plateau_texture_get_url);

        public TextureWrapMode WrapMode =>
            DLLUtil.GetNativeValue<TextureWrapMode>(Handle,
                NativeMethods.plateau_texture_get_wrap_mode);
    }
}