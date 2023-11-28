using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.Load.Convert.MaterialConvert
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
}