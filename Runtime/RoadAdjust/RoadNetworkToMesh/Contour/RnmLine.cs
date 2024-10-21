using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
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
            return IsSameWith(other) || IsSameWith(reverse);
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