using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Util
{
    public static class TransformEx
    {
        /// <summary>
        /// 自分の親をすべて取得する. 近い親から順に取得する
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetParents(this Transform self)
        {
            if (!self)
                yield break;

            var parent = self.parent;
            while (parent)
            {
                yield return parent;
                parent = parent.parent;
            }
        }
    }
}