using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// HashSetがシリアライズ化できないので仮で用意
    /// 使い方
    /// [Serializable]
    /// class MyHashSet : SerializableHashSet&lt;int&gt;{}
    /// のように使いたいタイプに対して継承して定義し、Serializableを付ける
    /// </summary>
    [Serializable]
    public class SerializableHashSet<T>
        : IEnumerable<T>
    {
        [SerializeField] private List<T> body = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return body.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
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

        public bool Contains(T item)
        {
            return body.BinarySearch(item) >= 0;
        }

        public bool Remove(T item)
        {
            var index = body.BinarySearch(item);
            if (index < 0)
                return false;

            body.RemoveAt(index);
            return true;
        }

        public int Count => body.Count;

        public int IndexOf(T item)
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

        public T this[int index]
        {
            get => body[index];
        }

        public int RemoveAll(System.Predicate<T> match)
        {
            return body.RemoveAll(match);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            body.AddRange(collection);
        }
    }
}