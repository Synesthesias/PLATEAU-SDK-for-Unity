using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using UnityEngine;

namespace PLATEAU.Behaviour
{
    /// <summary>
    /// シーン上の都市ゲームオブジェクトを列挙します。
    /// </summary>
    public static class CityHierarchyEnumerator
    {
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
                var children = ChildrenDfsAsTransformRecursive(child, minDepth, currentDepth+1);
                foreach (var c in children)
                {
                    yield return c;
                }
            }
        }
    }
}