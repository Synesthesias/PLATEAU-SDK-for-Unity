using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.ProgressDisplay;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
{
    /// <summary>
    /// インポートGUIのうち、範囲選択後に表示される部分です。
    /// ローカルインポートとリモートインポートで共通で利用します。
    /// </summary>
    internal class ImportGUIAfterAreaSelect
    {
        private readonly CityImport.CityImportConfigGUI cityImportConfigGUI;
        private readonly CityImportConfig cityImportConfig;
        private readonly ProgressDisplayGUI progressGui;
        private readonly ImportButton importButton;

        public ImportGUIAfterAreaSelect(CityImportConfig config, PackageToLodDict packageToLodDict, ProgressDisplayGUI progressGui)
        {
            this.cityImportConfigGUI = new CityImport.CityImportConfigGUI(config, packageToLodDict);
            this.cityImportConfig = config;
            this.progressGui = progressGui;
            this.importButton = new ImportButton();
        } 
            
        public void Draw()
        {
            this.cityImportConfigGUI.Draw();
                
            PlateauEditorStyle.Separator(0);
            PlateauEditorStyle.Separator(0);

            importButton.Draw(this.cityImportConfig, progressGui);
        }
    }
}