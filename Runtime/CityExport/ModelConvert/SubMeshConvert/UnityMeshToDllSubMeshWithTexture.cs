using System;
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
    ///
    /// 注意：シーン内蔵のテクスチャについては、テクスチャのファイルパスをC++に渡す目的で、実行時に一時ファイルに書き出します。
    ///      そのためDisposeパターンでクリーンアップメソッドを呼んで一時ファイルを消す必要があります。
    /// </summary>
    public class UnityMeshToDllSubMeshWithTexture : IUnityMeshToDllSubMeshConverter, IDisposable
    {
        private bool exportDefaultTextures;
        private readonly MaterialConverter materialConverter; // 自身のDispose時にこれをDisposeする必要があります
        private bool disposed = false;
        public List<Material> GameMaterials { get; } = new();

        public UnityMeshToDllSubMeshWithTexture(bool exportDefaultTextures)
        {
            this.exportDefaultTextures = exportDefaultTextures;
            this.materialConverter = new MaterialConverter();
        }
        
        public List<SubMesh> Convert(Mesh unityMesh, Renderer renderer)
        {
            var dllSubMeshes = IUnityMeshToDllSubMeshConverter.ForEachUnitySubMesh(
                unityMesh, renderer,
                (int startIndex, int endIndex, Material material) =>
                {
                    string texturePath = ""; // texturePath が空文字列なら、マテリアル情報はないもの扱います。
                    if (material != null)
                    {
                        // テクスチャをエクスポートします。
                        texturePath = materialConverter.MaterialToSubMeshTexturePath(material);
                        
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
        
        /// <summary>
        /// 作成された一時テクスチャファイルをクリーンアップします。
        /// </summary>
        private void CleanupTemporaryFiles()
        {
            materialConverter?.CleanupTemporaryTextureFiles();
        }
        
        /// <summary>
        /// IDisposable実装: 一時ファイルを自動的にクリーンアップします。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // マネージドリソースの破棄
                    materialConverter?.Dispose();
                }
                // アンマネージドリソースがあればここで破棄
                disposed = true;
            }
        }
    }
}