using System;
using System.Collections.Generic;

namespace PLATEAU.Util
{
    public static class DirectoryUtil
    {
        /// <summary>
        /// TryGetValueして存在しない場合にfactoryで新規オブジェクトを生成して追加したうえで返す
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
        {
            if (self.TryGetValue(key, out TValue v))
                return v;
            var tmp = factory(key);
            self[key] = tmp;
            return tmp;
        }

        /// <summary>
        /// TryGetValueして存在しない場合にnew TValue()で生成して追加したうえで返す
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
            where TValue : new()
        {
            return self.GetValueOrCreate(key, k => new TValue());
        }
    }
}