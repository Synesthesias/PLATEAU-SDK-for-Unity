using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    public interface IPoint : IPrimitiveData
    {

    }

    [Serializable, RoadNetworkSerializeData(typeof(RnPoint))]
    public class RnDataPoint : IPrimitiveData
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RnPoint.Vertex))]
        public Vector3 Vertex { get; set; }

        public RnDataPoint(Vector3 val)
        {
            Vertex = val;
        }

        public RnDataPoint()
        {

        }

    }


}