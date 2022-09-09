using System;
using PLATEAU.CityMeta;

namespace PLATEAU.Editor.CityImportOLD
{
    /// <summary>
    /// CityImporter の model, view, config を結びつけ、それぞれに指示を出します。
    /// </summary>
    [Obsolete]
    internal class CityImporterPresenter
    {
        private readonly CityImporterView view;
        private readonly CityImportConfig config;

        /// <summary> 既存の設定で初期化したインスタンスを返します。 </summary>
        public static CityImporterPresenter InitWithConfig(CityImportConfig importConfig)
        {
            importConfig.GuiConfFromLoadedConf();
            return new CityImporterPresenter(importConfig);
        }

        /// <summary> 初期値で初期化したインスタンスを返します。 </summary>
        public static CityImporterPresenter InitWithDefaultValue()
        {
            return InitWithConfig(CityImportConfig.Default);
        }

        /// <summary> 上の複数の初期化処理の共通部分です。 </summary>
        private CityImporterPresenter(CityImportConfig config)
        {
            this.view = new CityImporterView(config);
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
            return CityImporterModel.Import(gmlRelativePaths, this.config, out metadata);
        }
    }
}