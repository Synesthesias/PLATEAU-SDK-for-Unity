using NUnit.Framework;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.TestUtils
{
    public static class SceneUtil
    {
        public static void DestroyAllGameObjectsInEditModeTestScene()
        {
            var rootObjs = SceneUtil.GetEditModeTestScene().GetRootGameObjects();
            foreach (var obj in rootObjs)
            {
                Object.DestroyImmediate(obj);
            }
        }



        /// <summary>
        /// EditModeテストの実行時にUnityが自動的に立ち上げる、デフォルトのテストシーンを返します。
        /// </summary>
        public static Scene GetEditModeTestScene()
        {
            int numOpenScene = SceneManager.sceneCount;
            Scene testScene = default;
            bool found = false;
            for (int i = 0; i < numOpenScene; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                // Editモードのシーンは無名であると仮定します。
                if (scene.name == "")
                {
                    testScene = scene;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new Exception("Test Scene is not found.");
            }

            return testScene;
        }


        public static List<GameObject> GetObjectsOfEditModeTestScene()
        {
            return GameObjectUtil.ListGameObjsInScene(GetEditModeTestScene());
        }

        public static void AssertGameObjExists(string name)
        {
            AssertGameObjExists(name, true);
        }

        public static void AssertGameObjNotExists(string name)
        {
            AssertGameObjExists(name, false);
        }

        private static void AssertGameObjExists(string name, bool expect)
        {
            var foundObj = GetObjectsOfEditModeTestScene();
            bool found = foundObj.Any(obj => obj.name == name);
            string message = $"{name} という名称のゲームオブジェクトが";
            message += expect ? "存在します" : "存在しません";
            Assert.AreEqual(expect, found, message);
        }
    }
}