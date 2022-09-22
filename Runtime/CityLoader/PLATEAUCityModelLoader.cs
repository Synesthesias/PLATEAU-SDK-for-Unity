using System.IO;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Udx;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityLoader
{
    /// <summary>
    /// 都市モデルを読み込むための設定を保持するコンポーネントです。
    /// 具体的な都市読み込み処理については PLATEAUCityModelLoaderEditor を参照してください。
    /// </summary>
    internal class PLATEAUCityModelLoader : MonoBehaviour
    {
        [SerializeField] private CityLoadConfig cityLoadConfig = new CityLoadConfig();

        /// <summary>
        /// このコンポーネントが付いたゲームオブジェクトをシーンに生成します。
        /// </summary>
        public static GameObject Create(string sourcePathBeforeImport)
        {
            string objName = Path.GetFileName(sourcePathBeforeImport);
            var obj = new GameObject(objName);
            var loader = obj.AddComponent<PLATEAUCityModelLoader>();
            loader.Init(sourcePathBeforeImport);
            Debug.Log(loader.CityLoadConfig.SourcePathBeforeImport);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return obj;
        }

        private void Init(string sourcePathBeforeImportArg)
        {
            SourcePathBeforeImport = sourcePathBeforeImportArg;
        }

        public string SourcePathBeforeImport
        {
            get => CityLoadConfig.SourcePathBeforeImport;
            set => CityLoadConfig.SourcePathBeforeImport = value;
        }

        public string SourcePathAfterImport
        {
            get => CityLoadConfig.SourcePathAfterImport;
            set => CityLoadConfig.SourcePathAfterImport = value;
        }

        public string[] AreaMeshCodes
        {
            get => CityLoadConfig.AreaMeshCodes;
            set => CityLoadConfig.AreaMeshCodes = value;
        }

        public CityLoadConfig CityLoadConfig => this.cityLoadConfig;

        public void InitPackageConfigsWithPackageFlags(PredefinedCityModelPackage flags)
        {
            CityLoadConfig.InitWithPackageFlags(flags);
        }
    }
}
