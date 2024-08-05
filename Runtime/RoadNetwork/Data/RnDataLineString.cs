using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU.RoadNetwork.Data
{

    public interface ILineString : IPrimitiveData, ICollection<RnID<RnDataPoint>>
    {

    }

    // 線分を表す
    [Serializable, RoadNetworkSerializeData(typeof(RnLineString))]
    public class RnDataLineString : ILineString
    {
        [field: SerializeField]
        [RoadNetworkSerializeMember]
        public List<RnID<RnDataPoint>> Points { get; set; } = new List<RnID<RnDataPoint>>();

        public int Count => ((ICollection<RnID<RnDataPoint>>)Points).Count;

        public bool IsReadOnly => ((ICollection<RnID<RnDataPoint>>)Points).IsReadOnly;

        public void Add(RnID<RnDataPoint> item)
        {
            ((ICollection<RnID<RnDataPoint>>)Points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RnID<RnDataPoint>>)Points).Clear();
        }

        public bool Contains(RnID<RnDataPoint> item)
        {
            return ((ICollection<RnID<RnDataPoint>>)Points).Contains(item);
        }

        public void CopyTo(RnID<RnDataPoint>[] array, int arrayIndex)
        {
            ((ICollection<RnID<RnDataPoint>>)Points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RnID<RnDataPoint>> GetEnumerator()
        {
            return ((IEnumerable<RnID<RnDataPoint>>)Points).GetEnumerator();
        }

        public bool Remove(RnID<RnDataPoint> item)
        {
            return ((ICollection<RnID<RnDataPoint>>)Points).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Points).GetEnumerator();
        }
    }

}