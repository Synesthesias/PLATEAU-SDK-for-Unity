using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{
    public interface IPoint : IPrimitiveData
    {

    }

    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkPoint))]
    public class RoadNetworkDataPoint : IPrimitiveData
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkPoint.Vertex))]
        public Vector3 Vertex { get; set; }

        public RoadNetworkDataPoint(Vector3 val)
        {
            Vertex = val;
        }

        public RoadNetworkDataPoint()
        {

        }

    }


}