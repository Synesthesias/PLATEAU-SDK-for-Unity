using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEngine;

namespace PLATEAU.CityGrid
{
    public class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromAssets;
        [SerializeField] private int numGridX = 10;
        [SerializeField] private int numGridY = 10;

        #if UNITY_EDITOR
        public void Load()
        {
            if (this.numGridX <= 0 || this.numGridY <= 0)
            {
                Debug.LogError("numGrid の値を1以上にしてください");
                return;
            }
            string gmlAbsolutePath = Application.dataPath + "/" + this.gmlRelativePathFromAssets;
            CitygmlParserParams parserParams = new CitygmlParserParams(true, true, false);
            var cityModel = CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
            var meshMerger = new MeshMerger();
            var logger = new DllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            var plateauPolygons = meshMerger.GridMerge(cityModel, CityObjectType.COT_All, this.numGridX, this.numGridY, logger);
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
            
            for (int i = 0; i < numIndices; i++)
            {
                unityTriangles[i] = plateauIndices[i];
            }

            mesh.vertices = unityVerts;
            mesh.triangles = unityTriangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            var unityMesh = new UnityMesh(mesh, plateauPoly.ID);
            return unityMesh;
        }

        private static void PlaceGridMeshes(UnityMesh[] unityMeshes, string parentObjName)
        {
            var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
            foreach (var uMesh in unityMeshes)
            {
                if (uMesh.Mesh.vertexCount <= 0) continue;
                var meshObj = GameObjectUtil.AssureGameObjectInChild(uMesh.Name, parentTrans);
                var meshFilter = GameObjectUtil.AssureComponent<MeshFilter>(meshObj);
                meshFilter.mesh = uMesh.Mesh;
                var renderer = GameObjectUtil.AssureComponent<MeshRenderer>(meshObj);
                renderer.material = new UnityEngine.Material(Shader.Find("Standard"));
            }
        }

        private class UnityMesh
        {
            public readonly Mesh Mesh;
            public readonly string Name;

            public UnityMesh(Mesh mesh, string name)
            {
                this.Mesh = mesh;
                this.Name = name;
            }
        }
    }
}