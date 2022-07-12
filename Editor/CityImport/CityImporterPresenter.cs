using PLATEAU.CityMeta;
using PLATEAU.Util;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// CityImporter の model, view, config を結びつけ、それぞれに指示を出します。
    /// </summary>
    internal class CityImporterPresenter
    {
        private readonly CityImporterModel model;
        private readonly CityImporterView view;
        private readonly CityImportConfig config;

        /// <summary> 既存の設定で初期化します。 </summary>
        public static CityImporterPresenter InitWithConfig(CityImportConfig importConfig)
        {
            // 記録されたインポート元パスを復元し、GUI画面の初期値に代入します。
            string loadedSrcRootPath = importConfig.sourcePath.RootDirAssetPath;
            string initialSrcRootPath = loadedSrcRootPath;
            if (initialSrcRootPath.Replace('\\', '/').StartsWith("Assets/"))
            {
                initialSrcRootPath = PathUtil.AssetsPathToFullPath(initialSrcRootPath);
            }
            importConfig.SrcRootPathBeforeImport = initialSrcRootPath;
            
            return new CityImporterPresenter(importConfig);
        }

        /// <summary> 初期値で初期化します。 </summary>
        public static CityImporterPresenter InitWithDefaultValue()
        {
            var config = new CityImportConfig();
            config.objConvertTypesConfig.SetLodRangeToAllRange(); // 初期値
            return InitWithConfig(config);
        }

        /// <summary> 上の複数の初期化処理の共通部分です。 </summary>
        private CityImporterPresenter(CityImportConfig config)
        {
            this.model = new CityImporterModel();
            this.view = new CityImporterView();
            this.config = config;
        }
        
        /// <summary> インポート設定GUIを描画します。 </summary>
        public void Draw()
        {
            this.view.Draw(this, this.config);
        }

        /// <summary> インポートします。 </summary>
        public void Import(string[] gmlRelativePaths)
        {
            this.model.Import(gmlRelativePaths, this.config, out _);
        }
    }
}