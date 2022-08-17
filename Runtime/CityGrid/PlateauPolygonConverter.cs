using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLATEAU.CityGML;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// <see cref="PlateauPolygon"/> を Unityの Mesh に変換します。
    /// </summary>
    internal static class PlateauPolygonConverter
    {
        public static UnityMeshWithName Convert(PlateauPolygon plateauPoly)
        {
            // TODO コメントを整備したい
            var mesh = new Mesh();
            int numVerts = plateauPoly.VertexCount;
            var unityVerts = new Vector3[numVerts];
            for (int i = 0; i < numVerts; i++)
            {
                var vert = plateauPoly.GetVertex(i);
                unityVerts[i] = new Vector3((float)vert.X, (float)vert.Y, (float)vert.Z);
            }
            mesh.vertices = unityVerts;

            var plateauIndices = plateauPoly.Indices.ToList();

            var multiTexture = plateauPoly.GetMultiTexture();
            int currentSubMeshStart = 0;
            var subMeshTriangles = new List<List<int>>();
            for (int i = 0; i < multiTexture.Length; i++)
            {
                int nextSubMeshStart = multiTexture[i].VertexIndex;
                int count = nextSubMeshStart - currentSubMeshStart;
                if (count > 0)
                {
                    var subMeshIndices = plateauIndices.GetRange(currentSubMeshStart, count);
                    subMeshTriangles.Add(subMeshIndices);
                }
                currentSubMeshStart = nextSubMeshStart;
            }

            int lastSubMeshCount = plateauIndices.Count - currentSubMeshStart;
            subMeshTriangles.Add(plateauIndices.GetRange(currentSubMeshStart, lastSubMeshCount));
            mesh.subMeshCount = subMeshTriangles.Count;
            for (int i = 0; i < subMeshTriangles.Count; i++)
            {
                mesh.SetTriangles(subMeshTriangles[i], i);
            }
            Debug.Log(mesh.subMeshCount);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            var unityMesh = new UnityMeshWithName(mesh, plateauPoly.ID);

            return unityMesh;
        }


    }
    
    
    public class UnityMeshWithName
    {
        public readonly Mesh Mesh;
        public readonly string Name;

        public UnityMeshWithName(Mesh mesh, string name)
        {
            this.Mesh = mesh;
            this.Name = name;
        }

        public void PlaceToScene(Transform parentTrans)
        {
            if (Mesh.vertexCount <= 0) return;
            var meshObj = GameObjectUtil.AssureGameObjectInChild(Name, parentTrans);
            var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
            meshFilter.mesh = Mesh;
            var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
            var materials = new UnityEngine.Material[Mesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new  UnityEngine.Material(Shader.Find("Standard"));
            }
            renderer.materials = materials;
        }
    }
}