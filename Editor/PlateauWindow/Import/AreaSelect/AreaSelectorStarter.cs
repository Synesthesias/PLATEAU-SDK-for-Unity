using PLATEAU.CityLoader.AreaSelector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PLATEAU.Editor.PlateauWindow.Import.AreaSelect
{
    /// <summary>
    /// 地図からの範囲選択画面を開始します。
    /// 範囲選択専用のシーンを立ち上げます。Editモードであることが前提です。
    /// </summary>
    public class AreaSelectorStarter
    {
        private Scene prevScene;

        private const string areaSelectorPrafabPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Prefabs/AreaSelectorPrefab.prefab";
        
        /// <summary>
        /// 範囲選択の専用シーンを立ち上げます。
        /// ただし、現在のシーンに変更があれば保存するかどうかユーザーに尋ね、キャンセルが選択されたならば中止します。
        /// </summary>
        public static void Start(string dataSourcePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string prevScenePath = SceneManager.GetActiveScene().path;
            SetUpTemporaryScene();
            var behaviour = Object.FindObjectOfType<AreaSelectorBehaviour>();
            behaviour.Init(prevScenePath, dataSourcePath);
        }

        private static void SetUpTemporaryScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Temporary_AreaSelectScene";
            SceneManager.SetActiveScene(scene);
            var prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(areaSelectorPrafabPath);
            Object.Instantiate(prefab);
            prefab.transform.position = Vector3.zero;
            SetUpEventSystem();
        }

        private static void SetUpEventSystem()
        {
            var obj = new GameObject("EventSystem");
            obj.AddComponent<EventSystem>();
            obj.AddComponent<StandaloneInputModule>();
        }
    }
}
