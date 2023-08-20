using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// インポートGUIのうち、範囲選択後に表示される部分です。
    /// ローカルインポートとリモートインポートで共通で利用します。、
    /// </summary>
    internal class ImportGUIAfterAreaSelect
    {
        private readonly CityLoadConfigGUI cityLoadConfigGUI;
        private readonly CityLoadConfig cityLoadConfig;
        private readonly ProgressDisplayGUI progressGui;

        public ImportGUIAfterAreaSelect(CityLoadConfig config, PackageToLodDict packageToLodDict, ProgressDisplayGUI progressGui)
        {
            this.cityLoadConfigGUI = new CityLoadConfigGUI(config, packageToLodDict);
            this.cityLoadConfig = config;
            this.progressGui = progressGui;
        } 
            
        public void Draw()
        {
            PlateauEditorStyle.Heading("地物別設定", "num3.png");
            this.cityLoadConfigGUI.Draw();
                
            PlateauEditorStyle.Separator(0);
            PlateauEditorStyle.Separator(0);

            ImportButton.Draw(this.cityLoadConfig, progressGui);
        }
    }
}