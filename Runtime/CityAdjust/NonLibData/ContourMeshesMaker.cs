using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// 道路の分割前の頂点を覚えておきます。
    /// これは高さ合わせ後に道路ネットワークを生成するために必要です。
    /// </summary>
    internal class ContourMeshesMaker : INonLibData
    {
        // 記録用の辞書であり、Valueは頂点座標です。
        private NonLibDictionary<ContourMesh> data = new();
        
        /// <summary> 道路の頂点を記憶します。 </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                var contour = trans.GetComponent<PLATEAUContourMesh>();
                if (contour != null)
                {
                    data.Add(trans, src.Get.ToArray(), contour.contourMesh);
                    return NextSearchFlow.Continue;
                }
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return NextSearchFlow.Continue;
                if (cog.Package != PredefinedCityModelPackage.Road) return NextSearchFlow.Continue;
                var mf = trans.GetComponent<MeshFilter>();
                if (mf == null) return NextSearchFlow.Continue;
                var mesh = mf.sharedMesh;
                if (mesh == null) return NextSearchFlow.Continue;
                data.Add(trans, src.Get.ToArray(), new ContourMesh(mesh.vertices, mesh.triangles, mesh.uv4));
                
                return NextSearchFlow.Continue; 
            });
        }

        /// <summary> 道路の頂点を復元します。 </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                var vertices = data.GetNonRestoredAndMarkRestored(trans, target.Get.ToArray());
                if (vertices != null)
                {
                    var contour = trans.gameObject.AddComponent<PLATEAUContourMesh>();
                    contour.Init(vertices);
                }
                return NextSearchFlow.Continue;
            });
        }
    }
}