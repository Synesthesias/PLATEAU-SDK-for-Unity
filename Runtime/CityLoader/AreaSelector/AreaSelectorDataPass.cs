using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.GameUI.Update;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityLoader.AreaSelector
{
    /// <summary>
    /// 範囲選択画面が終わったとき、元のシーンの <see cref="PLATEAUCityModelLoader"/> コンポーネントに
    /// 選択結果を渡します。
    /// </summary>
    internal static class AreaSelectorDataPass
    {
        // シーンをまたいで渡したいデータ
        private static string prevScenePath;
        private static IEnumerable<MeshCode> areaSelectResult;
        private static string dataSourcePath;
        private static GlobalObjectId loaderBehaviourID;

        public static void Exec(string prevScenePathArg, IEnumerable<MeshCode> areaSelectResultArg, string dataSourcePathArg, GlobalObjectId loaderBehaviourIDArg)
        {
            #if UNITY_EDITOR

            prevScenePath = prevScenePathArg;
            areaSelectResult = areaSelectResultArg;
            dataSourcePath = dataSourcePathArg;
            loaderBehaviourID = loaderBehaviourIDArg;
            
            EditorSceneManager.sceneOpened += OnBackToPrevScene;
            EditorSceneManager.OpenScene(prevScenePath);
            
#endif
        }

        private static void OnBackToPrevScene(Scene scene, OpenSceneMode __)
        {
            EditorSceneManager.sceneOpened -= OnBackToPrevScene;
            SceneManager.SetActiveScene(scene);
            PassAreaSelectDataToBehaviour();
        }

        private static void PassAreaSelectDataToBehaviour()
        {
            
            var loaderBehaviourObj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(loaderBehaviourID);
            if (loaderBehaviourObj == null)
            {
                Debug.LogError($"元の{nameof(PLATEAUCityModelLoader)} コンポーネントが見つかりません。 globalID = {loaderBehaviourID}");
                return;
            }

            var loaderBehaviour = (PLATEAUCityModelLoader)loaderBehaviourObj;

            loaderBehaviour.SourcePathAfterImport = dataSourcePath;
            loaderBehaviour.AreaMeshCodes = areaSelectResult.Select(meshCode => meshCode.ToString()).ToArray();

            // TODO 下のインポート処理はインポート設定後に動作するように移動
            // var selectedAreas = areaSelectResult.ToArray();
            // var collection = UdxFileCollection.Find(dataSourcePath).FilterByMeshCodes(selectedAreas);
            // string destPath = PathUtil.plateauSrcFetchDir;
            // var fetchTargetGmls = new List<GmlFileInfo>();
            // var gmlInfoToDestroy = new List<GmlFileInfo>();
            // foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
            // {
            //     foreach (var gmlPath in collection.GetGmlFiles(package))
            //     {
            //         var gmlInfo = GmlFileInfo.Create(gmlPath);
            //         gmlInfoToDestroy.Add(gmlInfo);
            //         fetchTargetGmls.Add(gmlInfo);
            //     }
            // }
            //
            // int targetGmlCount = fetchTargetGmls.Count;
            // for (int i = 0; i < targetGmlCount; i++)
            // {
            //     var gml = fetchTargetGmls[i];
            //     EditorUtility.DisplayProgressBar("", $"インポート処理中 : [{i}/{targetGmlCount}] {Path.GetFileName(gml.Path)}",
            //         (float)i / targetGmlCount);
            //     collection.Fetch(destPath, gml);
            // }
            // foreach(var gml in gmlInfoToDestroy) gml.Dispose();
            //
            // Debug.Log(DebugUtil.EnumerableToString(selectedAreas.Select(code => code.ToString())));
            // EditorUtility.ClearProgressBar();
        }
    }
}
