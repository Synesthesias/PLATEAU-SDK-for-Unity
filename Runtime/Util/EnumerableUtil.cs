using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.Util
{
    public static class EnumerableUtil
    {
        private static bool TryFindCompare<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, Func<TResult, TResult, bool> compare, out TSource minElement)
        {
            var ret = self.Aggregate(new Tuple<bool, TSource, TResult>(false, default(TSource), default(TResult)),
                (a, elem) =>
                {
                    if (a.Item1 == false)
                        return new Tuple<bool, TSource, TResult>(true, elem, selector(elem));
                    var v = selector(elem);
                    if (compare(v, a.Item3))
                        return new Tuple<bool, TSource, TResult>(true, elem, v);
                    return a;
                });
            minElement = ret.Item2;
            return ret.Item1;
        }

        public static bool TryFindMin<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, out TSource minElement, IComparer<TResult> compare = null)
        {
            compare ??= Comparer<TResult>.Default;
            return self.TryFindCompare(selector, (v1, v2) => compare.Compare(v1, v2) < 0, out minElement);
        }

        public static bool TryFindMax<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, out TSource minElement, IComparer<TResult> compare = null)
        {
            compare ??= Comparer<TResult>.Default;
            return self.TryFindCompare(selector, (v1, v2) => compare.Compare(v1, v2) > 0, out minElement);
        }

        /// <summary>
        /// selfに対して、最初にpredicateがtrueになる要素のindexを返す
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="self"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int FindFirstIndex<TSource>(this IEnumerable<TSource> self, Func<TSource, bool> predicate)
        {
            var i = 0;
            foreach (var elem in self)
            {
                if (predicate(elem))
                    return i;
                i++;
            }

            return -1;
        }
    }
}