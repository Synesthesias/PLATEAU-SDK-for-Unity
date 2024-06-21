using System.Collections.Generic;

namespace PLATEAU.Util
{
    public static class ListUtil
    {
        /// <summary>
        /// return index &gt;= 0 and index &lt; self.Count;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IndexIn<T>(this IList<T> self, int index)
        {
            return index >= 0 && index < self.Count;
        }
    }
}