using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
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
        private readonly ConfigGUIBeforeAreaSelectLocal configGUIBeforeAreaSelectLocal;
        
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
            configGUIBeforeAreaSelectLocal = new ConfigGUIBeforeAreaSelectLocal();
        }

        public void Draw()
        {
            config.ConfBeforeAreaSelect = configGUIBeforeAreaSelectLocal.Draw();
            
            
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

        public void Dispose() { }
    }
}
