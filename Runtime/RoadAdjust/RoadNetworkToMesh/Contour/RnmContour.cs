using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから導かれる輪郭線であり、多角形を構成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    [Serializable]
    public class RnmContour : IEnumerable<RnmVertex>
    {
        private List<RnmVertex> vertices = new ();
        public RnmMaterialType MaterialType { get; set; }
        
        public RnmContour(IEnumerable<RnmVertex> vertices, RnmMaterialType material) : this(material)
        {
            this.vertices = vertices.ToList();
        }

        public RnmContour(RnmMaterialType material)
        {
            this.MaterialType = material;
        }


        public int Count => vertices.Count;

        public RnmVertex this[int index]
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
        public void AddVertices(IEnumerable<RnmVertex> v) => vertices.AddRange(v);

        /// <summary>時計回りならtrue、反時計回りならfalseを返します。 </summary>
        public bool IsClockwise()
        {
            if (Count < 3) throw new ArgumentException("頂点数が足りません");
            float sum = 0;
            for (int i = 0; i < Count; i++)
            {
                var v1 = vertices[i];
                var v2 = vertices[(i + 1) % Count];
                sum += (v2.Position.x - v1.Position.x) * (v2.Position.z + v1.Position.z);
            }

            return sum > 0;
        }

        public void Reverse() => vertices.Reverse();
        
        public IEnumerator<RnmVertex> GetEnumerator()
        {
            return vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}