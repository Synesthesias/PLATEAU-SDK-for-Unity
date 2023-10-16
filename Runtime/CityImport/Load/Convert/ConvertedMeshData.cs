using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityImport.Load.Convert
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
        private readonly List<string> textureUrls;
        private readonly List<CityGML.Material> gmlMaterials;

        private string Name { get; }
        private int SubMeshCount => this.subMeshTriangles.Count;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv1, Vector2[] uv4, List<List<int>> subMeshTriangles, List<string> textureUrls, List<CityGML.Material> materials, string name)
        {
            this.vertices = vertices;
            this.uv1 = uv1;
            this.uv4 = uv4;
            this.subMeshTriangles = subMeshTriangles;
            this.textureUrls = textureUrls;
            this.gmlMaterials = materials;
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
        public async Task<GameObject> PlaceToScene(Transform parentTrans, Dictionary<MaterialSet, Material> cachedMaterials, Material fallbackMaterial)
        {
            var mesh = GenerateUnityMesh();
            if (mesh.vertexCount <= 0) return null;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);

            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {

                //Material設定
                var texturePath = textureUrls[i];
                var gmlMaterial = gmlMaterials[i];
                MaterialSet materialSet = new MaterialSet(gmlMaterial, texturePath);

                // マテリアルがキャッシュ済みの場合はキャッシュを使用
                if (cachedMaterials.TryGetValue(materialSet, out var cachedMaterial))
                {
                    materials[i] = cachedMaterial;
                    continue;
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
                materials[i] = material;
                cachedMaterials.Add(materialSet, material);
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

    /// <summary>
    /// マテリアルとテクスチャのセットをDictionary Keyとして使用するための構造体です。
    /// </summary>
    internal struct MaterialSet : IEquatable<MaterialSet>
    {
        public MaterialSet(CityGML.Material mat, string texturePath)
        {
            if (mat == null)
            {
                Diffuse = Emissive = Specular = Vector3.zero;
                AmbientIntensity = Shininess = Transparency = 0f;
                IsSmooth = false;
                Texture = texturePath;
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
                Texture = texturePath;
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
        public string Texture { get; }
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
                Texture.Equals(other.Texture) &&
                HasMaterial.Equals(other.HasMaterial);
        }

        public override int GetHashCode()
        {
            return new { Diffuse, Emissive, Specular, AmbientIntensity, Shininess, Transparency, IsSmooth, Texture, HasMaterial }.GetHashCode(); ;
        }
    }
}