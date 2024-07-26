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
        private Dictionary<NonLibKeyName, ContourMesh> data = new();
        
        /// <summary> 道路の頂点を記憶します。 </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                var contour = trans.GetComponent<PLATEAUContourMesh>();
                if (contour != null)
                {
                    var contourKey = new NonLibKeyName(trans, src.Get.ToArray());
                    if (!data.TryAdd(contourKey, contour.contourMesh))
                    {
                        Debug.Log($"key {contourKey} is already exists.");
                    }
                    return NextSearchFlow.Continue;
                }
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return NextSearchFlow.Continue;
                if (cog.Package != PredefinedCityModelPackage.Road) return NextSearchFlow.Continue;
                var mf = trans.GetComponent<MeshFilter>();
                if (mf == null) return NextSearchFlow.Continue;
                var mesh = mf.sharedMesh;
                if (mesh == null) return NextSearchFlow.Continue;
                var key = new NonLibKeyName(trans, src.Get.ToArray());
                if (!data.TryAdd(key, new ContourMesh(mesh.vertices, mesh.triangles)))
                {
                    Debug.Log($"key {key} is already exists.");
                }
                
                return NextSearchFlow.Continue;
            });
        }

        /// <summary> 道路の頂点を復元します。 </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                if (data.TryGetValue(new NonLibKeyName(trans, target.Get.ToArray()), out var vertices))
                {
                    var contour = trans.gameObject.AddComponent<PLATEAUContourMesh>();
                    contour.Init(vertices);
                }
                return NextSearchFlow.Continue;
            });
        }
    }
}