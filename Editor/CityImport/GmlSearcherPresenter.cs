using System.Collections.Generic;
using PLATEAU.CityMeta;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// GmlSearcher の Model, View, Config を結びつけ、設定GUIの描画と適用を行います。
    /// </summary>
    internal class GmlSearcherPresenter
    {
        private GmlSearcherConfig config;
        private GmlSearcherView view = new GmlSearcherView();
        private GmlSearcherModel model;
        private bool isInitialized;
        private bool shouldOverwriteMetadata;


        private void Initialize(CityImportConfig importConfig)
        {
            this.config = importConfig.gmlSearcherConfig;
            this.config.GenerateAreaTree(this.model.AreaIds, !this.shouldOverwriteMetadata);
            this.isInitialized = true;
        }

        /// <summary>
        /// GmlSearcher の設定GUIを描画し、その設定を適用します。
        /// 戻り値は Gml検索の結果である gmlファイルの相対パスのリストです。
        /// </summary>
        public List<string> Draw(CityImportConfig importConfig) // TODO 戻り値は本当は config に含めた方が良いかも
        {
            if(!this.isInitialized) Initialize(importConfig);
            return this.view.Draw(this.model, ref this.config);
        }
        
        /// <summary>
        /// インポート元パスが変わったときの処理です。
        /// </summary>
        public void OnImportSrcPathChanged(string path, InputFolderSelectorGUI.PathChangeMethod changeMethod)
        {
            if (!GmlSearcherModel.IsPathPlateauRoot(path)) return;
            this.model ??= new GmlSearcherModel(path);
            this.view ??= new GmlSearcherView(); // TODO これは readonly の使い回しでよさそう
            this.model.GenerateFileDictionary(path);
            
            // 次の描画時に初期化処理をやり直します。
            this.shouldOverwriteMetadata = changeMethod == InputFolderSelectorGUI.PathChangeMethod.Dialogue;
            this.isInitialized = false;
        }
    }
}