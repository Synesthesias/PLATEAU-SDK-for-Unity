﻿using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Native;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportLocalGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private readonly PathSelectorFolderPlateauInput folderSelector = new PathSelectorFolderPlateauInput();
        private readonly CityLoadConfig config = new CityLoadConfig();
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;
        private bool foldOutSourceFolderPath = true;

        

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
                this.config.DatasetSourceConfig ??= new DatasetSourceConfig(false, "", "");
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
                CityLoadConfigGUI.Draw(this.config);
                
                PlateauEditorStyle.Separator(0);
                PlateauEditorStyle.Separator(0);
                
                ImportButton.Draw(this.config, progressGUI);
            }
            
            PlateauEditorStyle.Separator(0);
            progressGUI.Draw();
        }

        public void ReceiveResult(string[] areaMeshCodes, Extent extent, PredefinedCityModelPackage availablePackageFlags)
        {
            this.config.InitWithPackageFlags(availablePackageFlags);
            this.config.AreaMeshCodes = areaMeshCodes;
            this.config.Extent = extent;
        }
    }
}
