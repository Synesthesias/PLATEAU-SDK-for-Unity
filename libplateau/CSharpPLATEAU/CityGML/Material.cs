using PLATEAU.Interop;
using PLATEAU.Native;
using System;
using System.Runtime.InteropServices;

namespace PLATEAU.CityGML
{

    /// <summary>
    /// マテリアル情報です。
    /// </summary>
    public class Material : Appearance, IEquatable<Material>
    {
        internal Material(IntPtr handle) : base(handle)
        {
        }

        /// <summary> マテリアル情報を返します。 </summary>
        public PlateauVector3f Diffuse =>
            DLLUtil.GetNativeValue<PlateauVector3f>(Handle,
                NativeMethods.plateau_material_get_diffuse);

        public PlateauVector3f Emissive =>
            DLLUtil.GetNativeValue<PlateauVector3f>(Handle,
                NativeMethods.plateau_material_get_emissive);

        public PlateauVector3f Specular =>
            DLLUtil.GetNativeValue<PlateauVector3f>(Handle,
                NativeMethods.plateau_material_get_specular);

        public float AmbientIntensity =>
            DLLUtil.GetNativeValue<float>(Handle,
                NativeMethods.plateau_material_get_ambient_intensity);

        public float Shininess =>
            DLLUtil.GetNativeValue<float>(Handle,
                NativeMethods.plateau_material_get_shininess);

        public float Transparency =>
            DLLUtil.GetNativeValue<float>(Handle,
                NativeMethods.plateau_material_get_transparency);

        public bool IsSmooth =>
            DLLUtil.GetNativeValue<bool>(Handle,
                NativeMethods.plateau_material_is_smooth);

        // DictionaryのKeyとして使用するためにIEquatableのメソッドで値のみの比較を行います。
        public bool Equals(Material other)
        {
            return Diffuse.Equals(other.Diffuse) &&
                    Emissive.Equals(other.Emissive) &&
                    Specular.Equals(other.Specular) &&
                    AmbientIntensity.Equals(other.AmbientIntensity) &&
                    Shininess.Equals(other.Shininess) &&
                    Transparency.Equals(other.Transparency) &&
                    IsSmooth.Equals(other.IsSmooth);
        }

        // DictionaryのKeyとして使用するために比較する値のみのObjectのHashCodeを返します
        public override int GetHashCode()
        {
            return new { Diffuse, Emissive, Specular, AmbientIntensity, Shininess, Transparency, IsSmooth }.GetHashCode(); ;
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_diffuse(
                [In] IntPtr handle,
                out PlateauVector3f diffuse);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_emissive(
                [In] IntPtr handle,
                out PlateauVector3f emissive);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_specular(
                [In] IntPtr handle,
                out PlateauVector3f specular);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_ambient_intensity(
                [In] IntPtr handle,
                out float intensity);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_shininess(
                [In] IntPtr handle,
                out float shininess);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_get_transparency(
                [In] IntPtr handle,
                out float transparency);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_material_is_smooth(
                [In] IntPtr handle,
                out bool is_smooth);
        }
    }
}
