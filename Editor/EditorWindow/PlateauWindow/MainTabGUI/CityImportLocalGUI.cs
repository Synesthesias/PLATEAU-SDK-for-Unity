using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Native;
using UnityEditor;

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
            // TODO この実装、CityImportRemoteGUI とかぶってる
            this.foldOutSourceFolderPath = PlateauEditorStyle.FoldOut(this.foldOutSourceFolderPath, "入力フォルダ", () =>
            {
                this.config.DatasetSourceConfig ??= new DatasetSourceConfig(false, "", "", "", "");
                this.config.DatasetSourceConfig.LocalSourcePath = this.folderSelector.Draw("フォルダパス");
            });
            
            PlateauEditorStyle.Separator(0);
            PlateauEditorStyle.SubTitle("モデルデータの配置を行います。");
            PlateauEditorStyle.Heading("基準座標系の選択", "num1.png");

            // 基準座標系についてはこのWebサイトを参照してください。
            // https://www.gsi.go.jp/sokuchikijun/jpc.html
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.config.CoordinateZoneID = EditorGUILayout.Popup(
                    "基準座標系", this.config.CoordinateZoneID - 1, 
                    GeoReference.ZoneIdExplanation
                    ) + 1; // 番号は 1 スタート
            }
            

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
            // TODO ここの実装、Remoteと被ってる。 ReceiveResult の引数を1つのクラスにまとめたうえで、ここも1つのメソッドで済ませたほうが良い。
            
            this.config.InitWithAreaSelectResult(result);
            this.cityLoadConfigGUI = new CityLoadConfigGUI(result.PackageToLodDict);
        }
    }
}
