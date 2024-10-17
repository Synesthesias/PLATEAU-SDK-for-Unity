using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから導かれる輪郭線であり、多角形を構成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    [Serializable]
    public class RnmContour : IEnumerable<Vector3>
    {
        [SerializeField] private List<Vector3> vertices = new ();
        
        public RnmContour(IEnumerable<Vector3> vertices)
        {
            this.vertices = vertices.ToList();
        }
        
        public RnmContour(){}


        public int Count => vertices.Count;

        public Vector3 this[int index]
        {
            get
            {
                return vertices[index];
            }

            set
            {
                vertices[index] = value;
            }
        }
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

        /// <summary>
        /// 輪郭線を<paramref name="diff"/>メートルだけ縮めます。
        /// </summary>
        public void Shrink(float diff)
        {
            if (Count <= 2) return;

            var center = Vector3.zero;
            foreach (var v in vertices)
            {
                center += v;
            }
            center /= Count;


            for (int i = 0; i < Count; i++)
            {
                vertices[i] += (center - vertices[i]).normalized * diff;
            }
            
        }

        
        public IEnumerator<Vector3> GetEnumerator()
        {
            return vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// <see cref="RnmContour"/>を複数保持して1つのメッシュに相当するものです。
    /// </summary>
    [Serializable]
    internal class RnmContourMesh : IEnumerable<RnmContour>
    {
        [SerializeField] private List<RnmContour> contours = new();
        [SerializeField] private GameObject sourceObject;

        public GameObject SourceObject => sourceObject;
        
        public RnmContourMesh(GameObject sourceObject) { this.sourceObject = sourceObject; }

        public RnmContourMesh(GameObject sourceObject, IEnumerable<RnmContour> contours)
        :this(sourceObject)
        {
            this.contours = contours.ToList();
        }

        public RnmContourMesh(GameObject sourceObject, RnmContour contour)
        :this(sourceObject)
        {
            this.contours = new List<RnmContour> { contour };
        }
        
        public int Count => contours.Count;
        public RnmContour this[int index] => contours[index];
        public void Add(RnmContour c) => contours.Add(c);

        public void AddRange(RnmContourMesh c)
        {
            foreach (var contour in c.contours) Add(contour);
        }
        
        
        public IEnumerator<RnmContour> GetEnumerator() => contours.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        
    }
    
    /// <summary> <see cref="RnmContourMesh"/>を複数保持します。 </summary>
    [Serializable]
    internal class RnmContourMeshList : IEnumerable<RnmContourMesh>
    {
        [SerializeField] private List<RnmContourMesh> meshes = new();
        public int Count => meshes.Count;

        public RnmContourMeshList(){}

        public RnmContourMeshList(IEnumerable<RnmContourMesh> contourMeshes)
        {
            this.meshes = contourMeshes.ToList();
        }
        
        public RnmContourMesh this[int index] => meshes[index];
        public void Add(RnmContourMesh c) => meshes.Add(c);

        public void AddRange(RnmContourMeshList c)
        {
            foreach (var cMesh in c.meshes) Add(cMesh);
        }

        public void ShrinkContours(float diff)
        {
            foreach (var cMesh in meshes)
            {
                foreach(var contour in cMesh)
                {
                    contour.Shrink(diff);
                }
            }
        }
        

        public IEnumerator<RnmContourMesh> GetEnumerator() => meshes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    /// <summary> 線1つに計算用のデータを付与したデータ構造です。 </summary>
    internal class RnmLine : IReadOnlyList<Vector3>
    {
        public Vector3[] Vertices { get; }
        public bool IsProcessed { get; set; }
            
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
        
        /// <summary>
        /// <paramref name="baseLines"/> の頂点群のうち、 <paramref name="subtract"/> のいずれかと同じ位置にある点を除外し、
        /// 除外したところで線を分けた線群を返します。
        /// 同じ位置とみなす距離のしきい値を<paramref name="distThreshold"/>で指定します。
        /// </summary>
        public IEnumerable<RnmLine> SubtractSeparate(IEnumerable<Vector3> subtract,
            float distThreshold)
        {
            var nextLine = new List<Vector3>();
            foreach (var baseV in Vertices)
            {
                bool shouldSubtract = false;
                foreach (var subV in subtract)
                {
                    if (Vector3.Distance(baseV, subV) < distThreshold)
                    {
                        shouldSubtract = true;
                        break;
                    }
                }

                if (shouldSubtract)
                {
                    // baseの線が切り替わるタイミングで線を分けます。
                    if (nextLine.Count >= 2)
                    {
                        nextLine.Add(new RnPoint(baseV)); // 切り替え時に1点追加したほうが自然
                        yield return new RnmLine(nextLine);
                    }
                    nextLine.Clear();
                }
                else // subtractにマッチしない部分。ここは使います。
                {
                    nextLine.Add(new RnPoint(baseV));
                }
            }

            if (nextLine.Count >= 2)
            {
                yield return new RnmLine(nextLine);
            }
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return ((IEnumerable<Vector3>)Vertices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }
    }
}