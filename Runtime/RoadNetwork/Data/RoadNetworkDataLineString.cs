using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork.Data
{

    public interface ILineString : IPrimitiveData, ICollection<RnId<RoadNetworkDataPoint>>
    {

    }

    // 線分を表す
    [Serializable]
    public class RoadNetworkDataLineString : ILineString
    {
        public List<RnId<RoadNetworkDataPoint>> points;

        public int Count => ((ICollection<RnId<RoadNetworkDataPoint>>)points).Count;

        public bool IsReadOnly => ((ICollection<RnId<RoadNetworkDataPoint>>)points).IsReadOnly;

        public void Add(RnId<RoadNetworkDataPoint> item)
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)points).Clear();
        }

        public bool Contains(RnId<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnId<RoadNetworkDataPoint>>)points).Contains(item);
        }

        public void CopyTo(RnId<RoadNetworkDataPoint>[] array, int arrayIndex)
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RnId<RoadNetworkDataPoint>> GetEnumerator()
        {
            return ((IEnumerable<RnId<RoadNetworkDataPoint>>)points).GetEnumerator();
        }

        public bool Remove(RnId<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnId<RoadNetworkDataPoint>>)points).Remove(item);
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