using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
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