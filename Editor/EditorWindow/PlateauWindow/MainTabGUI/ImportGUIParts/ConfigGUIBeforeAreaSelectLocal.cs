using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// ローカルインポートのGUIのうち、範囲選択前に表示するものです。
    /// </summary>
    internal class ConfigGUIBeforeAreaSelectLocal : IConfigGUIBeforeAreaSelect
    {
        
        private readonly ConfigBeforeAreaSelect confBeforeAreaSelect = new();
        private readonly PathSelectorFolderPlateauInput folderSelector = new ();
        private bool foldOutSourceFolderPath = true;

        public ConfigBeforeAreaSelect Draw()
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
    }
}