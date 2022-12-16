using PLATEAU.CityImport.Setting;
using PLATEAU.Interop;
using PLATEAU.Dataset;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace PLATEAU.CityImport
{
    /// <summary>
    /// 都市モデルをロードするための設定を保持するコンポーネントです。
    /// インスペクタでの表示については PLATEAUCityModelLoaderEditor を参照してください。
    /// 実際にロードする処理については <see cref="Load.CityImporter"/> を参照してください。
    /// </summary>
    internal class PLATEAUCityModelLoader : MonoBehaviour
    {
        [SerializeField] private CityLoadConfig cityLoadConfig = new CityLoadConfig();
//
//         /// <summary>
//         /// このコンポーネントが付いたゲームオブジェクトをシーンに生成します。
//         /// </summary>
//         public static GameObject Create(DatasetSourceConfig datasetSourceConfig)
//         {
//             string objName = datasetSourceConfig.RootDirName;
//             var obj = new GameObject(objName);
//             var loader = obj.AddComponent<PLATEAUCityModelLoader>();
//             loader.Init(datasetSourceConfig);
// #if UNITY_EDITOR
//             EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
//             #endif
//             return obj;
//         }

        private void Init(DatasetSourceConfig arg)
        {
            DatasetSourceConfig = arg;
        }

        public DatasetSourceConfig DatasetSourceConfig
        {
            get => CityLoadConfig.DatasetSourceConfig;
            private set => CityLoadConfig.DatasetSourceConfig = value;
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

        public int CoordinateZoneID
        {
            get => CityLoadConfig.CoordinateZoneID;
            set => CityLoadConfig.CoordinateZoneID = value;
        }

        public Extent Extent
        {
            get => CityLoadConfig.Extent;
            set => CityLoadConfig.Extent = value;
        }

        public CityLoadConfig CityLoadConfig => this.cityLoadConfig;

        public void InitPackageConfigsWithPackageFlags(PredefinedCityModelPackage flags)
        {
            CityLoadConfig.InitWithPackageFlags(flags);
        }
    }
}
