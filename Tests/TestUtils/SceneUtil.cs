using UnityEngine;

namespace PLATEAU.Tests.TestUtils
{
    public static class SceneUtil
    {
        public static void DestroyAllGameObjectsInActiveScene()
        {
            var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in rootObjs)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}