using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
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
            guiBeforeAreaSelect = new GUIBeforeAreaSelect(this.config, this);
        }

        public void Draw()
        {
            guiBeforeAreaSelect.Draw();

            if (guiBeforeAreaSelect.IsAreaSelectComplete)
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
        private class GUIBeforeAreaSelect : IEditorDrawable
        {
            public bool IsAreaSelectComplete { get; private set; }
            private readonly PathSelectorFolderPlateauInput folderSelector = new ();
            private bool foldOutSourceFolderPath = true;
            private readonly CityLoadConfig config;
            private readonly IAreaSelectResultReceiver areaSelectResultReceiver;

            public GUIBeforeAreaSelect(CityLoadConfig config, IAreaSelectResultReceiver areaSelectResultReceiver)
            {
                this.config = config;
                this.areaSelectResultReceiver = areaSelectResultReceiver;
            }

            public void Draw()
            {
                this.foldOutSourceFolderPath = PlateauEditorStyle.FoldOut(this.foldOutSourceFolderPath, "入力フォルダ", () =>
                {
                    this.config.DatasetSourceConfig ??= new DatasetSourceConfig(false, "", "", "", "");
                    this.config.DatasetSourceConfig.LocalSourcePath = this.folderSelector.Draw("フォルダパス");
                });
            
                PlateauEditorStyle.Separator(0);
                PlateauEditorStyle.SubTitle("モデルデータの配置を行います。");
                PlateauEditorStyle.Heading("基準座標系の選択", "num1.png");
                CoordinateZonePopup.DrawAndSet(this.config);
            

                PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");
                IsAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, this.config.DatasetSourceConfig,
                    areaSelectResultReceiver, this.config.CoordinateZoneID);
            }
        }
    }
}
