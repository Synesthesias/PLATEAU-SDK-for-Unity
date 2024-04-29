using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    public interface IPoint : IPrimitiveData
    {

    }

    [Serializable]
    public struct RoadNetworkPoint : IPrimitiveData
    {
        public RoadNetworkPoint(Vector3 val)
        {
            value = val;
        }
        public Vector3 value;
    }


}