using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.GeometryModel;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;
using UnityEngine.Networking;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityGrid
{
    internal class ConvertedGameObjData
    {
        private ConvertedMeshData meshData;
        private string name;
        private List<ConvertedGameObjData> children = new List<ConvertedGameObjData>();

        /// <summary>
        /// C++側の <see cref="GeometryModel.Model"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        /// <param name="plateauModel"></param>
        public ConvertedGameObjData(Model plateauModel)
        {
            this.meshData = null;
            this.name = "CityRoot";
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                this.children.Add(new ConvertedGameObjData(rootNode));
            }
            Debug.Log("converted plateau model.");
        }
        
        /// <summary>
        /// C++側の <see cref="GeometryModel.Node"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        public ConvertedGameObjData(Node plateauNode)
        {
            this.meshData = MeshConverter.Convert(plateauNode.Mesh);
            this.name = plateauNode.Name;
            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                this.children.Add(new ConvertedGameObjData(child));
            }
        }

        public async Task PlaceToScene(Transform parent, string gmlAbsolutePath)
        {
            GameObject nextParent;
            if (this.meshData == null)
            {
                nextParent = new GameObject
                {
                    transform =
                    {
                        parent = parent
                    },
                    name = this.name
                };
            }else {
                nextParent = await this.meshData.PlaceToScene(parent, gmlAbsolutePath);
            }

            foreach (var child in this.children)
            {
                await child.PlaceToScene(nextParent.transform, gmlAbsolutePath);
            }
        }
    }
    
    /// <summary>
    /// DLL側の Mesh を Unity向けに変換したものです。
    /// </summary>
    internal class ConvertedMeshData
    {
        private readonly Vector3[] vertices;
        private readonly Vector2[] uv1;
        private readonly Vector2[] uv2;
        private readonly Vector2[] uv3;
        private readonly List<List<int>> subMeshTriangles;
        private readonly List<CityGML.Texture> plateauTextures;
        private string Name { get; }
        private readonly Dictionary<int, Texture> subMeshIdToTexture;
        private const string shaderName = "Standard";
        private int SubMeshCount => this.subMeshTriangles.Count;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv1, Vector2[] uv2, Vector2[] uv3, List<List<int>> subMeshTriangles,List<CityGML.Texture> plateauTextures, string name)
        {
            this.vertices = vertices;
            this.uv1 = uv1;
            this.uv2 = uv2;
            this.uv3 = uv3;
            this.subMeshTriangles = subMeshTriangles;
            this.plateauTextures = plateauTextures;
            Name = name;
            this.subMeshIdToTexture = new Dictionary<int, Texture>();
        }

        private void AddTexture(int subMeshId, Texture tex)
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

        public async Task<GameObject> PlaceToScene(Transform parentTrans, string gmlAbsolutePath)
        {
            var mesh = GenerateUnityMesh();
            if (mesh.vertexCount <= 0) return null;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
            
            await LoadTextures(this, this.plateauTextures, gmlAbsolutePath);
            
            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                materials[i] = new Material(Shader.Find(shaderName));
                if (this.subMeshIdToTexture.TryGetValue(i, out var tex))
                {
                    if (tex != null)
                    {
                        materials[i].mainTexture = tex;
                        materials[i].name = tex.name;
                    }
                }
            }
            renderer.materials = materials;
            return meshObj;
        }
        
        /// <summary>
        /// データをもとにUnity のメッシュを生成します。
        /// </summary>
        private Mesh GenerateUnityMesh()
        {
            var mesh = new Mesh
            {
                vertices = this.vertices,
                uv = this.uv1,
                uv2 = this.uv2,
                uv3 = this.uv3,
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
        /// <paramref name="plateauTextures"/> に記録された URL から、テクスチャを非同期でロードします。
        /// 生成した Unity の Textureインスタンスへの参照を <paramref name="meshData"/> に追加します。
        /// </summary>
        private static async Task LoadTextures(ConvertedMeshData meshData, IReadOnlyList<CityGML.Texture> plateauTextures, string gmlAbsolutePath)
        {
            for (int i = 0; i < meshData.SubMeshCount; i++)
            {
                // テクスチャURLを取得します。
                var plateauTex = plateauTextures[i];
                if (plateauTex == null) continue;
                string texUrl = plateauTex.Url;
                // テクスチャがない状態を表現するとき、urlが "noneTexture" となるのはライブラリの仕様です。
                if (texUrl == "noneTexture")
                {
                    meshData.AddTexture(i, null);
                    continue;
                }
                string textureFullPath = Path.GetFullPath(Path.Combine(gmlAbsolutePath, "../", texUrl));
                
                // 非同期でテクスチャをロードします。
                var request = UnityWebRequestTexture.GetTexture($"file://{textureFullPath}");
                
                request.timeout = 3;
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"failed to load texture : {textureFullPath} result = {(int)request.result}");
                    continue;
                }
                Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                
                // この Compress によってテクスチャ容量が 6分の1 になります。
                // 松山市のLOD2の建物モデルで計測したところ、 テクスチャのメモリ使用量が 2.6GB から 421.3MB になりました。
                // 画質は下がりますが、メモリ使用量を適正にするために必須と思われます。
                ((Texture2D)texture).Compress(true);
                
                // 生成したUnityテクスチャへの参照を meshData に追加します。
                texture.name = Path.GetFileNameWithoutExtension(texUrl);
                meshData.AddTexture(i, texture);
                
            }
        }
    }
}