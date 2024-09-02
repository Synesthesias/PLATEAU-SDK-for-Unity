using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnPoint : ARnParts<RnPoint>
    {
        [field: SerializeField] public Vector3 Vertex { get; set; }

        public RnPoint(Vector3 val)
        {
            Vertex = val;
        }

        public RnPoint() { }

        // Vector3型への暗黙の型変換
        public static implicit operator Vector3(RnPoint id) => id.Vertex;

        /// <summary>
        /// コピー作成
        /// </summary>
        /// <returns></returns>
        public RnPoint Clone()
        {
            return new RnPoint(Vertex);
        }

        /// <summary>
        /// x/yが同じかどうか(参照一致だけでなく値一致でもtrue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sqrMagnitudeTolerance">２点間の一致判定となる２乗距離</param>
        /// <returns></returns>
        public static bool Equals(RnPoint x, RnPoint y, float sqrMagnitudeTolerance = 0f)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }
            return (x.Vertex - y.Vertex).sqrMagnitude <= sqrMagnitudeTolerance;
        }
    }
}