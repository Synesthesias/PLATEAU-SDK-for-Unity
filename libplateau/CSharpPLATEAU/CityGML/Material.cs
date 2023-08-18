using PLATEAU.Interop;
using PLATEAU.Native;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PLATEAU.CityGML
{

    /// <summary>
    /// マテリアル情報です。
    /// </summary>
    public class Material : Appearance
    {
        internal Material(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// マテリアル情報を構造体にしてDictionaryのKeyとして利用します
        /// </summary>
        public struct MaterialStruct : IEquatable<MaterialStruct>
        {
            public MaterialStruct(PlateauVector3f df, PlateauVector3f em, PlateauVector3f sp, float am, float sh, float tr, bool sm)
            {
                Diffuse = new Vector3(df.X, df.Y, df.Z);
                Emissive = new Vector3(em.X, em.Y, em.Z);
                Specular = new Vector3(sp.X, sp.Y, sp.Z);
                AmbientIntensity = am;
                Shininess = sh;
                Transparency = tr;
                IsSmooth = sm;
            }
            
            public Vector3 Diffuse { get; }
            public Vector3 Emissive { get; }
            public Vector3 Specular { get; }
            public float AmbientIntensity { get; }
            public float Shininess { get; }
            public float Transparency { get; }
            public bool IsSmooth { get; }

            public bool Equals(MaterialStruct other)
            {
                return Diffuse.Equals(other.Diffuse) &&
                    Emissive.Equals(other.Emissive) &&
                    Specular.Equals(other.Specular) &&
                    AmbientIntensity.Equals(other.AmbientIntensity) &&
                    Shininess.Equals(other.Shininess) &&
                    Transparency.Equals(other.Transparency) &&
                    IsSmooth.Equals(other.IsSmooth);
            }
        }

        public MaterialStruct StructValue => new MaterialStruct(Diffuse,Emissive, Specular, AmbientIntensity,Shininess,Transparency,IsSmooth);

        /// <summary>
        /// マテリアル情報を文字列として返します
        /// </summary>
        public string StringValue
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6}", 
                    Diffuse.ToString(), 
                    Emissive.ToString(), 
                    Specular.ToString(), 
                    AmbientIntensity.ToString(), 
                    Shininess.ToString(), 
                    Transparency.ToString(), 
                    Convert.ToInt32(IsSmooth).ToString());
                return sb.ToString();
            }
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
