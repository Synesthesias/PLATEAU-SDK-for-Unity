using System.IO;
using PLATEAU.CityImport.Setting;
using PLATEAU.Udx;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityImport
{
    /// <summary>
    /// 都市モデルをロードするための設定を保持するコンポーネントです。
    /// インスペクタでの表示については PLATEAUCityModelLoaderEditor を参照してください。
    /// 実際にロードする処理については <see cref="Load.CityImporter"/> を参照してください。
    /// </summary>
    internal class PLATEAUCityLoaderBehaviour : MonoBehaviour
    {
        [SerializeField] private CityLoadConfig cityLoadConfig = new CityLoadConfig();

        /// <summary>
        /// このコンポーネントが付いたゲームオブジェクトをシーンに生成します。
        /// </summary>
        public static GameObject Create(string sourcePathBeforeImport)
        {
            string objName = Path.GetFileName(sourcePathBeforeImport);
            var obj = new GameObject(objName);
            var loader = obj.AddComponent<PLATEAUCityLoaderBehaviour>();
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
