using System;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// シーン上の都市ゲームオブジェクトを列挙します。
    /// </summary>
    [Obsolete]
    public static class CityHierarchyEnumerator
    {
        /// <summary>
        /// <paramref name="parent"/> に対し再帰的にヒエラルキーの子を探索し、 <see cref="CityObject"/> に対応付けできるものをすべて <see cref="CityObject"/> にして
        /// IEnumerable で返します。 <see cref="CityObject"/> 形式で列挙します。
        /// 順番は DFS (深さ優先探索) です。
        /// </summary>
        /// <param name="parent">対象となるゲームオブジェクトの Transform です。</param>
        /// <param name="minDepth">これより小さい深さは無視します。0は <paramref name="parent"/> 自身、1は子、2は孫です。</param>
        /// <param name="cityModelLoader">都市情報のロードに使います。</param>
        /// <param name="metadata">都市情報のロードに使います。</param>
        /// <returns><see cref="CityObject"/> の列挙子です。</returns>
        public static IEnumerable<CityObject> ChildrenDfsAsCityObjects(Transform parent, int minDepth, CityModelLoader cityModelLoader, CityMetadata metadata)
        {
            var childGameObjs = ChildrenDfsAsTransform(parent, minDepth).Select(transform => transform.gameObject);
            foreach (var go in childGameObjs)
            {
                var loaded = cityModelLoader.Load(go, metadata);
                if (loaded != null)
                {
                    yield return loaded;
                }
            }
        }


        /// <summary>
        /// <paramref name="parent"/> のヒエラルキーの子を再帰的に列挙します。
        /// 順番は DFS (深さ優先探索) です。
        /// </summary>
        /// <param name="parent">対象となるゲームオブジェクトの Transform です。</param>
        /// <param name="minDepth">これより小さい深さは無視します。0は <paramref name="parent"/> 自身、1は子、2は孫です。</param>
        /// <returns></returns>
        public static IEnumerable<Transform> ChildrenDfsAsTransform(Transform parent, int minDepth)
        {
            var result = ChildrenDfsAsTransformRecursive(parent, minDepth, 0);
            foreach (var transform in result)
            {
                yield return transform;
            }
        }

        private static IEnumerable<Transform> ChildrenDfsAsTransformRecursive(Transform parent, int minDepth, int currentDepth)
        {
            if (currentDepth >= minDepth)
            {
                yield return parent;
            }

            int numChild = parent.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var child = parent.GetChild(i);
                var children = ChildrenDfsAsTransformRecursive(child, minDepth, currentDepth + 1);
                foreach (var c in children)
                {
                    yield return c;
                }
            }
        }
    }
}