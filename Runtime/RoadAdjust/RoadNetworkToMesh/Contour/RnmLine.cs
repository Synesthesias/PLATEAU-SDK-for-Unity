using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    public struct RnmVertex
    {
        public Vector3 Position { get; }
        public Vector2 UV1 { get; }

        public RnmVertex(Vector3 position, Vector2 uv1)
        {
            Position = position;
            UV1 = uv1;
        }

        public RnmVertex(RnmVertex v) : this(v.Position, v.UV1)
        {
        }
    }
    
    /// <summary> 線1つに計算用のデータを付与したデータ構造です。 </summary>
    internal class RnmLine : IReadOnlyList<RnmVertex>
    {
        public RnmVertex[] Vertices { get;}
        
        public bool IsProcessed { get; set; }
            
        public RnmLine(IEnumerable<Vector3> vertexPositions, Vector2 startUV1, Vector2 endUV1)
        {
            var positions = vertexPositions.ToArray();
            var uv1 = UV1ByEndPoint(startUV1, endUV1, positions);
            Vertices = new RnmVertex[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                Vertices[i] = new RnmVertex(positions[i], uv1[i]);
            }
        }
        
        public RnmLine(IEnumerable<RnmVertex> vertices)
        {
            Vertices = vertices.ToArray();
        }
        
        public RnmVertex this[int index] => Vertices[index];
        public int Count => Vertices.Length;

        /// <summary>
        /// 線の両端のUV1を受け取り、その間を端からの補間で埋めたUV1配列を返します。
        /// </summary>
        private static Vector2[] UV1ByEndPoint(Vector2 startUV1, Vector2 endUV1, Vector3[] positions)
        {
            var UV1 = new Vector2[positions.Length];
            if (positions.Length == 0) return new Vector2[]{};
            float sum = SumDistance(positions);
            float dist = 0;
            UV1[0] = startUV1;
            for (int i = 1; i < positions.Length; i++)
            {
                dist += Vector3.Distance(positions[i - 1], positions[i]);
                float t = dist / sum;
                UV1[i] = Vector2.Lerp(startUV1, endUV1, t);
            }

            return UV1;
        }

        private static float SumDistance(Vector3[] positions)
        {
            float sum = 0;
            for (int i = 1; i < positions.Length; i++)
            {
                sum += Vector3.Distance(positions[i - 1], positions[i]);
            }

            return sum;
        }

        /// <summary> 線が一致するかどうかです。位置のみ考慮します。UV1は顧慮しません。 </summary>
        private bool IsSameWith(RnmLine other)
        {
            if (this.Count != other.Count) return false;
            for (int i = 0; i < this.Count; i++)
            {
                if (Vector3.Distance(this[i].Position, other[i].Position) > 0.01f) return false;
            }

            return true;
        }

        private bool IsSameWithReverse(RnmLine other)
        {
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (Vector3.Distance(this[i].Position, other[Count - i - 1].Position) > 0.01f) return false;
            }

            return true;
        }

        private bool IsSameWithReverse(RnmLine other)
        {
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (Vector3.Distance(this[i], other[Count - i - 1]) > 0.01f) return false;
            }

            return true;
        }

        /// <summary> 線が一致する、または順番を逆転させたら一致する </summary>
        public bool IsSameOrReverseWith(RnmLine other)
        {
            return IsSameWith(other) || IsSameWithReverse(other);
        }
        
        /// <summary>
        /// この線の頂点群のうち、 <paramref name="subtract"/> のいずれかと同じ位置にある点を除外し、
        /// 除外したところで線を分けた線群を返します。
        /// 同じ位置とみなす距離のしきい値を<paramref name="distThreshold"/>で指定します。
        /// </summary>
        public IEnumerable<RnmLine> SubtractSeparate(IEnumerable<Vector3> subtract,
            float distThreshold)
        {
            var nextLine = new List<RnmVertex>();
            foreach (var baseV in Vertices)
            {
                bool shouldSubtract = false;
                foreach (var subV in subtract)
                {
                    if (Vector3.Distance(baseV.Position, subV) < distThreshold)
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
                        nextLine.Add(new RnmVertex(baseV)); // 切り替え時に1点追加したほうが自然
                        yield return new RnmLine(nextLine);
                    }
                    nextLine.Clear();
                }
                else // subtractにマッチしない部分。ここは使います。
                {
                    nextLine.Add(new RnmVertex(baseV));
                }
            }

            if (nextLine.Count >= 2)
            {
                yield return new RnmLine(nextLine);
            }
        }

        public IEnumerator<RnmVertex> GetEnumerator()
        {
            return ((IEnumerable<RnmVertex>)Vertices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Vertices.GetEnumerator();
        }
    }

}