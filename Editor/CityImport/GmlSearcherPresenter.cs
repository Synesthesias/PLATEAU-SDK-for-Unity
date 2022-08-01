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
        private GmlSearcherModel model;
        private readonly GmlSearcherView view = new GmlSearcherView();
        // private bool isInitialized;
        // private bool shouldOverwriteMetadata;

        public GmlSearcherPresenter(GmlSearcherConfig searcherConfig)
        {
            this.config = searcherConfig;
            this.model = new GmlSearcherModel("");
            Initialize(false);
        }

        private void Initialize(bool shouldOverwriteMetadata)
        {
            this.config.GenerateAreaTree(this.model.AreaIds, !shouldOverwriteMetadata);
            // this.isInitialized = true;
        }

        /// <summary>
        /// GmlSearcher の設定GUIを描画し、その設定を適用します。
        /// 戻り値は Gml検索の結果である gmlファイルの相対パスのリストです。
        /// </summary>
        public List<string> Draw(GmlSearcherConfig searcherConfig) // TODO 戻り値は本当は config に含めた方が良いかも
        {
            // if(!this.isInitialized) Initialize(searcherConfig, false);
            return this.view.Draw(this.model, ref this.config);
        }
        
        /// <summary>
        /// インポート元パスが変わったときの処理です。
        /// </summary>
        public void OnImportSrcPathChanged(string path, InputFolderSelectorGUI.PathChangeMethod changeMethod)
        {
            // gmlファイルの検索をやり直します。
            if (!GmlSearcherModel.IsPathPlateauRoot(path)) return;
            this.model = new GmlSearcherModel(path);
            this.view.Reset();
            this.model.GenerateFileDictionary(path);
            
            // 初期化処理をやり直します。
            bool shouldOverwriteMetadata = changeMethod == InputFolderSelectorGUI.PathChangeMethod.Dialogue;
            Initialize(shouldOverwriteMetadata);
        }
    }
}