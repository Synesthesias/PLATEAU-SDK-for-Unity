using System.Collections.Generic;
using System.Linq;
using PLATEAU.Util;
using UnityEngine;
using Texture = PLATEAU.CityGML.Texture;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// <see cref="Mesh"/> を Unityの Mesh に変換します。
    /// </summary>
    internal static class MeshConverter
    {
        /// <summary>
        /// C++側の<see cref="GeometryModel.Mesh"/> を Unity向けのデータ に変換します。
        /// Unityのメッシュを生成する準備としてのデータ生成です。実際のメッシュはまだ触りません。
        /// その理由は <see cref="CityGridLoader.Load"/> のコメントを参照してください。
        /// このメソッドの結果をもとに、 <see cref="ConvertedGameObjData.PlaceToScene"/> メソッドで実際のメッシュを配置できます。
        /// </summary>
        public static ConvertedMeshData Convert(GeometryModel.Mesh plateauMesh, string meshName)
        {
            if (plateauMesh == null) return null;
            var (unityVerts, unityUv1, unityUv2, unityUv3) =
                CopyVerticesAndUV(plateauMesh);
            var (subMeshTriangles, textureUrls) = CopySubMeshInfo(plateauMesh);
            var meshData = new ConvertedMeshData(unityVerts, unityUv1, unityUv2, unityUv3, subMeshTriangles, textureUrls, meshName);
            return meshData;
        }

        private static
            (Vector3[] unityVerts, Vector2[] unityUv1, Vector2[] unityUv2, Vector2[] unityUv3)
            CopyVerticesAndUV(GeometryModel.Mesh plateauMesh)
        {
            int numVerts = plateauMesh.VerticesCount;
            var plateauUv1 = plateauMesh.GetUv1();
            var plateauUv2 = plateauMesh.GetUv2();
            var plateauUv3 = plateauMesh.GetUv3();
            var unityVerts = new Vector3[numVerts];
            var unityUv1 = new Vector2[numVerts];
            var unityUv2 = new Vector2[numVerts];
            var unityUv3 = new Vector2[numVerts];
            for (int i = 0; i < numVerts; i++)
            {
                var vert = plateauMesh.GetVertexAt(i);
                unityVerts[i] = new Vector3((float)vert.X, (float)vert.Y, (float)vert.Z);
                unityUv1[i] = new Vector2(plateauUv1[i].X, plateauUv1[i].Y);
                unityUv2[i] = new Vector2(plateauUv2[i].X, plateauUv2[i].Y);
                unityUv3[i] = new Vector2(plateauUv3[i].X, plateauUv3[i].Y);
            }
            return (unityVerts, unityUv1, unityUv2, unityUv3);
        }

        private static (List<List<int>> subMeshTriangles, List<string> textureUrls) CopySubMeshInfo(GeometryModel.Mesh plateauMesh)
        {
            // var plateauIndices = plateauMesh.Indices.ToList();
            var multiTexture = plateauMesh.GetMultiTexture();
            int currentSubMeshStart = 0;
            var subMeshTriangles = new List<List<int>>();
            var texUrls = new List<string>();
            string currentTexUrl = "";
            int numTexInfo = multiTexture.Length;
            // TODO このへんのsubMeshの実装はちょっと分かりにくい。あとで仕様変更で SubMeshクラスの導入により分かりやすくなるはず
            // Mesh の multiTexture ごとにサブメッシュを分けます。
            for (int i = 0; i <= numTexInfo; i++)
            {
                int nextSubMeshStart =
                    (i == numTexInfo) ?
                        plateauMesh.IndicesCount :
                        multiTexture[i].VertexIndex;
                int count = nextSubMeshStart - currentSubMeshStart;
                if (count > 0)
                {
                    var subMeshIndices = new List<int>();
                    for (int j = 0; j < count; j++)
                    {
                        subMeshIndices.Add(plateauMesh.GetIndiceAt(j + currentSubMeshStart));
                    }
                    subMeshTriangles.Add(subMeshIndices);
                    texUrls.Add(currentTexUrl);
                }

                if (i < numTexInfo)
                {
                    currentSubMeshStart = nextSubMeshStart;
                    currentTexUrl = multiTexture[i].TextureUrl;
                }
            }
            return (subMeshTriangles, texUrls);
        }


    }



}