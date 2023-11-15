using System;
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
            
            Material material = null;
            var texture = await LoadTexture(texturePath);
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
        
        
        /// <summary>
        /// テクスチャのURL（パス） から、テクスチャを非同期でロードします。
        /// 生成した Unity の Textureインスタンスへの参照を <paramref name="meshData"/> に追加します。
        /// </summary>
        private static async Task<Texture2D> LoadTexture(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                return null;

            // .PLATEAU からの相対パスを求めます。
            string pathToReplace = (PathUtil.PLATEAUSrcFetchDir + "/").Replace('\\', '/');
            string relativePath = (texturePath.Replace('\\', '/')).Replace(pathToReplace, "");

            Debug.Log($"Loading Texture : {texturePath}");

            // 非同期でテクスチャをロードします。
            var texture = await TextureLoader.LoadAsync(texturePath, 3);

            if (texture == null)
                return null;

            // この Compress によってテクスチャ容量が 6分の1 になります。
            // 松山市のLOD2の建物モデルで計測したところ、 テクスチャのメモリ使用量が 2.6GB から 421.3MB になりました。
            // 画質は下がりますが、メモリ使用量を適正にするために必須と思われます。
            var compressedTex = Compress(texture);

            compressedTex.name = relativePath;
            return compressedTex;
        }

        // テクスチャを圧縮します。
        private static Texture2D Compress(Texture2D src)
        {
            // Compressメソッドで圧縮する準備として、幅・高さを4の倍数にする必要があります。
            // 最も近い4の倍数を求めます。
            var widthX4 = (src.width + 2) / 4 * 4;
            var heightX4 = (src.height + 2) / 4 * 4;
            widthX4 = Math.Max(4, widthX4); // 幅・高さが 1ピクセルのケースで 0 に丸められてしまうのを防止します。
            heightX4 = Math.Max(4, heightX4);

            // テクスチャをリサイズします。
            // 参考: https://light11.hatenadiary.com/entry/2018/04/19/194015
            var rt = RenderTexture.GetTemporary(widthX4, heightX4);
            Graphics.Blit(src, rt);
            var prevRt = RenderTexture.active;
            RenderTexture.active = rt;
            var dst = new Texture2D(widthX4, heightX4);
            dst.ReadPixels(new Rect(0, 0, widthX4, heightX4), 0, 0);
            dst.Apply();
            RenderTexture.active = prevRt;
            RenderTexture.ReleaseTemporary(rt);

            // 圧縮のキモです。
            dst.Compress(true);
            return dst;
        }
    }
}