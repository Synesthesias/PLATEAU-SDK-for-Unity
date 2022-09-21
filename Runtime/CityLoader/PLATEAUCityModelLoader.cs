using System.IO;
using PLATEAU.Behaviour;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityLoader
{
    /// <summary>
    /// 都市モデルを読み込むコンポーネントです。
    /// インスペクタに表示されるボタンについては PLATEAUCityModelLoaderEditor を参照してください。
    /// </summary>
    internal class PLATEAUCityModelLoader : MonoBehaviour
    {
        [SerializeField] private string sourcePathBeforeImport;
        [SerializeField] private string sourcePathAfterImport;
        [SerializeField] private CityLoadConfig cityLoadConfig = new CityLoadConfig();
        [SerializeField] private string[] areaMeshCodes;
        
        /// <summary>
        /// このコンポーネントが付いたゲームオブジェクトをシーンに生成します。
        /// </summary>
        public static GameObject Create(string sourcePathBeforeImport)
        {
            string objName = Path.GetFileName(sourcePathBeforeImport);
            var obj = new GameObject(objName);
            var loader = obj.AddComponent<PLATEAUCityModelLoader>();
            loader.Init(sourcePathBeforeImport);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return obj;
        }

        private void Init(string sourcePathBeforeImportArg)
        {
            this.sourcePathBeforeImport = sourcePathBeforeImportArg;
        }

        public string SourcePathBeforeImport => this.sourcePathBeforeImport;

        public string SourcePathAfterImport
        {
            get => this.sourcePathAfterImport;
            set => this.sourcePathAfterImport = value;
        }

        public string[] AreaMeshCodes
        {
            get => this.areaMeshCodes;
            set => this.areaMeshCodes = value;
        }

        public CityLoadConfig CityLoadConfig => this.cityLoadConfig;

        public void InitPackageConfigsWithPackageFlags(PredefinedCityModelPackage flags)
        {
            this.cityLoadConfig.InitWithPackageFlags(flags);
        }
    }
}
