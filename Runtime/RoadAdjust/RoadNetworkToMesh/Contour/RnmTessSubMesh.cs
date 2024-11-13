using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークの輪郭線をテッセレーターにかけた結果から、UnityのMeshに変換するときに使う中間データ構造であり、
    /// メッシュ結合前の、マテリアルが1つである部分メッシュを表現します。
    /// </summary>
    internal class RnmTessSubMesh
    {
        private readonly Vector3[] vertices;
        private readonly Vector2[] uv1;
        private readonly int[] triangles;
        public RnmMaterialType MatType { get; private set; }
        public int VertexCount => vertices.Length;
        
        public static RnmTessSubMesh Generate(Tess tess, RnmMaterialType matType)
        {
            return TessToTessMesh(tess, matType);
        }

        private RnmTessSubMesh(Vector3[] vertices, int[] triangles, Vector2[] uv1, RnmMaterialType matType)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uv1 = uv1;
            this.MatType = matType;
        }

        private static RnmTessSubMesh TessToTessMesh(Tess tess, RnmMaterialType matType)
        {
            int numTriangle = tess.ElementCount;
            int numVertex = tess.VertexCount;
            var triangles = new int[numTriangle * 3];
            for (int i = 0; i < numTriangle; i++)
            {
                triangles[i*3] = tess.Elements[i*3];
                triangles[i*3+1] = tess.Elements[i*3+1];
                triangles[i*3+2] = tess.Elements[i*3+2];
            }
            
            var vertices = new Vector3[numVertex];
            for (int i = 0; i < numVertex; i++)
            {
                var tessVert = tess.Vertices[i];
                vertices[i] = new Vector3(tessVert.Position.X, tessVert.Position.Y, tessVert.Position.Z);
            }

            var uv1 = new Vector2[numVertex];
            for (int i = 0; i < numVertex; i++)
            {
                uv1[i] = (Vector2)tess.Vertices[i].Data;
            }

            return new RnmTessSubMesh(vertices, triangles, uv1, matType);
        }

        public CombineInstance ToCombineInstance()
        {
            var combine = new CombineInstance { mesh = ToUnityMesh(), transform = Matrix4x4.identity };
            return combine;
        }

        private Mesh ToUnityMesh()
        {
            var mesh = new Mesh { vertices = vertices, triangles = triangles, uv = uv1 };
            return mesh;
        }
    }

    /// <summary>
    /// 道路ネットワークの輪郭線をテッセレーターにかけた結果から、UnityのMeshに変換するときに使う中間データ構造であり、
    /// <see cref="RnmTessSubMesh"/>を結合してUnityのMeshを返します。
    /// </summary>
    internal class RnmTessMesh
    {
        private List<RnmTessSubMesh> subMeshes = new ();

        public void Add(RnmTessSubMesh subMesh)
        {
            if (subMesh == null)
            {
                Debug.LogWarning("subMesh is null.");
                return;
            }
            subMeshes.Add(subMesh);
        }

        public Mesh ToUnityMesh(out Dictionary<int, RnmMaterialType> subMeshIDToMatType)
        {
            subMeshIDToMatType = new();
            
            
            // まず同一マテリアルごとに結合します。
            var mats = subMeshes.Select(s => s.MatType).Distinct();
            var matMeshes = new List<Mesh>();
            int matID = 0;
            foreach (var mat in mats)
            {
                var matSubs = subMeshes.Where(s => s.MatType == mat).ToArray();
                if (matSubs.Length == 0)
                {
                    Debug.LogWarning("Invalid matSubs length.");
                    continue;
                }
                var matCombines = 
                    matSubs
                        .Where(m => m.VertexCount > 0)
                        .Select(s => s.ToCombineInstance())
                        .ToArray();
                
                if(matCombines.Length == 0) continue;
                
                var matMesh = new Mesh();
                matMesh.CombineMeshes(matCombines.ToArray(), true);
                matMeshes.Add(matMesh);
                subMeshIDToMatType.Add(matID, mat);
                matID++;
            }
            
            // 次にマテリアル別に結合します。
            var combines = matMeshes.Select(m => new CombineInstance() { mesh = m, transform = Matrix4x4.identity });
            
            var mesh = new Mesh();
            mesh.CombineMeshes(combines.ToArray(), false);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}