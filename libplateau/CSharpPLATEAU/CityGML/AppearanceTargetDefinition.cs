using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{

    /// <summary>
    /// <see cref="TextureTargetDefinition"/> , <see cref="MaterialTargetDefinition"/> の基底クラスです。
    /// <see cref="TargetID"/> , <see cref="Appearance"/> を保持します。
    /// <see cref="Appearance"/> は <see cref="TextureTargetDefinition"/> ならば <see cref="Texture"/> 型、
    /// <see cref="MaterialTargetDefinition"/> ならば <see cref="Material"/> 型です。
    /// </summary>
    public class AppearanceTargetDefinition<T> : Object where T : Appearance
    {
        internal AppearanceTargetDefinition(IntPtr handle) : base(handle)
        {
        }

        // AppearanceTargetDefinition の具体的な型は
        // TextureTargetDefinition か MaterialTargetDefinition のどちらかです。
        // どちらかによって呼ぶべき NativeMethod が変わるので場合分けします。

        public T Appearance
        {
            get
            {
                switch (this)
                {
                    case TextureTargetDefinition _:
                        IntPtr ptr = DLLUtil.GetNativeValue<IntPtr>(
                            Handle,
                            AppearanceTargetDefinitionNativeMethods.NativeMethods.plateau_appearance_target_definition_tex_get_appearance
                        );
                        return (T)(Appearance)new Texture(ptr);
                    case MaterialTargetDefinition _:
                        throw new NotImplementedException("Material を含むGMLの例が見当たらないため未実装です。");
                    default:
                        throw new Exception("Unknown type.");
                }
            }
        }

        public string TargetID
        {
            get
            {
                switch (this)
                {
                    case TextureTargetDefinition _:
                        return DLLUtil.GetNativeString(Handle,
                            AppearanceTargetDefinitionNativeMethods.NativeMethods.plateau_appearance_target_definition_tex_get_target_id);
                    case MaterialTargetDefinition _:
                        throw new NotImplementedException("Material を含むGMLの例が見当たらないため未実装です。");
                    default:
                        throw new Exception("Unknown type.");
                }
            }
        }
    }

    internal static class AppearanceTargetDefinitionNativeMethods
    {
        internal static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_definition_tex_get_target_id(
                [In] IntPtr handle,
                out IntPtr strPtr,
                out int strLength);
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_definition_tex_get_appearance(
                [In] IntPtr handle,
                out IntPtr outTextureHandle);
        }
    }
}
