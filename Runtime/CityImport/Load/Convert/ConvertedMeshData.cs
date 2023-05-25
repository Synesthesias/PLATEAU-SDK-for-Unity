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
        private readonly Vector2[] uv2;
        private readonly Vector2[] uv3;
        private readonly List<List<int>> subMeshTriangles;
        private readonly List<string> textureUrls;
        private string Name { get; }
        private readonly Dictionary<int, Texture> subMeshIdToTexture;
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
        /// 頂点がない場合は nullが返ります。
        /// </summary>
        public async Task<GameObject> PlaceToScene(Transform parentTrans, Dictionary<string, Texture> cachedTexture)
        {
            var mesh = GenerateUnityMesh();
            if (mesh.vertexCount <= 0) return null;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);

            await LoadTextures(this, this.textureUrls, cachedTexture);

            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                materials[i] = new Material(RenderUtil.DefaultMaterial);
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
        private static async Task LoadTextures(ConvertedMeshData meshData, IReadOnlyList<string> textureUrls,
            Dictionary<string, Texture> cachedTexture)
        {
            for (int i = 0; i < meshData.SubMeshCount; i++)
            {
                // TODO テクスチャを返すのが素直な実装であって、返す代わりに meshData.AddTexture で結果を格納するという今のやり方は分かりにくい
                // テクスチャURLを取得します。
                string textureFullPath = textureUrls[i];
                if (string.IsNullOrEmpty(textureFullPath)) 
                {
                    meshData.AddTexture(i, null);
                    continue;
                }
                
                // .PLATEAU からの相対パスを求めます。
                string pathToReplace = PathUtil.PLATEAUSrcFetchDir + "/";
                string relativePath = (textureFullPath.Replace('\\', '/')).Replace(pathToReplace, "");

                // キャッシュにあればそれを使います
                if (cachedTexture.TryGetValue(relativePath, out var tex))
                {
                    meshData.AddTexture(i, tex);
                    continue;
                }
                
                Debug.Log($"Loading Texture : {textureFullPath}");

                // 非同期でテクスチャをロードします。
                var texture = await TextureLoader.LoadAsync($"file://{textureFullPath}", 3);

                if (texture == null) continue;

                // この Compress によってテクスチャ容量が 6分の1 になります。
                // 松山市のLOD2の建物モデルで計測したところ、 テクスチャのメモリ使用量が 2.6GB から 421.3MB になりました。
                // 画質は下がりますが、メモリ使用量を適正にするために必須と思われます。
                var compressedTex = Compress(texture);

                // 生成したUnityテクスチャへの参照を meshData に追加します。
                meshData.AddTexture(i, compressedTex);
                compressedTex.name = relativePath;
                cachedTexture[relativePath] = compressedTex;
            }
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
}