using System.Collections.Generic;
using System.Linq;
using PLATEAU.Interop;
using PLATEAU.Udx;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

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
        private static IAreaSelectResultReceiver areaSelectResultReceiver;
        private static PredefinedCityModelPackage availablePackageFlags;
        private static Extent extent;

        public static void Exec(
            string prevScenePathArg, IEnumerable<MeshCode> selectedMeshCodesArg,
            IAreaSelectResultReceiver areaSelectResultReceiverArg, PredefinedCityModelPackage availablePackageFlagsArg,
            Extent selectedExtent)
        {
#if UNITY_EDITOR

            prevScenePath = prevScenePathArg;
            selectedMeshCodes = selectedMeshCodesArg;
            areaSelectResultReceiver = areaSelectResultReceiverArg;
            availablePackageFlags = availablePackageFlagsArg;
            extent = selectedExtent;
            
            EditorSceneManager.sceneOpened += OnBackToPrevScene;
            EditorSceneManager.OpenScene(prevScenePath);
#endif
        }
        
#if UNITY_EDITOR
        private static void OnBackToPrevScene(Scene scene, OpenSceneMode __)
        {
            EditorSceneManager.sceneOpened -= OnBackToPrevScene;
            SceneManager.SetActiveScene(scene);
            PassAreaSelectDataToBehaviour();
            EditorSceneManager.MarkSceneDirty(scene);
        }
#endif

        /// <summary>
        /// 戻った先のシーンで、範囲選択の結果を渡します。
        /// </summary>
        private static void PassAreaSelectDataToBehaviour()
        {
            var areaMeshCodes = selectedMeshCodes
                .Select(meshCode => meshCode.ToString())
                .ToArray();
            areaSelectResultReceiver.ReceiveResult(areaMeshCodes, extent, availablePackageFlags);
        }
    }
}
