using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.Util;
using Unity.Collections;
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
        public Vector3[] Vertices { get; }
        public Vector2[] UV1 { get; }
        public Vector2[] UV4 { get; }
        public int[] Indices { get; }
        public int[] SubMeshStarts { get; }
        public int[] SubMeshLengths { get; }
        public string[] TextureUrls { get; }
        public CityGML.Material[] GmlMaterials { get; }
        public int[] GameMaterialIDs { get; }

        public string Name { get; }
        public int IndicesCount => Indices.Length;
        public int SubMeshCount => SubMeshStarts.Length;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv1, Vector2[] uv4, int[] indices, int[] subMeshStarts, int[] subMeshLengths,
            string[] textureUrls, CityGML.Material[] materials, int[] gameMaterialIDs, string name)
        {
            int numSubMesh = subMeshStarts.Length;
            if (subMeshLengths.Length != numSubMesh || textureUrls.Length != numSubMesh ||
                materials.Length != numSubMesh || gameMaterialIDs.Length != numSubMesh)
            {
                Debug.LogError("サブメッシュの数が一致しません。");
            }
            Vertices = vertices;
            UV1 = uv1;
            UV4 = uv4;
            Indices = indices;
            SubMeshStarts = subMeshStarts;
            SubMeshLengths = subMeshLengths;
            TextureUrls = textureUrls;
            GmlMaterials = materials;
            GameMaterialIDs = gameMaterialIDs;
            Name = name;
        }

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// 頂点がない場合は nullが返ります。
        /// </summary>
        public async Task<GameObject> PlaceToScene(Transform parentTrans,
            IDllSubMeshToUnityMaterialConverter materialConverter, Material fallbackMaterial, bool isActive)
        {
            // ここでメッシュを作ります
            var mesh = new UnityMeshGenerator().Generate(this);
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

        public int VerticesCount => this.Vertices.Length;
        
             



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