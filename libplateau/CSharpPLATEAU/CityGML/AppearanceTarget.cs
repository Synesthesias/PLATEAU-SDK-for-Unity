using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// 見た目に関する情報のターゲットとなる CityGMLオブジェクトの基底クラスです。
    /// テーマ名, <see cref="TextureTargetDefinition"/>, <see cref="MaterialTargetDefinition"/> を保持し、それらを紐付けます。
    /// </summary>
    public class AppearanceTarget : Object
    {
        private readonly Dictionary<IntPtr, TextureTargetDefinition> cachedTexTargetDefs;

        internal AppearanceTarget(IntPtr handle) : base(handle)
        {
            this.cachedTexTargetDefs = new Dictionary<IntPtr, TextureTargetDefinition>();
        }

        /// <summary>
        /// 保持する <see cref="TextureTargetDefinitions"/> の数を返します。
        /// </summary>
        public int TextureTargetDefinitionsCount =>
            DLLUtil.GetNativeValue<int>(Handle,
                NativeMethods.plateau_appearance_target_get_texture_target_definitions_count);

        /// <summary>
        /// インデックス指定で <see cref="TextureTargetDefinition"/> を取得します。
        /// </summary>
        public TextureTargetDefinition GetTextureTargetDefinition(int index)
        {
            IntPtr ptr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_appearance_target_get_texture_target_definition_by_index);
            if (this.cachedTexTargetDefs.ContainsKey(ptr))
            {
                return this.cachedTexTargetDefs[ptr];
            }

            return this.cachedTexTargetDefs[ptr] = new TextureTargetDefinition(ptr);
        }

        /// <summary>
        /// 保持する <see cref="TextureTargetDefinition"/> を foreach や LINQ でイテレートするための機能です。
        /// </summary>
        public IEnumerable<TextureTargetDefinition> TextureTargetDefinitions
        {
            get
            {
                int cnt = TextureTargetDefinitionsCount;
                for (int i = 0; i < cnt; i++)
                {
                    yield return GetTextureTargetDefinition(i);
                }
            }
        }

        /// <summary>
        /// テーマ名から <see cref="TextureTargetDefinition"/> を取得します。
        /// 与えられたテーマ名に該当するものがない場合は <see cref="KeyNotFoundException"/> を投げます。
        /// </summary>
        public TextureTargetDefinition GetTextureTargetDefinition(string themeName, bool front)
        {
            var result = NativeMethods.plateau_appearance_target_get_texture_target_definition_by_theme_name(
                Handle, out IntPtr ptr, DLLUtil.StrToUtf8Bytes(themeName), front);
            if (result == APIResult.ErrorValueNotFound)
            {
                throw new KeyNotFoundException($"themeName: {themeName} is not found.");
            }

            DLLUtil.CheckDllError(result);
            if (this.cachedTexTargetDefs.ContainsKey(ptr))
            {
                return this.cachedTexTargetDefs[ptr];
            }

            return this.cachedTexTargetDefs[ptr] = new TextureTargetDefinition(ptr);
        }

        /// <summary>
        /// 保持するテクスチャのテーマのうち、
        /// <paramref name="front"/> が与えられたタイプであるものの数を返します。
        /// </summary>
        public int TextureThemesCountOfFront(bool front)
        {
            APIResult result = NativeMethods.plateau_appearance_target_get_all_texture_theme_names_count(
                Handle, out int count, front);
            DLLUtil.CheckDllError(result);
            return count;
        }

        /// <summary>
        /// テクスチャのテーマ名(0個以上)を取得して配列で返します。
        /// </summary>
        public string[] TextureThemeNames(bool front)
        {
            string[] ret = DLLUtil.GetNativeStringArrayByValue(
                Handle,
                (IntPtr handle, out int arrayLength) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_texture_theme_names_count(
                        handle, out arrayLength, front
                    );
                    return result;
                },
                (handle, strSizes) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_texture_theme_names_str_sizes(
                        handle, strSizes, front);
                    return result;
                },
                (handle, strPtrArrayPtr) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_texture_theme_names(
                        handle, strPtrArrayPtr, front);
                    return result;
                }
            );
            return ret;
        }

        public int MaterialThemesCountByFront(bool front)
        {
            APIResult result = NativeMethods.plateau_appearance_target_get_all_material_theme_names_count(
                Handle, out int count, front);
            DLLUtil.CheckDllError(result);
            return count;
        }

        // TODO 未テスト MaterialTargetDefinition を利用したGMLファイルの例が見当たらないため
        public string[] MaterialThemeNames(bool front)
        {
            string[] ret = DLLUtil.GetNativeStringArrayByValue(
                Handle,
                (IntPtr handle, out int arrayLength) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_material_theme_names_count(
                        handle, out arrayLength, front
                    );
                    return result;
                },
                (handle, strSizes) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_material_theme_names_str_sizes(
                        handle, strSizes, front);
                    return result;
                },
                (handle, strPtrArrayPtr) =>
                {
                    var result = NativeMethods.plateau_appearance_target_get_all_material_theme_names(
                        handle, strPtrArrayPtr, front);
                    return result;
                }
            );
            return ret;
        }

        // TODO 未テスト MaterialTargetDefinition を利用したGMLファイルの例が見当たらないため
        public MaterialTargetDefinition GetMaterialTargetDefinitionByThemeName(string themeName, bool front)
        {
            var result = NativeMethods.plateau_appearance_target_get_material_target_definition_by_theme_name(
                Handle, out IntPtr matTargetHandle, DLLUtil.StrToUtf8Bytes(themeName), front);
            if (result == APIResult.ErrorValueNotFound)
            {
                throw new KeyNotFoundException($"themeName: {themeName} is not found.");
            }

            DLLUtil.CheckDllError(result);
            return new MaterialTargetDefinition(matTargetHandle);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names_count(
                [In] IntPtr handle,
                out int outCount,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names(
                [In] IntPtr handle,
                [In, Out] IntPtr outThemeStrArrayHandle,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names_str_sizes(
                [In] IntPtr handle,
                [Out] int[] outSizeArray,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_material_theme_names_count(
                [In] IntPtr handle,
                out int outCount,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_material_theme_names(
                [In] IntPtr handle,
                [In, Out] IntPtr outThemeStrArrayHandle,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_all_material_theme_names_str_sizes(
                [In] IntPtr handle,
                [Out] int[] outSizeArray,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_appearance_target_get_texture_target_definition_by_theme_name(
                [In] IntPtr handle,
                [Out] out IntPtr outTextureTargetHandle,
                [In] byte[] themeUtf8,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_appearance_target_get_material_target_definition_by_theme_name(
                [In] IntPtr handle,
                [Out] out IntPtr outMaterialTargetHandle,
                [In] byte[] themeNameUtf8,
                [MarshalAs(UnmanagedType.U1)] bool front);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_texture_target_definitions_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_appearance_target_get_texture_target_definition_by_index(
                [In] IntPtr handle,
                out IntPtr outTexTargetDefHandle,
                int index);
        }
    }
}
