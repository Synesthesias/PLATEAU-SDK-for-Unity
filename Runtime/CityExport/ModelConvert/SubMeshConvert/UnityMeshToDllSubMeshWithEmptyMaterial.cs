using System.Collections.Generic;
using PLATEAU.PolygonMesh;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// UnityのMeshから、見た目情報が空であるSubMeshを生成します。
    /// 特に、エクスポートでテクスチャを含めない設定にしたときに利用されます。
    /// </summary>
    public class UnityMeshToDllSubMeshWithEmptyMaterial : IUnityMeshToDllSubMeshConverter
    {
        public List<SubMesh> Convert(Mesh unityMesh, Renderer renderer)
        {
            // 空のSubMeshを1つ生成します。
            var indices = unityMesh.triangles;
            return new List<SubMesh>
            {
                SubMesh.Create(0, indices.Length - 1, "")
            };
        }
    }
}