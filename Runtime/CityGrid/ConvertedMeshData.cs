using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.Networking;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// DLL側の PlateauMesh を Unity向けに変換したものです。
    /// </summary>
    internal class ConvertedMeshData
    {
        public Vector3[] Vertices;
        public Vector2[] Uv;
        public List<List<int>> SubMeshTriangles;
        public List<CityGML.Texture> PlateauTextures;
        public string Name { get; }
        private readonly Dictionary<int, Texture> subMeshIdToTexture;
        private const string shaderName = "Standard";
        public int SubMeshCount => this.SubMeshTriangles.Count;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv, List<List<int>> subMeshTriangles,List<CityGML.Texture> plateauTextures, string name)
        {
            this.Vertices = vertices;
            this.Uv = uv;
            this.SubMeshTriangles = subMeshTriangles;
            this.PlateauTextures = plateauTextures;
            Name = name;
            this.subMeshIdToTexture = new Dictionary<int, Texture>();
        }

        public void AddTexture(int subMeshId, Texture tex)
        {
            this.subMeshIdToTexture.Add(subMeshId, tex);
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

        public async Task PlaceToScene(Transform parentTrans, string gmlAbsolutePath)
        {
            var mesh = GenerateMesh();
            if (mesh.vertexCount <= 0) return;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
            
            await LoadTextures(this, this.PlateauTextures, gmlAbsolutePath);
            
            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                materials[i] = new Material(Shader.Find(shaderName));
                if (this.subMeshIdToTexture.TryGetValue(i, out var tex))
                {
                    materials[i].mainTexture = tex;
                    materials[i].name = tex.name;
                }
            }
            renderer.materials = materials;
        }
        
        private Mesh GenerateMesh()
        {
            var mesh = new Mesh
            {
                vertices = this.Vertices,
                uv = this.Uv,
                subMeshCount = this.SubMeshTriangles.Count
            };
            // subMesh ごとに Indices(Triangles) を UnityのMeshにコピーします。
            for (int i = 0; i < this.SubMeshTriangles.Count; i++)
            {
                mesh.SetTriangles(this.SubMeshTriangles[i], i);
            }

            PostProcess(mesh);
            mesh.name = Name;
            return mesh;
        }
        
        private static async Task LoadTextures(ConvertedMeshData meshData, IReadOnlyList<CityGML.Texture> plateauTextures, string gmlAbsolutePath)
        {
            for (int i = 0; i < meshData.SubMeshCount; i++)
            {
                if (plateauTextures[i] == null) continue;
                string textureFullPath = Path.GetFullPath(Path.Combine(gmlAbsolutePath, "../", plateauTextures[i].Url));
                var request = UnityWebRequestTexture.GetTexture($"file://{textureFullPath}");
                request.timeout = 3;
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"failed to load texture : {textureFullPath} result = {(int)request.result}");
                    continue;
                }
                UnityEngine.Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                texture.name = Path.GetFileNameWithoutExtension(plateauTextures[i].Url);
                meshData.AddTexture(i, texture);
                
            }
        }
    }
}