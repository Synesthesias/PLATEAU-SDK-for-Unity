using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// インポートのGUIで、ローカルインポートに関する部分です。
    /// </summary>
    internal class CityImportLocalGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        
        private readonly CityLoadConfig config = new ();
        
        /// <summary> GUIのうち、範囲選択前に表示する部分です。 </summary>
        private readonly GUIBeforeAreaSelect guiBeforeAreaSelect;
        
        /// <summary> GUIのうち、範囲選択後に表示する部分です。 </summary>
        private ImportGUIAfterAreaSelect guiAfterAreaSelect;
        
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;

        /// <summary>
        /// メインスレッドから呼ばれることを前提とします。
        /// </summary>
        public CityImportLocalGUI(UnityEditor.EditorWindow parentEditorWindow)
        { 
            progressGUI = new ProgressDisplayGUI(parentEditorWindow);
            guiBeforeAreaSelect = new GUIBeforeAreaSelect();
        }

        public void Draw()
        {
            config.ConfBeforeAreaSelect = guiBeforeAreaSelect.Draw();
            
            
            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");
            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, this.config.ConfBeforeAreaSelect.DatasetSourceConfig,
                this, this.config.ConfBeforeAreaSelect.CoordinateZoneID);

            if (isAreaSelectComplete)
            {
                guiAfterAreaSelect.Draw();
            }
            
            progressGUI.Draw();
        }

        public void ReceiveResult(AreaSelectResult result)
        {
            this.config.InitWithAreaSelectResult(result);
            guiAfterAreaSelect = new ImportGUIAfterAreaSelect(this.config, result.PackageToLodDict, progressGUI);
        }

        /// <summary>
        /// ローカルインポートのGUIのうち、範囲選択前に表示するものです。
        /// </summary>
        private class GUIBeforeAreaSelect
        {
            private readonly CityLoadConfigBeforeAreaSelect confBeforeAreaSelect = new();
            private readonly PathSelectorFolderPlateauInput folderSelector = new ();
            private bool foldOutSourceFolderPath = true;

            public CityLoadConfigBeforeAreaSelect Draw()
            {
                this.foldOutSourceFolderPath = PlateauEditorStyle.FoldOut(this.foldOutSourceFolderPath, "入力フォルダ", () =>
                {
                    this.confBeforeAreaSelect.DatasetSourceConfig ??= new DatasetSourceConfigLocal("");
                    ((DatasetSourceConfigLocal)confBeforeAreaSelect.DatasetSourceConfig).LocalSourcePath = this.folderSelector.Draw("フォルダパス");
                });
            
                PlateauEditorStyle.Separator(0);
                PlateauEditorStyle.SubTitle("モデルデータの配置を行います。");
                PlateauEditorStyle.Heading("基準座標系の選択", "num1.png");
                confBeforeAreaSelect.CoordinateZoneID = CoordinateZonePopup.Draw(confBeforeAreaSelect.CoordinateZoneID);
                return confBeforeAreaSelect;
            }

            public void Dispose() { }
        }

        public void Dispose() { }
    }
}
