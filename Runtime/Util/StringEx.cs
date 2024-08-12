using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.Util
{
    public static class StringEx
    {
        /// <summary>
        /// string.Joinの拡張メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join2String<T>(this IEnumerable<T> self, string separator = ",")
        {
            return string.Join(separator, self);
        }

        /// <summary>
        /// string.IsNullOrEmptyの拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        /// <summary>
        /// string.IsNullOrWhiteSpaceの拡張メソッド
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }
    }
}