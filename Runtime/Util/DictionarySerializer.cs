using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    public static class DictionarySerializer
    {
        /// <summary>
        /// シリアライズするときに List形式に直します。
        /// </summary>
        public static void OnBeforeSerialize<TKey,TValue>(
            Dictionary<TKey,TValue> dict,
            List<TKey> keys,
            List<TValue> values
            )
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in dict)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        /// <summary>
        /// デシリアライズするときに List から Dictionary 形式に直します。
        /// </summary>
        public static Dictionary<TKey,TValue> OnAfterSerialize<TKey, TValue>(
            List<TKey> keys,
            List<TValue> values
        )
        {
            var dict = new Dictionary<TKey, TValue>();
            int cnt = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < cnt; i++)
            {
                dict.Add(keys[i], values[i]);
            }

            return dict;
        }
    }
}