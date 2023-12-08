using System.Linq;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// インポート画面で「サーバー」が選択されたときのGUIです。
    /// </summary>
    internal class CityImportRemoteGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private readonly CityLoadConfig config = new CityLoadConfig();
        private readonly ConfigGUIBeforeAreaSelectRemote configGUIBeforeAreaSelectRemote;
        private ImportGUIAfterAreaSelect guiAfterAreaSelect;

        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;


        public CityImportRemoteGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.configGUIBeforeAreaSelectRemote = new ConfigGUIBeforeAreaSelectRemote(parentEditorWindow);
            progressGUI = new ProgressDisplayGUI(parentEditorWindow);
        }

        public void Draw()
        {
            config.ConfBeforeAreaSelect = this.configGUIBeforeAreaSelectRemote.Draw();
            
            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, 
                (DatasetSourceConfigRemote)this.config.ConfBeforeAreaSelect.DatasetSourceConfig,
                this, config.ConfBeforeAreaSelect.CoordinateZoneID);

            if (isAreaSelectComplete)
            {
                this.guiAfterAreaSelect.Draw();
            }

            progressGUI.Draw();
        }

        public void Dispose() { }

        public void ReceiveResult(AreaSelectResult result)
        {
            this.config.InitWithAreaSelectResult(result);
            this.guiAfterAreaSelect = new ImportGUIAfterAreaSelect(this.config, result.PackageToLodDict, progressGUI);
        }

        // テストで使う用です。
        internal ServerDatasetFetchGUI.LoadStatusEnum DatasetFetchStatus => configGUIBeforeAreaSelectRemote.DatasetFetchStatus;

    }
}