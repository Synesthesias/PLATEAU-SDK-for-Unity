using System.Collections.Generic;
using PLATEAU.PolygonMesh;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// Unityの<see cref="Mesh"/>から共通ライブラリの<see cref="SubMesh"/>を作るにあたって、
    /// 具体的な見た目をC++に送るのではなく、UnityマテリアルにIDを割り振ってIDのみをC++の<see cref="SubMesh"/>に記録する変換モードです。
    /// 用途はモデル修正で、変換後にUnityマテリアルIDをもとにマテリアルを復元するために使います。
    /// ここでいうマテリアルIDとは、<see cref="GameMaterials"/>のインデックスを指します。
    /// </summary>
    public class GameMaterialIDRegistry : IUnityMeshToDllSubMeshConverter
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
                    SendMaterialIDToSubMesh(dllSubMesh, GameMaterials, material);
                    return dllSubMesh;
                });
            return dllSubMeshes;
        }

        /// <summary>
        /// マテリアルが記録になければ追加します。そのマテリアルに割り当てられたIDをout引数で返します。
        /// </summary>
        public bool TryAddMaterial(Material mat, out int outMatID)
        {
            outMatID = GameMaterials.IndexOf(mat);
            if (outMatID >= 0)
            {
                return false;
            }
            GameMaterials.Add(mat);
            outMatID = GameMaterials.Count - 1;
            return true;
        }

        /// <summary> 与えられたマテリアルが記録にあればそのIDを送り、なければ新たに記録して送ります。 </summary>
        public static void SendMaterialIDToSubMesh(SubMesh dllSubMesh, List<Material> gameMaterials, Material material)
        {
            int found = gameMaterials.IndexOf(material);
            if (found < 0)
            {
                gameMaterials.Add(material);
                found = gameMaterials.Count - 1;
            }
            dllSubMesh.GameMaterialID = found;
        }
    }
}