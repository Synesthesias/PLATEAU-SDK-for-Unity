using LibTessDotNet.PLATEAU;
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
            // 輪郭線からなる図形ごとにテッセレートします。
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
                tess.AddContour(tessContour);
                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3, VertexCombine);
                tessMesh.Add(RnmTessSubMesh.Generate(tess, rnmContour.MaterialType, rnmContour));
            }
            
            // モディファイアを適用します。
            // ループ途中で末尾に要素が増えることがあるので、forからforeachへの変更は不可です。
            for (var i = 0; i < tessMesh.SubMeshes.Count; i++)
            {
                var subMesh = tessMesh.SubMeshes[i];
                subMesh.ApplyModifiers(tessMesh);
            }

            // 作った図形を結合してUnityメッシュにします。
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