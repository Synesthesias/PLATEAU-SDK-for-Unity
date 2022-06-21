using System;
using LibPLATEAU.NET.Util;

namespace LibPLATEAU.NET.CityGML
{

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