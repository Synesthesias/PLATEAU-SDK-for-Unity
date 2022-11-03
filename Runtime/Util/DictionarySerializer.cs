using System;
using System.Collections.Generic;

namespace PLATEAU.Util
{
    /// <summary>
    /// Dictionary をシリアライズするための機能を提供します。
    /// Unityの機能では通常では Dictionary はシリアライズできませんが、
    /// ISerializationCallbackReceiver を実装してシリアライズ時に List に変換し、デシリアライズ時に Dictionary に戻すことで Dictionary の保存が可能です。
    /// </summary>
    internal static class DictionarySerializer
    {
        /// <summary>
        /// シリアライズするときに List形式に直します。
        /// </summary>
        public static void OnBeforeSerialize<TKey, TValue>(
            Dictionary<TKey, TValue> dict,
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
        public static Dictionary<TKey, TValue> OnAfterSerialize<TKey, TValue>(
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