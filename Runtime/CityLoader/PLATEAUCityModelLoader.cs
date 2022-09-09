using System.IO;
using PLATEAU.CityLoader.Setting;
using UnityEngine;

namespace PLATEAU.CityLoader
{
    /// <summary>
    /// 都市モデルを読み込むコンポーネントです。
    /// インスペクタに表示されるボタンについては PLATEAUCityModelLoaderEditor を参照してください。
    /// </summary>
    internal class PLATEAUCityModelLoader : MonoBehaviour
    {
        [SerializeField] private string sourcePathBeforeImport;
        [SerializeField] private string sourcePathAfterImport; // TODO
        [SerializeField] private CityLoadSetting cityLoadSetting = new CityLoadSetting();
        
        /// <summary>
        /// このコンポーネントが付いたゲームオブジェクトをシーンに生成します。
        /// </summary>
        public static GameObject Create(string sourcePathBeforeImport)
        {
            string objName = Path.GetFileName(sourcePathBeforeImport);
            var obj = new GameObject(objName);
            var loader = obj.AddComponent<PLATEAUCityModelLoader>();
            loader.Init(sourcePathBeforeImport);
            return obj;
        }

        private void Init(string sourcePathBeforeImportArg)
        {
            this.sourcePathBeforeImport = sourcePathBeforeImportArg;
        }

        public string SourcePathBeforeImport => this.sourcePathBeforeImport;
        public string SourcePathAfterImport  => this.sourcePathAfterImport;

    }
}
