using PLATEAU.CityImport.Config;
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
        private static AreaSelectResult areaSelectResult;
        private static IAreaSelectResultReceiver areaSelectResultReceiver;

#if UNITY_EDITOR
        public static void Exec(
            string prevScenePathArg, AreaSelectResult areaSelectResultArg,
            IAreaSelectResultReceiver areaSelectResultReceiverArg,
            EditorWindow prevEditorWindow)
        {
            prevScenePath = prevScenePathArg;
            areaSelectResult = areaSelectResultArg;
            areaSelectResultReceiver = areaSelectResultReceiverArg;
            
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
            if (areaSelectResult.AreaMeshCodes.Count == 0)
            {
                Debug.Log("地域は選択されませんでした。");
            }
            areaSelectResultReceiver.ReceiveResult(areaSelectResult);
        }
    }
}
