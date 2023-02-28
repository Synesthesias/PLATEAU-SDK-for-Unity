using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportLocalGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private readonly PathSelectorFolderPlateauInput folderSelector = new ();
        private readonly CityLoadConfig config = new ();
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;
        private bool foldOutSourceFolderPath = true;
        private CityLoadConfigGUI cityLoadConfigGUI;

        /// <summary>
        /// メインスレッドから呼ばれることを前提とします。
        /// </summary>
        public CityImportLocalGUI(UnityEditor.EditorWindow parentEditorWindow)
        { 
            progressGUI ??= new ProgressDisplayGUI(parentEditorWindow);
            progressGUI.ParentEditorWindow = parentEditorWindow;
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
            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes, this.config.DatasetSourceConfig,
                    this, this.config.CoordinateZoneID);


            if (isAreaSelectComplete)
            {
                PlateauEditorStyle.Heading("地物別設定", "num3.png");
                this.cityLoadConfigGUI?.Draw(this.config);
                
                PlateauEditorStyle.Separator(0);
                PlateauEditorStyle.Separator(0);
                
                ImportButton.Draw(this.config, progressGUI);
            }
            
            PlateauEditorStyle.Separator(0);
            progressGUI.Draw();
        }

        public void ReceiveResult(AreaSelectResult result)
        {
            this.config.InitWithAreaSelectResult(result);
            this.cityLoadConfigGUI = new CityLoadConfigGUI(result.PackageToLodDict);
        }
    }
}
