using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{

    public interface ILineString : IPrimitiveData, ICollection<RnID<RoadNetworkDataPoint>>
    {

    }

    // 線分を表す
    [Serializable, RoadNetworkSerializeData(typeof(RoadNetworkLineString))]
    public class RoadNetworkDataLineString : ILineString
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public List<RnID<RoadNetworkDataPoint>> Points { get; set; } = new List<RnID<RoadNetworkDataPoint>>();

        public int Count => ((ICollection<RnID<RoadNetworkDataPoint>>)Points).Count;

        public bool IsReadOnly => ((ICollection<RnID<RoadNetworkDataPoint>>)Points).IsReadOnly;

        public void Add(RnID<RoadNetworkDataPoint> item)
        {
            ((ICollection<RnID<RoadNetworkDataPoint>>)Points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RnID<RoadNetworkDataPoint>>)Points).Clear();
        }

        public bool Contains(RnID<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnID<RoadNetworkDataPoint>>)Points).Contains(item);
        }

        public void CopyTo(RnID<RoadNetworkDataPoint>[] array, int arrayIndex)
        {
            ((ICollection<RnID<RoadNetworkDataPoint>>)Points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RnID<RoadNetworkDataPoint>> GetEnumerator()
        {
            return ((IEnumerable<RnID<RoadNetworkDataPoint>>)Points).GetEnumerator();
        }

        public bool Remove(RnID<RoadNetworkDataPoint> item)
        {
            return ((ICollection<RnID<RoadNetworkDataPoint>>)Points).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Points).GetEnumerator();
        }
    }

}