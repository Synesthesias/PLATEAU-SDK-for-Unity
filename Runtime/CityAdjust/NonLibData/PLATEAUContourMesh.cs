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
        public RnmContourMesh contourMesh;

        public void Init(RnmContourMesh contour)
        {
            contourMesh = contour;
        }
    }

    [Serializable]
    public class RnmContourMesh
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv4;
        
        public RnmContourMesh(Vector3[] vertices, int[] triangles, Vector2[] uv4)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uv4 = uv4;
        }
    }
}