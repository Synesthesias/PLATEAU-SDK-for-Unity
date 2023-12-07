using System.Collections.Generic;
using PLATEAU.PolygonMesh;
using UnityEngine;
using UnityEngine.Assertions;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// UnityのMeshから共通ライブラリのSubMeshを構築する機能を抽象化したものです。
    /// なぜ抽象化する必要があるかというと、SubMeshの構築方法にはいくつか種類があるからです。
    /// 具体的には、
    /// エクスポート時にテクスチャを含む設定であれば、テクスチャパスをSubMeshに含めることになります。
    /// エクスポート時にテクスチャを含まない設定であれば、空のSubMeshを用意することになります。
    /// 結合分割時は、ゲームエンジンのマテリアルをIDに変換してSubMeshに含めることになります。
    /// </summary>
    public interface IUnityMeshToDllSubMeshConverter
    {
        List<SubMesh> Convert(Mesh unityMesh, Renderer renderer);

        delegate SubMesh ForEachUnitySubMeshToDllSubMesh(int startIndex, int endIndex, Material material);

        
        /// <summary>
        /// 変換に使う共通機能です。
        /// Unityの各SubMeshに対して、<paramref name="predicate"/>でDllSubMeshを追加し、そのリストを返します。
        /// </summary>
        public static List<SubMesh> ForEachUnitySubMesh(Mesh unityMesh, Renderer renderer, ForEachUnitySubMeshToDllSubMesh predicate)
        {
            var indices = unityMesh.triangles;
            int subMeshCount = unityMesh.subMeshCount;
            var dllSubMeshes = new List<PolygonMesh.SubMesh>();
            Material[] materials = null;
            if (renderer != null) materials = renderer.sharedMaterials;
            for (int i = 0; i < subMeshCount; i++)
            {
                var unitySubMesh = unityMesh.GetSubMesh(i);
                int startIndex = unitySubMesh.indexStart;
                int endIndex = startIndex + unitySubMesh.indexCount - 1;
                if (startIndex >= endIndex) continue;
                Assert.IsTrue(startIndex < endIndex);
                Assert.IsTrue(endIndex < indices.Length);
                Assert.IsTrue(startIndex < indices.Length);
                Assert.IsTrue(0 <= startIndex);

                var dllSubMesh = predicate(startIndex, endIndex, materials?[i]);
                
                dllSubMeshes.Add(dllSubMesh);
                
            }

            return dllSubMeshes;
        }
    }
}