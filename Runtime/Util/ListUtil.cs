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

        /// <summary>
        /// compareで比較して最小のindexを返す.同値のものがある場合は最初のものを返す.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static int FindMinIndex<T>(this IReadOnlyList<T> self, Comparer<T> compare)
        {
            var ret = -1;
            for (var i = 0; i < self.Count; i++)
            {
                if (ret == -1 || compare.Compare(self[i], self[ret]) < 0)
                    ret = i;
            }

            return ret;
        }

        /// <summary>
        /// LinqのReverseは遅いので自前で実装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<T> Reversed<T>(this IList<T> self)
        {
            for (var i = self.Count - 1; i >= 0; i--)
                yield return self[i];
        }
    }
}