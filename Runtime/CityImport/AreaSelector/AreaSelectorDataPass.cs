using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageLoadConfigs;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
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
        private static MeshCodeList selectedMeshCodes;
        private static IAreaSelectResultReceiver areaSelectResultReceiver;
        private static PackageToLodDict availablePackageLods;

#if UNITY_EDITOR
        public static void Exec(
            string prevScenePathArg, MeshCodeList selectedMeshCodesArg,
            IAreaSelectResultReceiver areaSelectResultReceiverArg, PackageToLodDict availablePackageLodsArg,
            EditorWindow prevEditorWindow)
        {
            prevScenePath = prevScenePathArg;
            selectedMeshCodes = selectedMeshCodesArg;
            areaSelectResultReceiver = areaSelectResultReceiverArg;
            availablePackageLods = availablePackageLodsArg;
            
            EditorSceneManager.sceneOpened += OnBackToPrevScene;
            EditorSceneManager.OpenScene(prevScenePath);
            // MacOSだと、範囲選択のときに PlateauWindow が背面に回ってしまい見失いがちなので再表示します。
            prevEditorWindow.Focus();
        }
#endif

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
            if (selectedMeshCodes.Count == 0)
            {
                Debug.Log("地域は選択されませんでした。");
            }

            var areaSelectResult = new AreaSelectResult(selectedMeshCodes, availablePackageLods);
            areaSelectResultReceiver.ReceiveResult(areaSelectResult);
        }
    }
}
