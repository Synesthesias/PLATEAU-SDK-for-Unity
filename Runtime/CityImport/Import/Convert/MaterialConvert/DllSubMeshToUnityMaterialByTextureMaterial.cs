using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.CityImport.Import.Convert.MaterialConvert
{
    /// <summary>
    /// 共通ライブラリのSubMeshをUnityのマテリアルに変換する方法の1つを提供します。
    /// その方法とは、CityGMLに記載されているテクスチャパスとマテリアルからUnityのマテリアルを生成します。
    /// </summary>
    internal class DllSubMeshToUnityMaterialByTextureMaterial : IDllSubMeshToUnityMaterialConverter
    {
        //GMLマテリアル、 テクスチャパス と マテリアルを紐付ける辞書です。同じマテリアルが重複して生成されることを防ぎます。
        private readonly Dictionary<ConvertedMeshData.MaterialSet, UnityEngine.Material> cachedMaterials = new ();
        
        /// <summary>
        /// DLLのSubMesh情報をUnityのマテリアルに変換します。
        /// 追加可能なら結果をキャッシュにも追加します。
        /// </summary>
        public async Task<Material> ConvertAsync(
            ConvertedMeshData meshData, int subMeshIndex, Material fallbackMaterial)
        {
            // テクスチャがフォールバックマテリアルのものである場合は、フォールバックマテリアルにします。
            var texturePath = meshData.TextureUrls[subMeshIndex];
            var gmlMaterial = meshData.GmlMaterials[subMeshIndex];
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