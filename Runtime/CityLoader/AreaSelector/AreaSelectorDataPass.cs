using System.Collections.Generic;
using System.Linq;
using PLATEAU.Udx;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
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
        private static GlobalObjectId loaderBehaviourID;
        private static PredefinedCityModelPackage availablePackageFlags;

        public static void Exec(string prevScenePathArg, IEnumerable<MeshCode> areaSelectResultArg, GlobalObjectId loaderBehaviourIDArg, PredefinedCityModelPackage availablePackageFlagsArg)
        {
            #if UNITY_EDITOR

            prevScenePath = prevScenePathArg;
            areaSelectResult = areaSelectResultArg;
            loaderBehaviourID = loaderBehaviourIDArg;
            availablePackageFlags = availablePackageFlagsArg;
            
            EditorSceneManager.sceneOpened += OnBackToPrevScene;
            EditorSceneManager.OpenScene(prevScenePath);
#endif
        }

        private static void OnBackToPrevScene(Scene scene, OpenSceneMode __)
        {
            EditorSceneManager.sceneOpened -= OnBackToPrevScene;
            SceneManager.SetActiveScene(scene);
            PassAreaSelectDataToBehaviour();
            EditorSceneManager.MarkSceneDirty(scene);
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
            
            loaderBehaviour.AreaMeshCodes = areaSelectResult.Select(meshCode => meshCode.ToString()).ToArray();
            loaderBehaviour.InitPackageConfigsWithPackageFlags(availablePackageFlags);
        }
    }
}
