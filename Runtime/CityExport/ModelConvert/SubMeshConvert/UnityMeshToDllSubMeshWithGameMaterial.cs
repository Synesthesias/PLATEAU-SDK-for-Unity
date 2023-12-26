using System.Collections.Generic;
using PLATEAU.PolygonMesh;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// Unityの<see cref="Mesh"/>から共通ライブラリの<see cref="SubMesh"/>を作るにあたって、
    /// 具体的な見た目情報ではなく、UnityマテリアルのIDのみを<see cref="SubMesh"/>に記録する変換モードです。
    /// 用途は結合分割機能で、あとでUnityマテリアルの番号からマテリアルを復元するために使う変換モードです。
    /// ここでいうマテリアルIDとは、<see cref="GameMaterials"/>のインデックスを指します。
    /// </summary>
    public class UnityMeshToDllSubMeshWithGameMaterial : IUnityMeshToDllSubMeshConverter
    {
        public List<Material> GameMaterials { get; } = new();
        public List<SubMesh> Convert(Mesh unityMesh, Renderer renderer)
        {
            var dllSubMeshes = IUnityMeshToDllSubMeshConverter.ForEachUnitySubMesh(
                unityMesh, renderer,
                (int startIndex, int endIndex, Material material) =>
                {
                    // 各Unity SubMeshについて、マテリアルを記録しながらそのインデックスをDLL SubMeshに送ります。
                    var dllSubMesh = SubMesh.Create(startIndex, endIndex, "");
                    int found = GameMaterials.IndexOf(material);
                    if (found < 0)
                    {
                        GameMaterials.Add(material);
                        found = GameMaterials.Count - 1;
                    }
                    dllSubMesh.GameMaterialID = found;
                    return dllSubMesh;
                });
            return dllSubMeshes;
        }
    }
}