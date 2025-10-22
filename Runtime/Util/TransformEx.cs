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

        /// <summary>
        /// self.childCountとself.GetChild(i)から直下の子をIEnumerableで取得する
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> GetChildren(this Transform self)
        {
            if (!self)
                yield break;

            for (var i = 0; i < self.childCount; ++i)
            {
                yield return self.GetChild(i);
            }
        }

        /// <summary> /// 指定された名前を持つ親Transformを探し、その直下の子から条件に合うTransformを1つ返す /// </summary> 
        public static Transform FindChildOfNamedParent(this Transform origin, string parentName)
        {
            Transform parent = origin.parent;
            Transform current = origin;
            while (parent?.name != parentName)
            {
                current = parent;
                if (parent == null)
                    break;
                parent = parent.parent;
            }
            return current;
        }

        // 指定した Component を持つ Transform のみを取得
        public static List<Transform> GetAllChildrenWithComponent<T>(Transform parent) where T : Component
        {
            List<Transform> result = new List<Transform>();
            CollectChildrenWithComponentRecursive<T>(parent, result);
            return result;
        }

        private static void CollectChildrenWithComponentRecursive<T>(Transform current, List<Transform> result) where T : Component
        {
            foreach (Transform child in current)
            {
                if (child.GetComponent<T>() != null)
                {
                    result.Add(child);
                }
                CollectChildrenWithComponentRecursive<T>(child, result);
            }
        }

        /// <summary> /// 指定した名前の親までのPathを返す /// </summary> 
        public static string GetPathToParent(this Transform origin, string parentName)
        {
            Transform current = origin;
            string path = current.name;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
                if (current.name == parentName)
                    break;
            }
            return path;
        }

        // 全ての子Transformを取得
        public static List<Transform> GetAllChildren(this Transform parent)
        {
            var result = new List<Transform>();
            CollectChildrenRecursive(parent, result);
            return result;
        }

        private static void CollectChildrenRecursive(Transform current, List<Transform> result)
        {
            foreach (Transform child in current)
            {
                result.Add(child);
                CollectChildrenRecursive(child, result);
            }
        }

    }
}