using System;
using System.IO;
using System.Reflection;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.CityImport.Import.Convert.MaterialConvert
{
    /// <summary>
    /// Unityのマテリアルと、DLLのSubMesh情報を相互変換します。
    /// </summary>
    internal class MaterialConverter
    {
        
        
        /// <summary>
        /// Unityのマテリアルを、DLLのSubMeshのTexturePathに変換します。
        /// </summary>
        public static string MaterialToSubMeshTexturePath(Material mat)
        {
            if (mat == null) return "";
            if (!mat.HasMainTextureAttribute()) return "";
            var tex = mat.mainTexture;
            if (tex == null) return "";
            
#if UNITY_EDITOR
            // デフォルトマテリアルのテクスチャは、GetAssetPathでパスを取得できます。
            string texAssetPath = AssetDatabase.GetAssetPath(tex);
            if (texAssetPath != "")
            {
                return Path.GetFullPath(texAssetPath);
            }
#endif
            // PLATEAUのテクスチャは、テクスチャ名がパスを表すこととしています。
            // 土地の航空写真もこのケースに含まれます。
            return Path.Combine(PathUtil.PLATEAUSrcFetchDir, tex.name);
        }
        
    }

    internal static class MaterialExtension
    {
        /// <summary>
        /// シェーダーによっては、material.mainTexture が失敗することがあります。
        /// mainTextureの実行前に、これの成功に必要な属性をシェーダーが有しているかをチェックします。
        /// OKならtrue、NGならfalseを返します。
        /// </summary>
        public static bool HasMainTextureAttribute(this Material mat)
        {
            // [MainTexture]属性がシェーダーで指定されているかどうかをチェックします。
            // privateメソッドの利用が必要なのでリフレクションで無理やり実行します。
            var matType = mat.GetType();
            var propIdGetMethod = matType.GetMethod("GetFirstPropertyNameIdByAttribute",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (propIdGetMethod == null)
            {
                throw new Exception("Something is wrong");
            }
            int nameIdByAttribute = (int)propIdGetMethod.Invoke(mat, new object[] { ShaderPropertyFlags.MainTexture });
            if (nameIdByAttribute >= 0) return true;
            
            // _MainTexがあるかをチェックします。
            return mat.HasProperty("_MainTex");
        }
    }
}