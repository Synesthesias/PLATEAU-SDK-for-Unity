using System.Collections.Generic;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.PolygonMesh;
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
        public List<SubMesh> Convert(Mesh unityMesh, Renderer renderer)
        {
            var dllSubMeshes = IUnityMeshToDllSubMeshConverter.ForEachUnitySubMesh(
                unityMesh, renderer,
                (int startIndex, int endIndex, Material material) =>
                {
                    // テクスチャパスは、Unityシーン内のテクスチャの名前に記載してあるので取得します。
                    string texturePath = "";
                    if (material != null)
                    {
                        texturePath = MaterialConverter.MaterialToSubMeshTexturePath(material);
                    }
                    return SubMesh.Create(startIndex, endIndex, texturePath);
                }
            );

            return dllSubMeshes;
        }
    }
}