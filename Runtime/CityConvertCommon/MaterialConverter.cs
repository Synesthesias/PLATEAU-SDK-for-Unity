using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityConvertCommon
{
    /// <summary>
    /// Unityのマテリアルと、DLLのSubMesh情報を相互変換します。
    /// </summary>
    internal static class MaterialConverter
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

        
        /// <summary>
        /// DLLのSubMesh情報をUnityのマテリアルに変換します。
        /// 追加可能なら結果をキャッシュにも追加します。
        /// </summary>
        public static async Task<Material> SubMeshInfoToMaterialAsync(
            string texturePath, CityGML.Material gmlMaterial, Material fallbackMaterial,
            Dictionary<ConvertedMeshData.MaterialSet, Material> cachedMaterials)
        {
            // テクスチャがフォールバックマテリアルのものである場合は、フォールバックマテリアルにします。
            Material fallbackMat = FallbackMaterial.ByMainTextureName(texturePath);
            if (fallbackMat != null)
            {
                return fallbackMat;
            }
            
            // マテリアルがキャッシュ済みの場合はキャッシュを使用
            ConvertedMeshData.MaterialSet materialSet = new ConvertedMeshData.MaterialSet(gmlMaterial, texturePath);
            if (cachedMaterials.TryGetValue(materialSet, out var cachedMaterial))
            {
                return cachedMaterial;
            }
            
            Material material;
            var texture = await TextureLoader.LoadPlateauTextureAsync(texturePath);
            // マテリアルを決めるための場合分けです。
            if (gmlMaterial == null && texture == null)
            {
                // マテリアル指定もテクスチャ指定もない場合、fallbackMaterialを使います。それもない場合、デフォルトマテリアルを使います。
                if (fallbackMaterial == null)
                {
                    material = RenderUtil.CreateDefaultMaterial();
                }
                else
                {
                    material = fallbackMaterial;
                }
            }
            else
            {
                // マテリアル指定があればそれを使い、なければデフォルトマテリアルを使います。
                if (gmlMaterial != null)
                {
                    material = RenderUtil.GetPLATEAUX3DMaterialByCityGMLMaterial(gmlMaterial);
                    material.name = gmlMaterial.ID;
                }
                else
                {
                    material = RenderUtil.CreateDefaultMaterial();
                }

                //Textureがあればそれを使います。
                if (texture != null)
                {
                    material.mainTexture = texture;
                    material.name = texture.name;
                }
                    
            }

            cachedMaterials.Add(materialSet, material);
            return material;
        }
        
    }
}