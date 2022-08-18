using System.Collections.Generic;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// DLL側の PlateauMesh を Unityに変換したものです。
    /// 次の情報が含まれます : Mesh(3D形状)、名前、サブメッシュIDとテクスチャの紐付け情報
    /// </summary>
    internal class UnityConvertedMesh
    {
        private Mesh Mesh { get; }
        private string Name { get; }
        private readonly Dictionary<int, Texture> subMeshIdToTexture;
        private const string shaderName = "Standard";

        public UnityConvertedMesh(Mesh mesh, string name)
        {
            Mesh = mesh;
            Name = name;
            this.subMeshIdToTexture = new Dictionary<int, Texture>();
        }

        public void AddTexture(int subMeshId, Texture tex)
        {
            this.subMeshIdToTexture.Add(subMeshId, tex);
        }

        public void PlaceToScene(Transform parentTrans)
        {
            if (Mesh.vertexCount <= 0) return;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = Mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
            var materials = new Material[Mesh.subMeshCount];
            for (int i = 0; i < Mesh.subMeshCount; i++)
            {
                materials[i] = new Material(Shader.Find(shaderName));
                if (this.subMeshIdToTexture.TryGetValue(i, out var tex))
                {
                    materials[i].mainTexture = tex;
                }
            }
            renderer.materials = materials;
        }
    }
}