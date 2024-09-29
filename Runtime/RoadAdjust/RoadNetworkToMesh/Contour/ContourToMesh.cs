using LibTessDotNet;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// <see cref="RnmContour"/>からメッシュを作ります。
    /// </summary>
    internal class ContourToMesh
    {
        public Mesh Generate(RnmContour rnmContour)
        {
            // LibTessDotNetライブラリを使って、輪郭線をテッセレートします。
            var tess = new Tess();
            int numPoint = rnmContour.Count;
            var tessContour = new ContourVertex[numPoint];
            for (int i = 0; i < numPoint; i++)
            {
                var rnmPoint = rnmContour[i];
                tessContour[i].Position = new Vec3(rnmPoint.x, rnmPoint.y, rnmPoint.z);
            }
            tess.AddContour(tessContour, ContourOrientation.Clockwise);
            tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3);
            
            // 結果をUnityのMeshにします。
            int numTriangle = tess.ElementCount;
            var triangles = new int[numTriangle * 3];
            for (int i = 0; i < numTriangle; i++)
            {
                triangles[i*3] = tess.Elements[i*3];
                triangles[i*3+1] = tess.Elements[i*3+1];
                triangles[i*3+2] = tess.Elements[i*3+2];
            }
            
            var vertices = new Vector3[tess.VertexCount];
            for (int i = 0; i < tess.VertexCount; i++)
            {
                var tessVert = tess.Vertices[i];
                vertices[i] = new Vector3(tessVert.Position.X, tessVert.Position.Y, tessVert.Position.Z);
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}