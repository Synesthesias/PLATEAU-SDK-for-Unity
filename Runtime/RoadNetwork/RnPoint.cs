using PLATEAU.RoadNetwork.Data;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
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
    }
}