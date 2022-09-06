using System.Collections.Generic;
using PLATEAU.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityLoader.AreaSelector
{
    public static class AreaSelectorDataPass
    {
        public static void Exec(PlayModeStateChange state, string prevScenePath, List<int> areaSelectResult)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            var scene = EditorSceneManager.OpenScene(prevScenePath);
            SceneManager.SetActiveScene(scene);
            Debug.Log(DebugUtil.EnumerableToString(areaSelectResult));
        }
    }
}
