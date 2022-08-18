using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Util.Async;
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
        public static async Task<UnityConvertedMesh> Convert(PlateauPolygon plateauPoly, string gmlAbsolutePath)
        {
            var (unityVerts, unityUv1) = await Task.Run(()=>CopyVerticesAndUV1(plateauPoly));
            
            var mesh = new Mesh
            {
                vertices = unityVerts,
                uv = unityUv1
            };
            
            CopyIndicesPerSubMeshes(plateauPoly, mesh, out var plateauTextures);

            PostProcess(mesh);
            var unityMesh = new UnityConvertedMesh(mesh, plateauPoly.ID);
            await LoadTextures(unityMesh, plateauTextures, gmlAbsolutePath);
            return unityMesh;
        }

        private static (Vector3[] unityVerts, Vector2[] unityUv1) CopyVerticesAndUV1(PlateauPolygon plateauPoly)
        {
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
            return (unityVerts, unityUv1);
        }

        private static void CopyIndicesPerSubMeshes(PlateauPolygon plateauPoly, Mesh mesh, out List<Texture> plateauTextures)
        {
            var plateauIndices = plateauPoly.Indices.ToList();
            var multiTexture = plateauPoly.GetMultiTexture();
            int currentSubMeshStart = 0;
            var subMeshTriangles = new List<List<int>>();
            plateauTextures = new List<Texture>();
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

        private static async Task LoadTextures(UnityConvertedMesh unityMesh, IReadOnlyList<Texture> plateauTextures, string gmlAbsolutePath)
        {
            for (int i = 0; i < unityMesh.Mesh.subMeshCount; i++)
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
                unityMesh.AddTexture(i, texture);
                
            }
        }


    }
    
    
    
}