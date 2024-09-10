using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 道路ネットワークをもとに、車線の線となるメッシュを生成します。
    /// </summary>
    public class RoadMarkingGenerator
    {
        private RnModel targetNetwork;
        
        /// <summary> 道路に描かれる線の幅はルールで決まっています </summary>
        private const float LineWidth = 0.15f;

        private const float HeightOffset = 0.05f;

        private static string materialFolder = "PlateauRoadMarkingMaterials";
        private static string materialNameWhite = "PlateauRoadMarkingWhite";
        private static string materialNameYellow = "PlateauRoadMarkingYellow";
        private static Material materialWhite = Resources.Load<Material>(materialFolder + "/" + materialNameWhite);
        private static Material materialYellow = Resources.Load<Material>(materialFolder + "/" + materialNameYellow);
        
        public RoadMarkingGenerator(RnModel targetNetwork)
        {
            this.targetNetwork = targetNetwork;
        }

        public void Generate()
        {
            var ways = MarkedWayList.ComposeFrom(targetNetwork);
            var parent = new GameObject("RoadMarking").transform;
            foreach (var way in ways.Get)
            {
                GenerateWayMarking(way, parent);
            }
        }
        

        private GameObject GenerateWayMarking(MarkedWay way, Transform dstParent)
        {
            var mesh = GenerateMesh(way);
            if (mesh == null) return null;
            return GenerateGameObj(mesh, dstParent);
        }

        private Mesh GenerateMesh(MarkedWay way)
        {
            var points = way.Way.Points.Select(p => p.Vertex).ToArray();
            if (points.Length < 2)
            {
                Debug.LogWarning("Not enough points to generate mesh.");
                return null;
            }

            var mesh = new Mesh();
            var vertices = new Vector3[points.Length * 2];
            int[] triangles = new int[(points.Length - 1) * 6];
            for (int i = 0; i < points.Length; i++)
            {
                var forward = Vector3.zero;
                if (i < points.Length - 1) forward += points[i + 1] - points[i];
                if (i > 0) forward += points[i] - points[i - 1];
                forward.Normalize();
                var right = Vector3.Cross(forward, Vector3.up).normalized;

                vertices[i * 2] = points[i] + right * LineWidth * 0.5f + Vector3.up * HeightOffset;
                vertices[i * 2 + 1] = points[i] - right * LineWidth * 0.5f + Vector3.up * HeightOffset;
                if (i < points.Length - 1)
                {
                    int baseIndex = i * 6;
                    int vertexIndex = i * 2;
                    triangles[baseIndex + 0] = vertexIndex + 0;
                    triangles[baseIndex + 1] = vertexIndex + 2;
                    triangles[baseIndex + 2] = vertexIndex + 1;
                    triangles[baseIndex + 3] = vertexIndex + 2;
                    triangles[baseIndex + 4] = vertexIndex + 3;
                    triangles[baseIndex + 5] = vertexIndex + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private GameObject GenerateGameObj(Mesh mesh, Transform dstParent)
        {
            var obj = new GameObject("RoadMarking");
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(materialWhite);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // 道路と重なっているので影は不要
            obj.transform.parent = dstParent;
            return obj;
        }
    }
}