using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{

    public interface ILineString : IPrimitiveData, ICollection<RnId<RoadNetworkDataPoint>>
    {

    }

    // 線分を表す
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkLineString))]
    public class RoadNetworkDataLineString : ILineString
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember(nameof(RoadNetworkLineString.Points))]
        public List<RnId<RoadNetworkDataPoint>> Points { get; set; } = new List<RnId<RoadNetworkDataPoint>>();

        public int Count => ((ICollection<RnId<RoadNetworkDataPoint>>)Points).Count;

        public bool IsReadOnly => ((ICollection<RnId<RoadNetworkDataPoint>>)Points).IsReadOnly;

        public void Add(RnId<RoadNetworkDataPoint> item)
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)Points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)Points).Clear();
        }

        public bool Contains(RnId<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnId<RoadNetworkDataPoint>>)Points).Contains(item);
        }

        public void CopyTo(RnId<RoadNetworkDataPoint>[] array, int arrayIndex)
        {
            ((ICollection<RnId<RoadNetworkDataPoint>>)Points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RnId<RoadNetworkDataPoint>> GetEnumerator()
        {
            return ((IEnumerable<RnId<RoadNetworkDataPoint>>)Points).GetEnumerator();
        }

        public bool Remove(RnId<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnId<RoadNetworkDataPoint>>)Points).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Points).GetEnumerator();
        }
    }

}