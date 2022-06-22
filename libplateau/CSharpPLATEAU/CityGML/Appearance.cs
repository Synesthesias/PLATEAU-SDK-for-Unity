using System;
using PLATEAU.CityGML.Util;

namespace PLATEAU.CityGML
{

    /// <summary>
    /// <see cref="Texture"/> , <see cref="Material"/> の基底クラスです。
    /// </summary>
    public class Appearance : Object
    {
        internal Appearance(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// Texture なら "Texture" という文字列を返します。
        /// </summary>
        public string Type =>
            DLLUtil.GetNativeString(Handle,
                NativeMethods.plateau_appearance_get_type);

        public bool IsFront =>
            DLLUtil.GetNativeValue<bool>(Handle,
                NativeMethods.plateau_appearance_get_is_front);

        /// <summary>
        /// テーマ名を配列で返します。
        /// </summary>
        public string[] Themes =>
            DLLUtil.GetNativeStringArrayByPtr(
                Handle,
                NativeMethods.plateau_appearance_get_themes_count,
                NativeMethods.plateau_appearance_get_themes);
    }
}