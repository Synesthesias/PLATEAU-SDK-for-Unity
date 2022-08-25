using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityGrid
{
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
        private readonly List<string> textureUrls;
        private string Name { get; }
        private readonly Dictionary<int, Texture> subMeshIdToTexture;
        private const string shaderName = "Standard";
        private int SubMeshCount => this.subMeshTriangles.Count;

        public ConvertedMeshData(Vector3[] vertices, Vector2[] uv1, Vector2[] uv2, Vector2[] uv3, List<List<int>> subMeshTriangles, List<string> textureUrls, string name)
        {
            this.vertices = vertices;
            this.uv1 = uv1;
            this.uv2 = uv2;
            this.uv3 = uv3;
            this.subMeshTriangles = subMeshTriangles;
            this.textureUrls = textureUrls;
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

        /// <summary>
        /// ゲームオブジェクト、メッシュ、テクスチャの実体を作ってシーンに配置します。
        /// </summary>
        public async Task<GameObject> PlaceToScene(Transform parentTrans, string gmlAbsolutePath)
        {
            var mesh = GenerateUnityMesh();
            if (mesh.vertexCount <= 0) return null;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);

            await LoadTextures(this, this.textureUrls, gmlAbsolutePath);

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
        /// テクスチャのURL（パス） から、テクスチャを非同期でロードします。
        /// 生成した Unity の Textureインスタンスへの参照を <paramref name="meshData"/> に追加します。
        /// </summary>
        private static async Task LoadTextures(ConvertedMeshData meshData, IReadOnlyList<string> textureUrls, string gmlAbsolutePath)
        {
            for (int i = 0; i < meshData.SubMeshCount; i++)
            {
                // テクスチャURLを取得します。
                string texUrl = textureUrls[i];
                if (string.IsNullOrEmpty(texUrl)) 
                {
                    meshData.AddTexture(i, null);
                    continue;
                }
                string textureFullPath = Path.GetFullPath(Path.Combine(gmlAbsolutePath, "../", texUrl));

                // 非同期でテクスチャをロードします。
                var request = UnityWebRequestTexture.GetTexture($"file://{textureFullPath}");
                request.timeout = 3;
                
                // 注意 :
                // 下の SendWebRequest は、見た目に反してメインスレッドで行われ、Unityのコルーチンによって await します。
                // UnityWebRequestExtension クラスの拡張メソッドにより、コルーチンを await できるようにする機能を使っており、
                // コルーチンはメインスレッド限定です。
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