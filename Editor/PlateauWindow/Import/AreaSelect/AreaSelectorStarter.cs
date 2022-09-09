using PLATEAU.CityLoader.AreaSelector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PLATEAU.Runtime.CityLoader.AreaSelector.Import.AreaSelect
{
    public class AreaSelectorStarter
    {
        private Scene prevScene;

        private const string areaSelectorPrafabPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Prefabs/AreaSelectorPrefab.prefab";
        public void Start()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string prevScenePath = SceneManager.GetActiveScene().path;
            SetUpTemporaryScene();
            var behaviour = Object.FindObjectOfType<AreaSelectorBehaviour>();
            behaviour.Init(prevScenePath);
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
