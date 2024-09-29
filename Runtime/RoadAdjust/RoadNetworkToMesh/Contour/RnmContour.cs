using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから導かれる輪郭線であり、多角形を構成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    [Serializable]
    public class RnmContour
    {
        [SerializeField] private List<Vector3> vertices = new ();
        
        public RnmContour(IEnumerable<Vector3> vertices)
        {
            this.vertices = vertices.ToList();
        }
        
        public RnmContour(){}

        public int Count => vertices.Count;
        public Vector3 this[int index] => vertices[index];
        public void AddVertices(IEnumerable<Vector3> v) => vertices.AddRange(v);

        /// <summary>時計回りならtrue、反時計回りならfalseを返します。 </summary>
        public bool IsClockwise()
        {
            if (Count < 3) throw new ArgumentException("頂点数が足りません");
            float sum = 0;
            for (int i = 0; i < Count; i++)
            {
                var v1 = vertices[i];
                var v2 = vertices[(i + 1) % Count];
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }

            return sum > 0;
        }

        public void Reverse() => vertices.Reverse();
    }

    /// <summary> <see cref="RnmContour"/>を複数保持します。 </summary>
    [Serializable]
    public class RnmContourList : IEnumerable<RnmContour>
    {
        [SerializeField] private List<RnmContour> contours = new();
        public int Count => contours.Count;
        public RnmContour this[int index] => contours[index];
        public void Add(RnmContour c) => contours.Add(c);

        public void AddRange(RnmContourList c)
        {
            foreach (var contour in c.contours) Add(contour);
        }

        public IEnumerator<RnmContour> GetEnumerator() => contours.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    /// <summary> 線1つに計算用のデータを付与したデータ構造です。 </summary>
    public class RnmLine
    {
        public Vector3[] Vertices { get; private set; }
        public bool IsProcessed { get; set; } = false;
            
        public RnmLine(IEnumerable<Vector3> vertices)
        {
            Vertices = vertices.ToArray();
        }
            
        public Vector3 this[int index] => Vertices[index];
        public int Count => Vertices.Length;

        /// <summary> 線が一致するかどうかです。 </summary>
        private bool IsSameWith(RnmLine other)
        {
            if (this.Count != other.Count) return false;
            for (int i = 0; i < this.Count; i++)
            {
                if (Vector3.Distance(this[i], other[i]) > 0.01f) return false;
            }

            return true;
        }

        /// <summary> 線が一致する、または順番を逆転させたら一致する </summary>
        public bool IsSameOrReverseWith(RnmLine other)
        {
            var reverse = new RnmLine(Vertices.Reverse());
            return this.IsSameWith(other) || this.IsSameWith(reverse);
        }
    }
}