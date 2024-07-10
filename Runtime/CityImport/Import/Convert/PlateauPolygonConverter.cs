using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityImport.Import.Convert
{
    /// <summary>
    /// PLATEAUの <see cref="Mesh"/> を Unityの Mesh 用データに変換します。
    /// </summary>
    internal static class MeshConverter
    {
        /// <summary>
        /// C++側の<see cref="PolygonMesh.Mesh"/> を Unity向けのデータ に変換します。
        /// Unityのメッシュを生成する準備としてのデータ生成です。実際のメッシュはまだ触りません。
        /// その理由は <see cref="PlateauToUnityModelConverter.ConvertAndPlaceToScene"/> のコメントを参照してください。
        /// このメソッドの結果をもとに、 <see cref="ConvertedGameObjData.PlaceToScene"/> メソッドで実際のメッシュを配置できます。
        /// </summary>
        public static ConvertedMeshData Convert(PolygonMesh.Mesh plateauMesh, string meshName)
        {
            if (plateauMesh == null) return null;
            CopyVerticesAndUV(plateauMesh, out var unityVerts, out var unityUV1, out var unityUV4);
            CopySubMeshInfo(
                plateauMesh,
                out var subMeshTriangles, out var texturePaths, out var materials,
                out var gameMaterialIDs);
            var meshData = new ConvertedMeshData(unityVerts, unityUV1, unityUV4, subMeshTriangles, texturePaths, materials, gameMaterialIDs, meshName);
            return meshData;
        }

        private static void CopyVerticesAndUV(PolygonMesh.Mesh plateauMesh, out Vector3[] unityVerts, out Vector2[] unityUV1, out Vector2[] unityUV4)
        {
            int numVerts = plateauMesh.VerticesCount;
            var plateauUv1 = plateauMesh.GetUv1();
            var plateauUv4 = plateauMesh.GetUv4();
            unityVerts = new Vector3[numVerts];
            unityUV1 = new Vector2[numVerts];
            unityUV4 = new Vector2[numVerts];

            for (int i = 0; i < numVerts; i++)
            {
                var vert = plateauMesh.GetVertexAt(i);
                unityVerts[i] = new Vector3((float)vert.X, (float)vert.Y, (float)vert.Z);
                unityUV1[i] = new Vector2(plateauUv1[i].X, plateauUv1[i].Y);
                unityUV4[i] = new Vector2(plateauUv4[i].X, plateauUv4[i].Y);
            }
        }

        private static void CopySubMeshInfo(
            PolygonMesh.Mesh plateauMesh,
            out List<List<int>> subMeshTriangles, out List<string> texturePaths, out List<CityGML.Material> materials,
            out List<int> gameMaterialIDs)
        {
            subMeshTriangles = new List<List<int>>();
            texturePaths = new List<string>();
            materials = new List<CityGML.Material>();
            gameMaterialIDs = new List<int>();
            int numSubMesh = plateauMesh.SubMeshCount;
            for (int i = 0; i < numSubMesh; i++)
            {
                var subMesh = plateauMesh.GetSubMeshAt(i);
                int sizeOfList = subMesh.EndIndex - subMesh.StartIndex + 1;
                var subMeshIndices = new List<int>(sizeOfList);
                if (sizeOfList % 3 != 0)
                {
                    throw new Exception($"三角形リストの要素数が3の倍数になりません。 num={sizeOfList}");
                }
                for (int j = subMesh.StartIndex; j <= subMesh.EndIndex; j++)
                {
                    subMeshIndices.Add(plateauMesh.GetIndiceAt(j));
                }
                subMeshTriangles.Add(subMeshIndices);
                texturePaths.Add(subMesh.TexturePath);
                materials.Add(subMesh.Material);
                gameMaterialIDs.Add(subMesh.GameMaterialID);
            }
        }
    }
}