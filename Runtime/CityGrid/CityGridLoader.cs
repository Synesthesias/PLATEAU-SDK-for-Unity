using System.Linq;
using System.Text;
using Codice.CM.Common;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEngine;
using UnityEngine.Rendering;
using Object = PLATEAU.CityGML.Object;

namespace PLATEAU.CityGrid
{
    public class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromAssets;

        #if UNITY_EDITOR
        public void Load()
        {
            string gmlAbsolutePath = Application.dataPath + "/" + this.gmlRelativePathFromAssets;
            CitygmlParserParams parserParams = new CitygmlParserParams(true, true, false);
            var cityModel = CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks, DllLogLevel.Error);
            var meshMerger = new MeshMerger();
            var logger = new DllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            var plateauPolygons = meshMerger.GridMerge(cityModel, CityObjectType.COT_All, 10, 10, logger);
            int numPolygons = plateauPolygons.Length;
            var unityMeshes = new UnityMesh[numPolygons];
            for (int i = 0; i < numPolygons; i++)
            {
                unityMeshes[i] = PlateauPolygonToUnityMesh(plateauPolygons[i]);
            }
            PlaceGridMeshes(unityMeshes, GmlFileNameParser.FileNameWithoutExtension(this.gmlRelativePathFromAssets));
        }
        #endif

        private static UnityMesh PlateauPolygonToUnityMesh(Polygon plateauPoly)
        {
            var mesh = new Mesh();
            int numVerts = plateauPoly.VertexCount;
            var unityVerts = new Vector3[numVerts];
            for (int i = 0; i < numVerts; i++)
            {
                var vert = plateauPoly.GetVertex(i);
                unityVerts[i] = new Vector3((float)vert.X, (float)vert.Y, (float)vert.Z);
            }

            int numIndices = plateauPoly.IndicesCount;
            var unityTriangles = new int[numIndices];
            var plateauIndices = plateauPoly.Indices.ToArray();
            
            // debug print
            var sbIndices = new StringBuilder("Indices : ");
            foreach (int index in plateauIndices)
            {
                sbIndices.Append($"{index}, ");
            }
            Debug.Log(sbIndices.ToString());
            var sbVertices = new StringBuilder("Vertices : ");
            for(int i=0; i < plateauPoly.VertexCount; i++)
            {
                var vert = plateauPoly.GetVertex(i);
                sbVertices.Append($"({vert.X}, {vert.Y}, {vert.Z}), ");
            }
            Debug.Log(sbVertices.ToString());
            
            
            for (int i = 0; i < numIndices; i++)
            {
                unityTriangles[i] = plateauIndices[i];
            }

            mesh.vertices = unityVerts;
            mesh.triangles = unityTriangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            var unityMesh = new UnityMesh(mesh, plateauPoly.ID);
            Debug.Log(unityMesh);
            return unityMesh;
        }

        private static void PlaceGridMeshes(UnityMesh[] unityMeshes, string parentObjName)
        {
            var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
            foreach (var uMesh in unityMeshes)
            {
                var meshObj = GameObjectUtil.AssureGameObjectInChild(uMesh.name, parentTrans);
                var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
                meshFilter.mesh = uMesh.mesh;
                var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
                renderer.material = new UnityEngine.Material(Shader.Find("Standard"));
            }
        }

        private class UnityMesh
        {
            public Mesh mesh;
            public string name;

            public UnityMesh(Mesh mesh, string name)
            {
                this.mesh = mesh;
                this.name = name;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"UnityMesh name={this.name}\n");
                sb.Append($"verticesCount={this.mesh.vertexCount}\nindices = ");
                for (int i = 0; i < this.mesh.triangles.Length; i++)
                {
                    sb.Append($"{this.mesh.triangles[i]} , ");
                }
                return sb.ToString();
            }
        }
    }
}