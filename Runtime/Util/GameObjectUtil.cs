using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.Util
{
    internal static class GameObjectUtil
    {
        /// <summary>
        /// <paramref name="parent"/> とその子を再帰的に検索し、
        /// 名前が <paramref name="searchName"/> であるものを探して返します。
        /// なければ null を返します。
        /// </summary>
        public static Transform FindRecursive(Transform parent, string searchName)
        {
            if (parent.name == searchName) return parent;
            foreach (Transform child in parent)
            {
                var found = FindRecursive(child, searchName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// <paramref name="parent"/> の子（孫以下は含まない）であり、
        /// 名前が <paramref name="targetName"/> であるものを即座に削除します。
        /// そのような GameObject が存在しない場合は何もしません。
        /// </summary>
        public static void DestroyChildOf(Transform parent, string targetName)
        {
            var oldObj = parent.Find(targetName);
            if (oldObj != null)
            {
                Object.DestroyImmediate(oldObj.gameObject);
            }
        }


        /// <summary>
        /// <paramref name="gameObj"/> にコンポーネント <typeparamref name="T"/> がなければ追加します。
        /// すでにあれば何もしません。
        /// どちらにしても 対象の <typeparamref name="T"/> を返します。
        /// </summary>
        public static T AssureComponent<T>(GameObject gameObj) where T : Component
        {
            var component = gameObj.GetComponent<T>();
            if (component == null)
            {
                component = gameObj.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        /// <paramref name="name"/> という名前の GameObject が存在しなければ作ります。
        /// すでに存在すれば何もしません。
        /// どちらにしても 対象の GameObject を返します。
        /// </summary>
        public static GameObject AssureGameObject(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var gameObj = ListGameObjsInScene(scene).FirstOrDefault(go => go.name == name);
            if (gameObj == null)
            {
                gameObj = new GameObject(name);
            }

            return gameObj;
        }

        /// <summary>
        /// <paramref name="parent"/> の子に <paramref name="name"/> という名前の GameObject が存在しなければ作ります。
        /// すでに存在すれば何もしません。
        /// どちらにしても対象の GameObject を返します。
        /// </summary>
        public static GameObject AssureGameObjectInChild(string name, Transform parent)
        {
            var existingTrans = parent.Find(name);
            if (existingTrans) return existingTrans.gameObject;
            var newObj = new GameObject(name)
            {
                transform =
                {
                    parent = parent
                }
            };
            return newObj;
        }

        /// <summary>
        /// <paramref name="obj"/> の子をすべて削除します。
        /// </summary>
        public static void DestroyChildOf(GameObject obj)
        {
            var trans = obj.transform;
            int cnt = trans.childCount;
            var destroyList = new List<GameObject>();
            for (int i = 0; i < cnt; i++)
            {
                destroyList.Add(trans.GetChild(i).gameObject);
            }
            foreach (var d in destroyList) Object.DestroyImmediate(d);
        }

        public static List<GameObject> ListGameObjsInScene(Scene scene)
        {
            var list = new List<GameObject>();
            var rootObjs = scene.GetRootGameObjects();
            foreach (var go in rootObjs)
            {
                ListGameObjsRecursive(go, list);
            }

            return list;
        }

        public static void ListGameObjsRecursive(GameObject targetGameObj, List<GameObject> list)
        {
            list.Add(targetGameObj);
            foreach (Transform childTrans in targetGameObj.transform)
            {
                ListGameObjsRecursive(childTrans.gameObject, list);
            }
        }
    }
}