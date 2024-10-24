using LibTessDotNet;
using System.Collections.Generic;
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
                    tessContour[i].Position = new Vec3(rnmPoint.x , rnmPoint.y, rnmPoint.z);
                }
                tess.AddContour(tessContour, ContourOrientation.Original);
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
                tessMesh.Add(RnmTessSubMesh.Generate(tess, rnmContour.MaterialType));
            }
            

            return tessMesh.ToUnityMesh(out subMeshIDToMatType);
        }
        
    }
}