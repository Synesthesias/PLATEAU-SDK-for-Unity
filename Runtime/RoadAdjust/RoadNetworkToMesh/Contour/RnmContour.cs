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
    internal class RnmContour : IEnumerable<RnmVertex>
    {
        public List<RnmVertex> Vertices { get; private set; }= new ();
        public RnmMaterialType MaterialType { get; set; }
        public List<IRnmTessModifier> TessModifiers { get; private set; } = new ();
        
        public RnmContour(IEnumerable<RnmVertex> vertices, RnmMaterialType material) : this(material)
        {
            this.Vertices = vertices.ToList();
        }

        public RnmContour(RnmMaterialType material)
        {
            this.MaterialType = material;
        }

        public RnmContour CopyWithoutModifier()
        {
            return new RnmContour(Vertices, MaterialType);
        }

        public int Count => Vertices.Count;

        public RnmVertex this[int index]
        {
            get
            {
                return Vertices[index];
            }

            set
            {
                Vertices[index] = value;
            }
        }
        public void AddVertices(IEnumerable<RnmVertex> v) => Vertices.AddRange(v);
        
        public void AddModifier(IRnmTessModifier modifier) => TessModifiers.Add(modifier);

        /// <summary>時計回りならtrue、反時計回りならfalseを返します。 </summary>
        public bool IsClockwise()
        {
            if (Count < 3) throw new ArgumentException("頂点数が足りません");
            float sum = 0;
            for (int i = 0; i < Count; i++)
            {
                var v1 = Vertices[i];
                var v2 = Vertices[(i + 1) % Count];
                sum += (v2.Position.x - v1.Position.x) * (v2.Position.z + v1.Position.z);
            }

            return sum > 0;
        }

        public void Reverse() => Vertices.Reverse();
        
        public IEnumerator<RnmVertex> GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public enum NormalAxis{Y}
    }
}