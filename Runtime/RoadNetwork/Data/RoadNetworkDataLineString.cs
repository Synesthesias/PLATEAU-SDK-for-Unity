using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Data
{

    public interface ILineString : IPrimitiveData, ICollection<RoadNetworkID<RoadNetworkPoint>>
    {

    }

    // 線分を表す
    [Serializable]
    public class RoadNetworkDataLineString : ILineString
    {
        public List<RoadNetworkID<RoadNetworkPoint>> points;

        public int Count => ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).Count;

        public bool IsReadOnly => ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).IsReadOnly;

        public void Add(RoadNetworkID<RoadNetworkPoint> item)
        {
            ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).Clear();
        }

        public bool Contains(RoadNetworkID<RoadNetworkPoint> item)
        {
            return ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).Contains(item);
        }

        public void CopyTo(RoadNetworkID<RoadNetworkPoint>[] array, int arrayIndex)
        {
            ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RoadNetworkID<RoadNetworkPoint>> GetEnumerator()
        {
            return ((IEnumerable<RoadNetworkID<RoadNetworkPoint>>)points).GetEnumerator();
        }

        public bool Remove(RoadNetworkID<RoadNetworkPoint> item)
        {
            return ((ICollection<RoadNetworkID<RoadNetworkPoint>>)points).Remove(item);
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