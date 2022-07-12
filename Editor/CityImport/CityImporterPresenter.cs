using PLATEAU.CityMeta;
using PLATEAU.Util;
using UnityEngine;

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
            // メタデータにファイルとして記録されたインポート元パスがあれば、それを復元し、GUI画面の初期値に代入します。
            string loadedSrcRootPath = importConfig.sourcePath.RootDirAssetPath;
            if (loadedSrcRootPath.Replace('\\', '/').StartsWith("Assets/"))
            {
                loadedSrcRootPath = PathUtil.AssetsPathToFullPath(loadedSrcRootPath);
            }
            if (!string.IsNullOrEmpty(loadedSrcRootPath))
            {
                importConfig.SrcRootPathBeforeImport = loadedSrcRootPath;
            }
            
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

        /// <summary> インポートします。 変換に成功したgmlの数を返します。 </summary>
        public int Import(string[] gmlRelativePaths, out CityMetadata metadata)
        {
            return this.model.Import(gmlRelativePaths, this.config, out metadata);
        }
    }
}