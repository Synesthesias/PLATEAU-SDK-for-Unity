using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public struct RoadNetworkPrimID<_PrimDataType>
        where _PrimDataType : struct, IPrimitiveData
    {
        public int id;
        bool IsValid {  get => id >= 0; }
    }

    public interface IPrimitiveData
    {

    }

    [Serializable]
    public struct Point : IPrimitiveData
    {
        public Vector3 value;
    }

    [Serializable]
    public struct LineStrings : IPrimitiveData
    {
        public List<RoadNetworkPrimID<Point>> points;

        //public Point this[int index]
        //{
        //    get => Points[index];
        //    set => Points[index] = value;
        //}

        //public IEnumerator<Point> GetEnumerator()
        //{
        //    for (int i = 0; i < Points.Length; i++)
        //    {
        //        yield return this[i];
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }


}
