using System.Collections.Generic;
using System.Linq;
using PLATEAU.Udx;
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
        public static void Exec(string prevScenePath, IEnumerable<MeshCode> areaSelectResult)
        {
            #if UNITY_EDITOR
            var scene = EditorSceneManager.OpenScene(prevScenePath);
            SceneManager.SetActiveScene(scene);
            #endif
            Debug.Log(DebugUtil.EnumerableToString(areaSelectResult.Select(code => code.ToString())));
        }
    }
}
