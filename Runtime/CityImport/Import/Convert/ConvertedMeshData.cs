using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityImport.Import.Convert
{
    /// <summary>
    /// DLL側の Mesh を Unity向けに変換したものです。
    /// </summary>
    internal class ConvertedMeshData
    {
        private readonly Vector3[] vertices;
        private readonly Vector2[] uv1;
        private readonly Vector2[] uv4;
        private readonly List<List<int>> subMeshTriangles;
        public List<string> TextureUrls { get; }
        public List<CityGML.Material> GmlMaterials { get; }
        public List<int> GameMaterialIDs { get; }

        private string Name { get; }
        private int SubMeshCount => this.subMeshTriangles.Count;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv1, Vector2[] uv4, List<List<int>> subMeshTriangles,
            List<string> textureUrls, List<CityGML.Material> materials, List<int> gameMaterialIDs, string name)
        {
            this.vertices = vertices;
            this.uv1 = uv1;
            this.uv4 = uv4;
            this.subMeshTriangles = subMeshTriangles;
            TextureUrls = textureUrls;
            GmlMaterials = materials;
            GameMaterialIDs = gameMaterialIDs;
            Name = name;
        }

        /// <summary>
        /// メッシュの形状を変更したあとに必要な後処理です。
        /// </summary>
        private static void PostProcess(Mesh mesh)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 頂点がない場合は nullが返ります。
        /// </summary>
        public async Task<GameObject> PlaceToScene(Transform parentTrans,
            IDllSubMeshToUnityMaterialConverter materialConverter, Material fallbackMaterial, bool isActive)
        {
            var mesh = GenerateUnityMesh();
            if (mesh.vertexCount <= 0) return null;
            var meshObj = new GameObject(Name)
            {
                transform =
                {
                    parent = parentTrans
                },
                isStatic = true
            };
            meshObj.SetActive(isActive);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);

            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {

                //Material設定
                materials[i] = await materialConverter.ConvertAsync(
                    this, i, fallbackMaterial);
            }

            renderer.materials = materials;
            return meshObj;
        }

        public int VerticesCount => this.vertices.Length;

        /// <summary>
        /// データをもとにUnity のメッシュを生成します。
        /// </summary>
        private Mesh GenerateUnityMesh()
        {
            var mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32,
                vertices = this.vertices,
                uv = this.uv1,
                uv4 = this.uv4,
                subMeshCount = this.subMeshTriangles.Count
            };

            // subMesh ごとに Indices(Triangles) を UnityのMeshにコピーします。
            for (int i = 0; i < this.subMeshTriangles.Count; i++)
            {
                mesh.SetTriangles(this.subMeshTriangles[i], i);
            }

            PostProcess(mesh);
            mesh.name = Name;
            return mesh;
        }



        /// <summary>
        /// マテリアルとテクスチャのセットをDictionary Keyとして使用するための構造体です。
        /// </summary>
        internal struct MaterialSet : IEquatable<MaterialSet>
        {
            public MaterialSet(CityGML.Material mat, string texturePathPath)
            {
                if (mat == null)
                {
                    Diffuse = Emissive = Specular = Vector3.zero;
                    AmbientIntensity = Shininess = Transparency = 0f;
                    IsSmooth = false;
                    TexturePath = texturePathPath;
                    HasMaterial = false;
                }
                else
                {
                    Diffuse = new Vector3(mat.Diffuse.X, mat.Diffuse.Y, mat.Diffuse.Z);
                    Emissive = new Vector3(mat.Emissive.X, mat.Emissive.Y, mat.Emissive.Z);
                    Specular = new Vector3(mat.Specular.X, mat.Specular.Y, mat.Specular.Z);
                    AmbientIntensity = mat.AmbientIntensity;
                    Shininess = mat.Shininess;
                    Transparency = mat.Transparency;
                    IsSmooth = mat.IsSmooth;
                    TexturePath = texturePathPath;
                    HasMaterial = true;
                }
            }

            public Vector3 Diffuse { get; }
            public Vector3 Emissive { get; }
            public Vector3 Specular { get; }
            public float AmbientIntensity { get; }
            public float Shininess { get; }
            public float Transparency { get; }
            public bool IsSmooth { get; }
            public string TexturePath { get; }
            public bool HasMaterial { get; }

            public bool Equals(MaterialSet other)
            {
                return Diffuse.Equals(other.Diffuse) &&
                       Emissive.Equals(other.Emissive) &&
                       Specular.Equals(other.Specular) &&
                       AmbientIntensity.Equals(other.AmbientIntensity) &&
                       Shininess.Equals(other.Shininess) &&
                       Transparency.Equals(other.Transparency) &&
                       IsSmooth.Equals(other.IsSmooth) &&
                       TexturePath.Equals(other.TexturePath) &&
                       HasMaterial.Equals(other.HasMaterial);
            }

            public override int GetHashCode()
            {
                return new
                {
                    Diffuse, Emissive, Specular, AmbientIntensity, Shininess, Transparency, IsSmooth,
                    Texture = TexturePath, HasMaterial
                }.GetHashCode();
                ;
            }
        }
    }
}