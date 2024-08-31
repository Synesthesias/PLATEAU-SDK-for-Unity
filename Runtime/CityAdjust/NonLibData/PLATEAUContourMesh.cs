using System;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// 道路の分割前の頂点を記録するMonoBehaviourです。
    /// これは高さ合わせ後に道路ネットワークを生成するために利用します。
    /// </summary>
    public class PLATEAUContourMesh : MonoBehaviour
    {
        public ContourMesh contourMesh;

        public void Init(ContourMesh contour)
        {
            contourMesh = contour;
        }
    }

    [Serializable]
    public class ContourMesh
    {
        public Vector3[] vertices;
        public int[] triangles;
        
        public ContourMesh(Vector3[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }
}