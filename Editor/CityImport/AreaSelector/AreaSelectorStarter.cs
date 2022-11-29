using System.IO;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.Dataset;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PLATEAU.Editor.CityImport.AreaSelector
{
    /// <summary>
    /// 地図からの範囲選択画面を開始します。
    /// 範囲選択専用のシーンを立ち上げます。Editモードであることが前提です。
    /// </summary>
    internal static class AreaSelectorStarter
    {

        private const string areaSelectorPrefabPath =
            "Packages/com.synesthesias.plateau-unity-sdk/Prefabs/AreaSelectorPrefab.prefab";
        
        /// <summary>
        /// 範囲選択の専用シーンを立ち上げます。
        /// ただし、現在のシーンに変更があれば保存するかどうかユーザーに尋ね、キャンセルが選択されたならば中止します。
        /// </summary>
        public static void Start(DatasetSourceInitializer datasetSourceInitializer, IAreaSelectResultReceiver areaSelectResultReceiver, int coordinateZoneID)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (AreaSelectorBehaviour.IsAreaSelectEnabled)
            {
                EditorUtility.DisplayDialog("PLATEAU SDK", "範囲選択画面がすでに開いています。\nシーンビュー左上の決定ボタンを押して選択を確定してください。", "OK");
                return;
            }

            string prevScenePath = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(prevScenePath))
            {
                EditorUtility.DisplayDialog("PLATEAU SDK", "シーンが未保存です。\n保存してから再度実行してください。", "OK");
                return;
            }
            
            SetUpTemporaryScene();
            var behaviour = Object.FindObjectOfType<AreaSelectorBehaviour>();
            behaviour.Init(prevScenePath, datasetSourceInitializer, areaSelectResultReceiver, coordinateZoneID);
        }

        private static void SetUpTemporaryScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Temporary_AreaSelectScene";
            SceneManager.SetActiveScene(scene);
            var prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(areaSelectorPrefabPath);
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
