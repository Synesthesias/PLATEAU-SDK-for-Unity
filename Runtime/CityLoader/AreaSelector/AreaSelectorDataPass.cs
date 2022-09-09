using System.Collections.Generic;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityLoader.AreaSelector
{
    public static class AreaSelectorDataPass
    {
        public static void Exec(string prevScenePath, List<int> areaSelectResult)
        {
            #if UNITY_EDITOR
            var scene = EditorSceneManager.OpenScene(prevScenePath);
            SceneManager.SetActiveScene(scene);
            #endif
            Debug.Log(DebugUtil.EnumerableToString(areaSelectResult));
        }
    }
}
