using PLATEAU.RoadNetwork.Data;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class RoadNetworkPoint
    {
        [field: SerializeField] public Vector3 Vertex { get; set; }

        public RoadNetworkPoint(Vector3 val)
        {
            Vertex = val;
        }

        public RoadNetworkPoint() { }

        // Vector3型への暗黙の型変換
        public static implicit operator Vector3(RoadNetworkPoint id) => id.Vertex;
    }
}