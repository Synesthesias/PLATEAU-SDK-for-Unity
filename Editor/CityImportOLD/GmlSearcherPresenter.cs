using System;
using System.Collections.Generic;
using PLATEAU.CityMeta;

namespace PLATEAU.Editor.CityImportOLD
{
    /// <summary>
    /// GmlSearcher の Model, View, Config を結びつけ、設定GUIの描画と適用を行います。
    /// </summary>
    internal class GmlSearcherPresenter
    {
        private GmlSearcherConfig config;
        private GmlSearcherModel model;
        private readonly GmlSearcherView view = new GmlSearcherView();

        public GmlSearcherPresenter(GmlSearcherConfig searcherConfig)
        {
            this.config = searcherConfig;
            ExecGmlSearch("", false);
        }

        /// <summary>
        /// GmlSearcher の設定GUIを描画し、その設定を適用します。
        /// 戻り値は Gml検索の結果である gmlファイルの相対パスのリストです。
        /// </summary>
        [Obsolete]
        public List<string> Draw()
        {
            return this.view.Draw(this.model, ref this.config, this);
        }

        /// <summary>
        /// 現在の設定で対象となる Gmlファイルのリストを返します。
        /// </summary>
        public List<string> ListTargetGmlFiles()
        {
            return GmlSearcherModel.ListTargetGmlFiles(this.model, this.config);
        }

        /// <summary>
        /// インポート元パスが変わったときの処理です。
        /// </summary>
        public void OnImportSrcPathChanged(string path, InputFolderSelectorGUI.PathChangeMethod changeMethod)
        {
            ExecGmlSearch(path, changeMethod == InputFolderSelectorGUI.PathChangeMethod.Dialogue);
        }

        private void ExecGmlSearch(string path, bool shouldOverwriteMetadata)
        {
            // gmlファイルの検索をやり直します。
            if (!GmlSearcherModel.IsPathPlateauRoot(path)) return;
            this.model = new GmlSearcherModel(path);
            this.view.Reset();
            this.model.GenerateFileDictionary(path);

            // 初期化処理をやり直します。
            this.config.GenerateAreaTree(this.model.AreaIds, !shouldOverwriteMetadata);
        }
    }
}