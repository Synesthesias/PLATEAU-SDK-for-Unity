using System;
using System.Collections.Generic;
using System.Linq;

namespace PLATEAU.Util
{
    public static class EnumerableUtil
    {
        private static bool TryFindCompare<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, Func<TResult, TResult, bool> compare, out TSource minElement)
        {
            var ret = self.Aggregate(new Tuple<bool, TSource, TResult>(false, default, default),
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

        private static TResult FindOr<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector,
            Func<TResult, TResult, bool> compare, TResult defaultValue)
        {
            var hasValue = false;
            TResult res = defaultValue;
            foreach (var elem in self)
            {
                var v = selector(elem);
                if (hasValue == false || compare(v, res))
                {
                    res = v;
                    hasValue = true;
                }
            }

            return res;
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
        /// selectorで指定した値が最小となる要素を返す(存在しない場合はdefaultValueを返す)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="selector"></param>
        /// <param name="defaultValue"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static TResult FindMinOr<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, TResult defaultValue, IComparer<TResult> compare = null)
        {
            compare ??= Comparer<TResult>.Default;
            return self.FindOr(selector, (v1, v2) => compare.Compare(v1, v2) < 0, defaultValue);
        }

        /// <summary>
        /// selectorで指定した値が最大となる要素を返す(存在しない場合はdefaultValueを返す)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="selector"></param>
        /// <param name="defaultValue"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static TResult FindMaxOr<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector, TResult defaultValue, IComparer<TResult> compare = null)
        {
            compare ??= Comparer<TResult>.Default;
            return self.FindOr(selector, (v1, v2) => compare.Compare(v1, v2) > 0, defaultValue);
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