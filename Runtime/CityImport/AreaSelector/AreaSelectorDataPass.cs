using System.Collections.Generic;
using System.Linq;
using PLATEAU.Interop;
using PLATEAU.Udx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityImport.AreaSelector
{
    /// <summary>
    /// 範囲選択画面が終わったとき、元のシーンの <see cref="PLATEAUCityModelLoader"/> コンポーネントに
    /// 選択結果を渡します。
    /// </summary>
    internal static class AreaSelectorDataPass
    {
        // シーンをまたいで渡したいデータ
        private static string prevScenePath;
        private static IEnumerable<MeshCode> selectedMeshCodes;
        private static GlobalObjectId loaderBehaviourID;
        private static PredefinedCityModelPackage availablePackageFlags;
        private static Extent extent;

        public static void Exec(
            string prevScenePathArg, IEnumerable<MeshCode> selectedMeshCodesArg,
            GlobalObjectId loaderBehaviourIDArg, PredefinedCityModelPackage availablePackageFlagsArg,
            Extent selectedExtent)
        {
            #if UNITY_EDITOR

            prevScenePath = prevScenePathArg;
            selectedMeshCodes = selectedMeshCodesArg;
            loaderBehaviourID = loaderBehaviourIDArg;
            availablePackageFlags = availablePackageFlagsArg;
            extent = selectedExtent;
            
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

        /// <summary>
        /// 戻った先のシーンで、Behaviourに範囲選択の結果を渡します。
        /// </summary>
        private static void PassAreaSelectDataToBehaviour()
        {
            
            var loaderBehaviourObj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(loaderBehaviourID);
            if (loaderBehaviourObj == null)
            {
                Debug.LogError($"元の{nameof(PLATEAUCityModelLoader)} コンポーネントが見つかりません。 globalID = {loaderBehaviourID}");
                return;
            }

            var loaderBehaviour = (PLATEAUCityModelLoader)loaderBehaviourObj;
            
            loaderBehaviour.AreaMeshCodes = selectedMeshCodes.Select(meshCode => meshCode.ToString()).ToArray();
            loaderBehaviour.InitPackageConfigsWithPackageFlags(availablePackageFlags);
            loaderBehaviour.Extent = extent;
        }
    }
}
