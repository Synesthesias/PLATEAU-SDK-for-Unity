using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// HashSetがシリアライズ化できないので仮で用意
    /// </summary>
    [Serializable]
    public class PLATEAURoadNetworkIndexMap : IEnumerable<int>
    {
        [SerializeField] private List<int> body = new List<int>();

        public void Add(int item)
        {
            // BinarySeasonの仕様として見つからない場合、itemの次に大きい要素のインデックスのビットごとの補数
            // https://learn.microsoft.com/ja-jp/dotnet/api/system.collections.generic.list-1.binarysearch?view=netframework-4.8
            var index = body.BinarySearch(item);
            if (index >= 0)
                return;
            // indexを反転させてその位置に入れる
            body.Insert(~index, item);
        }

        public void Clear()
        {
            body.Clear();
        }

        public bool Contains(int item)
        {
            return body.BinarySearch(item) >= 0;
        }

        public bool Remove(int item)
        {
            var index = body.BinarySearch(item);
            if (index < 0)
                return false;

            body.RemoveAt(index);
            return true;
        }

        public int Count => body.Count;

        public int IndexOf(int item)
        {
            var index = body.BinarySearch(item);
            // List<T>.IndexOfに合わせて見つからない場合は-1
            // Max(index, -1)でも良さそうだけど可読性優先
            if (index < 0)
                return -1;
            return index;
        }

        public void RemoveAt(int index)
        {
            body.RemoveAt(index);
        }

        public int this[int index]
        {
            get => body[index];
        }

        public IEnumerator<int> GetEnumerator()
        {
            return body.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int RemoveAll(System.Predicate<int> match)
        {
            return body.RemoveAll(match);
        }

        public void AddRange(IEnumerable<int> collection)
        {
            body.AddRange(collection);
        }
    }

}