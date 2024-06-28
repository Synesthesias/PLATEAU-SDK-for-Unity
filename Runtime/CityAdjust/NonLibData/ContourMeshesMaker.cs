using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Util;
using System.Collections.Generic;
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
        private Dictionary<NonLibKeyParent, ContourMesh> data = new();
        
        /// <summary> 道路の頂点を記憶します。 </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                var contour = trans.GetComponent<PLATEAUContourMesh>();
                if (contour != null)
                {
                    data.Add(new NonLibKeyParent(trans), contour.contourMesh);
                    return NextSearchFlow.Continue;
                }
                var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                if (cog == null) return NextSearchFlow.Continue;
                if (cog.Package != PredefinedCityModelPackage.Road) return NextSearchFlow.Continue;
                var mf = trans.GetComponent<MeshFilter>();
                if (mf == null) return NextSearchFlow.Continue;
                var mesh = mf.sharedMesh;
                if (mesh == null) return NextSearchFlow.Continue;
                var key = new NonLibKeyParent(trans);
                data.Add(key, new ContourMesh(mesh.vertices, mesh.triangles));
                return NextSearchFlow.Continue;
            });
        }

        /// <summary> 道路の頂点を復元します。 </summary>
        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                if (data.TryGetValue(new NonLibKeyParent(trans), out var vertices))
                {
                    var contour = trans.gameObject.AddComponent<PLATEAUContourMesh>();
                    contour.Init(vertices);
                }
                return NextSearchFlow.Continue;
            });
        }
    }

    /// <summary> 変換の前後でゲームオブジェクトを特定するための情報であり、ゲームオブジェクト名とその親の名前で構成 </summary>
    internal class NonLibKeyParent
    {
        public string ObjName { get; }
        public string ParentName { get; }

        public NonLibKeyParent(Transform obj)
        {
            ObjName = obj.name;
            ParentName = obj.parent == null ? "" : obj.parent.name;
        }

        public override bool Equals(object obj)
        {
            if (obj is NonLibKeyParent other)
            {
                return ObjName == other.ObjName && ParentName == other.ParentName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (ParentName + ObjName).GetHashCode();
        }
    }
}