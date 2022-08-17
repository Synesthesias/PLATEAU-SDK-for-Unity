using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using PLATEAU.CityGML;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Texture = PLATEAU.CityGML.Texture;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// <see cref="PlateauPolygon"/> を Unityの Mesh に変換します。
    /// </summary>
    internal static class PlateauPolygonConverter
    {
        /// <summary>
        /// <see cref="PlateauPolygon"/> を Unityの Mesh に変換します。
        /// </summary>
        public static UnityConvertedMesh Convert(PlateauPolygon plateauPoly, string gmlAbsolutePath)
        {
            var mesh = new Mesh();
            
            // 頂点をコピーします。
            int numVerts = plateauPoly.VertexCount;
            var plateauUv1 = plateauPoly.GetUv1();
            var unityVerts = new Vector3[numVerts];
            var unityUv1 = new Vector2[numVerts];
            for (int i = 0; i < numVerts; i++)
            {
                var vert = plateauPoly.GetVertex(i);
                unityVerts[i] = new Vector3((float)vert.X, (float)vert.Y, (float)vert.Z);
                unityUv1[i] = new Vector2(plateauUv1[i].X, plateauUv1[i].Y);
            }
            mesh.vertices = unityVerts;
            mesh.uv = unityUv1;

            // Indices(Triangles)をコピーします。
            // サブメッシュごとに行います。
            var plateauIndices = plateauPoly.Indices.ToList();
            var multiTexture = plateauPoly.GetMultiTexture();
            int currentSubMeshStart = 0;
            var subMeshTriangles = new List<List<int>>();
            var plateauTextures = new List<Texture>();
            Texture currentPlateauTex = null;
            // PlateauPolygon の multiTexture ごとにサブメッシュを分けます。
            for (int i = 0; i < multiTexture.Length; i++)
            {
                int nextSubMeshStart = multiTexture[i].VertexIndex;
                int count = nextSubMeshStart - currentSubMeshStart;
                if (count > 0)
                {
                    var subMeshIndices = plateauIndices.GetRange(currentSubMeshStart, count);
                    subMeshTriangles.Add(subMeshIndices);
                    plateauTextures.Add(currentPlateauTex);
                }
                currentSubMeshStart = nextSubMeshStart;
                currentPlateauTex = multiTexture[i].Texture;
            }
            // 上のforループでは最後の subMesh までは回らないのでここで最後の1回を実行します。
            int lastSubMeshCount = plateauIndices.Count - currentSubMeshStart;
            subMeshTriangles.Add(plateauIndices.GetRange(currentSubMeshStart, lastSubMeshCount));
            plateauTextures.Add(currentPlateauTex);
            // subMesh ごとに Indices(Triangles) を UnityのMeshにコピーします。
            mesh.subMeshCount = subMeshTriangles.Count;
            for (int i = 0; i < subMeshTriangles.Count; i++)
            {
                mesh.SetTriangles(subMeshTriangles[i], i);
            }
            
            // 形状を変更したあとに必要な後処理です。
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            var unityMesh = new UnityConvertedMesh(mesh, plateauPoly.ID);

            // テクスチャに関して
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (plateauTextures[i] == null) continue;
                string textureFullPath = Path.GetFullPath(Path.Combine(gmlAbsolutePath, "../", plateauTextures[i].Url));
                // EditorUtility.DisplayProgressBar("Loading Texture...", textureFullPath, (float)i / plateauTextures.Count);
                // UnityEngine.Texture texture = null;
                // Debug.Log(textureFullPath);
                // var request = UnityWebRequestTexture.GetTexture($"file://{textureFullPath}");
                // request.timeout = 2;
                // request.SendWebRequest();
                // // TODO ここは非同期処理にしたい
                // float loadTime = 0f;
                // bool isLoadSuccess = true;
                // // while (!request.isDone)
                // // {
                // //     Thread.Sleep(50);
                // //     loadTime += 50f / 1000f;
                // //     if (loadTime > 1f)
                // //     {
                // //         isLoadSuccess = false;
                // //         break;
                // //     }
                // // }
                // Thread.Sleep(300);
                // //
                // if (request.result != UnityWebRequest.Result.Success)
                // {
                //     isLoadSuccess = false;
                // }
                //
                // if (!isLoadSuccess)
                // {
                //     Debug.LogError($"failed to load texture : {textureFullPath}");
                //     // TODO 要整理
                //     unityMesh.AddTexture(null);
                //     continue;
                // }
                // texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                // unityMesh.AddTexture(texture);
                // TODO 仮（Assetsフォルダ内でないと動かない）
                string textureAssetsPath = PathUtil.FullPathToAssetsPath(textureFullPath);
                UnityEngine.Texture texture =
                    AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(textureAssetsPath);
                Debug.Log($"loading texture : {textureAssetsPath}");
                if (texture == null)
                {
                    Debug.LogError($"texture {textureAssetsPath} is not found.");
                }
                unityMesh.AddTexture(i, texture);
            }
            EditorUtility.ClearProgressBar();

            return unityMesh;
        }


    }
    
    
    public class UnityConvertedMesh
    {
        public Mesh Mesh { get; }
        public string Name { get; }
        private Dictionary<int, UnityEngine.Texture> subMeshIdToTexture;

        public UnityConvertedMesh(Mesh mesh, string name)
        {
            Mesh = mesh;
            Name = name;
            this.subMeshIdToTexture = new Dictionary<int, UnityEngine.Texture>();
        }

        public void AddTexture(int subMeshId, UnityEngine.Texture tex)
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
            var materials = new UnityEngine.Material[Mesh.subMeshCount];
            for (int i = 0; i < Mesh.subMeshCount; i++)
            {
                materials[i] = new UnityEngine.Material(Shader.Find("Standard"));
                if (this.subMeshIdToTexture.TryGetValue(i, out var tex))
                {
                    materials[i].mainTexture = tex;
                }
            }
            renderer.materials = materials;
        }
    }
}