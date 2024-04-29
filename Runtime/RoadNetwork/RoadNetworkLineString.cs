using System;
using System.Collections;
using System.Collections.Generic;

namespace PLATEAU.RoadNetwork
{

    public interface ILineString : IPrimitiveData, ICollection<RnPointId>
    {

    }

    // 線分を表す
    [Serializable]
    public class RoadNetworkLineString : ILineString
    {
        public List<RnPointId> points;

        public int Count => ((ICollection<RnPointId>)points).Count;

        public bool IsReadOnly => ((ICollection<RnPointId>)points).IsReadOnly;

        public void Add(RnPointId item)
        {
            ((ICollection<RnPointId>)points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RnPointId>)points).Clear();
        }

        public bool Contains(RnPointId item)
        {
            return ((ICollection<RnPointId>)points).Contains(item);
        }

        public void CopyTo(RnPointId[] array, int arrayIndex)
        {
            ((ICollection<RnPointId>)points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RnPointId> GetEnumerator()
        {
            return ((IEnumerable<RnPointId>)points).GetEnumerator();
        }

        public bool Remove(RnPointId item)
        {
            return ((ICollection<RnPointId>)points).Remove(item);
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