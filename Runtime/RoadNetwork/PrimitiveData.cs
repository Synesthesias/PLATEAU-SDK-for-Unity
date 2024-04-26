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

    public interface IPoint : IPrimitiveData
    {

    }

    [Serializable]
    public struct Point : IPrimitiveData
    {
        public Point(Vector3 val)
        {
            value = val;
        }
        public Vector3 value;
    }

    public interface ILineStrings : IPrimitiveData, ICollection<RoadNetworkPrimID<Point>>
    {

    }

    [Serializable]
    public struct LineStrings : ILineStrings
    {
        public List<RoadNetworkPrimID<Point>> points;

        public int Count => ((ICollection<RoadNetworkPrimID<Point>>)points).Count;

        public bool IsReadOnly => ((ICollection<RoadNetworkPrimID<Point>>)points).IsReadOnly;

        public void Add(RoadNetworkPrimID<Point> item)
        {
            ((ICollection<RoadNetworkPrimID<Point>>)points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RoadNetworkPrimID<Point>>)points).Clear();
        }

        public bool Contains(RoadNetworkPrimID<Point> item)
        {
            return ((ICollection<RoadNetworkPrimID<Point>>)points).Contains(item);
        }

        public void CopyTo(RoadNetworkPrimID<Point>[] array, int arrayIndex)
        {
            ((ICollection<RoadNetworkPrimID<Point>>)points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RoadNetworkPrimID<Point>> GetEnumerator()
        {
            return ((IEnumerable<RoadNetworkPrimID<Point>>)points).GetEnumerator();
        }

        public bool Remove(RoadNetworkPrimID<Point> item)
        {
            return ((ICollection<RoadNetworkPrimID<Point>>)points).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)points).GetEnumerator();
        }

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
