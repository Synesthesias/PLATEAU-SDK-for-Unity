using System.Collections.Generic;
using PLATEAU.CityConvertCommon;
using PLATEAU.PolygonMesh;
using UnityEngine;
using UnityEngine.Assertions;
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
                    GameMaterials.Add(material);
                    var dllSubMesh = SubMesh.Create(startIndex, endIndex, "");
                    dllSubMesh.GameMaterialID = GameMaterials.Count - 1;
                    return dllSubMesh;
                });
            return dllSubMeshes;
        }
    }
}