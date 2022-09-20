using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.GameUI.Update;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityLoader.AreaSelector
{
    public static class AreaSelectorDataPass
    {
        public static void Exec(string prevScenePath, IEnumerable<MeshCode> areaSelectResult, string dataSourcePath)
        {
            #if UNITY_EDITOR
            var scene = EditorSceneManager.OpenScene(prevScenePath);
            SceneManager.SetActiveScene(scene);
            var selectedAreas = areaSelectResult.ToArray();
            var collection = UdxFileCollection.Find(dataSourcePath).FilterByMeshCodes(selectedAreas);
            string destPath = PathUtil.plateauSrcFetchDir + "/" + Path.GetFileName(dataSourcePath);
            var fetchTargetGmls = new List<GmlFileInfo>();
            var gmlInfoToDestroy = new List<GmlFileInfo>();
            foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            {
                foreach (var gmlPath in collection.GetGmlFiles(package))
                {
                    var gmlInfo = GmlFileInfo.Create(gmlPath);
                    gmlInfoToDestroy.Add(gmlInfo);
                    fetchTargetGmls.Add(gmlInfo);
                }
            }

            for (int i = 0; i < fetchTargetGmls.Count; i++)
            {
                var gml = fetchTargetGmls[i];
                EditorUtility.DisplayProgressBar("", $"コピー中 : {Path.GetFileName(gml.Path)}",
                    (float)i / fetchTargetGmls.Count);
                collection.Fetch(destPath, gml);
            }
            foreach(var gml in gmlInfoToDestroy) gml.Dispose();
            
            Debug.Log(DebugUtil.EnumerableToString(selectedAreas.Select(code => code.ToString())));
            EditorUtility.ClearProgressBar();
            #endif
        }
    }
}
