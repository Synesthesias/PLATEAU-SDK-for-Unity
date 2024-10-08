using System.Collections.Generic;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.IO;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// UnityのMeshから共通ライブラリのSubMeshを生成します。
    /// テクスチャ情報を含みます。
    /// 特にエクスポート機能で利用されます。
    /// </summary>
    public class UnityMeshToDllSubMeshWithTexture : IUnityMeshToDllSubMeshConverter
    {
        private bool exportDefaultTextures;
        public List<Material> GameMaterials { get; } = new();

        public UnityMeshToDllSubMeshWithTexture(bool exportDefaultTextures)
        {
            this.exportDefaultTextures = exportDefaultTextures;
        }
        
        public List<SubMesh> Convert(Mesh unityMesh, Renderer renderer)
        {
            var dllSubMeshes = IUnityMeshToDllSubMeshConverter.ForEachUnitySubMesh(
                unityMesh, renderer,
                (int startIndex, int endIndex, Material material) =>
                {
                    // テクスチャパスは、Unityシーン内のテクスチャの名前に記載してあるので取得します。
                    string texturePath = ""; // texturePath が空文字列なら、マテリアル情報はないもの扱います。
                    if (material != null)
                    {
                        // テクスチャをエクスポートします。
                        texturePath = MaterialConverter.MaterialToSubMeshTexturePath(material);
                        
                        // デフォルトマテリアルはエクスポートしない設定であり、デフォルトマテリアルであるなら、テクスチャをエクスポートしません。
                        if ((!exportDefaultTextures) && FallbackMaterial.ByMainTextureName(texturePath) != null)
                        {
                            texturePath = "";
                        }
                        
                    }
                    // パスのチェック
                    if ((!string.IsNullOrEmpty(texturePath)) && (!File.Exists(texturePath)))
                    {
                        Debug.LogError($"テクスチャファイルが存在しません： {texturePath}");
                        texturePath = "";
                    }
                    
                    var dllSubMesh = SubMesh.Create(startIndex, endIndex, texturePath);
                    
                    // Assetsへの変換機能では、GameMaterialを考慮する必要があります
                    GameMaterialIDRegistry.SendMaterialIDToSubMesh(dllSubMesh, GameMaterials, material);

                    return dllSubMesh;
                }
            );

            return dllSubMeshes;
        }
    }
}