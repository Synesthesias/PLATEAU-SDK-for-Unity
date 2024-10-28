using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// <see cref="RnmContour"/>からメッシュを作ります。
    /// </summary>
    internal class ContourToMesh
    {
        public Mesh Generate(RnmContourMesh rnmContourMesh, out Dictionary<int, RnmMaterialType> subMeshIDToMatType)
        {
            // LibTessDotNetライブラリを使って、輪郭線をテッセレートします。
            var tessMesh = new RnmTessMesh(); 
            foreach (var rnmContour in rnmContourMesh)
            {
                var tess = new Tess();
                int numPoint = rnmContour.Count;
                var tessContour = new ContourVertex[numPoint];
                for (int i = 0; i < numPoint; i++)
                {
                    var rnmPoint = rnmContour[i];
                    tessContour[i].Position = new Vec3(rnmPoint.Position.x , rnmPoint.Position.y, rnmPoint.Position.z);
                    tessContour[i].Data = rnmPoint.UV1;
                }
                tess.AddContour(tessContour, ContourOrientation.Original);
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3, VertexCombine);
                tessMesh.Add(RnmTessSubMesh.Generate(tess, rnmContour.MaterialType));
            }
            

            return tessMesh.ToUnityMesh(out subMeshIDToMatType);
        }
        
        private object VertexCombine(Vec3 position, object[] data, float[] weights)
        {
            var uv1Array = data.Select(d => (Vector2)d).ToArray();
            var result = Vector2.zero;
            for (int i = 0; i < data.Length; i++)
            {
                result += uv1Array[i] * weights[i];
            }

            return result;
        }
    }
    
    
}