using System.Threading;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportLocalGUI : IEditorDrawable, IAreaSelectResultReceiver
    {
        private readonly PathSelectorFolderPlateauInput folderSelector = new PathSelectorFolderPlateauInput();
        private readonly CityLoadConfig config = new CityLoadConfig();
        private bool isAreaSelectComplete;

        
        public void Draw()
        {
            this.config.SourcePathBeforeImport = this.folderSelector.Draw("入力フォルダ");

            PlateauEditorStyle.Separator(0);
            PlateauEditorStyle.SubTitle("モデルデータの配置を行います。");
            PlateauEditorStyle.Heading("基準座標系の選択", null);

            // 基準座標系についてはこのWebサイトを参照してください。
            // https://www.gsi.go.jp/sokuchikijun/jpc.html

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.config.CoordinateZoneID = EditorGUILayout.Popup(
                    "基準座標系", this.config.CoordinateZoneID - 1, 
                    new[] {
                        "01: 長崎, 鹿児島(南西部)",
                        "02: 福岡, 佐賀, 熊本, 大分, 宮崎, 鹿児島(北東部)",
                        "03: 山口, 島根, 広島",
                        "04: 香川, 愛媛, 徳島, 高知",
                        "05: 兵庫, 鳥取, 岡山",
                        "06: 京都, 大阪, 福井, 滋賀, 三重, 奈良, 和歌山",
                        "07: 石川, 富山, 岐阜, 愛知",
                        "08: 新潟, 長野, 山梨, 静岡",
                        "09: 東京(本州), 福島, 栃木, 茨城, 埼玉, 千葉, 群馬, 神奈川",
                        "10: 青森, 秋田, 山形, 岩手, 宮城",
                        "11: 北海道(西部)",
                        "12: 北海道(中央部)",
                        "13: 北海道(東部)",
                        "14: 諸島(東京南部)",
                        "15: 沖縄",
                        "16: 諸島(沖縄西部)",
                        "17: 諸島(沖縄東部)",
                        "18: 小笠原諸島",
                        "19: 南鳥島"
                    }) + 1; // 番号は 1 スタート
            }
            

            PlateauEditorStyle.Heading("マップ範囲選択", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (PlateauEditorStyle.MainButton("範囲選択"))
                {
                    AreaSelectorStarter.Start(this.config.SourcePathBeforeImport, this, this.config.CoordinateZoneID);
                }
                this.isAreaSelectComplete = this.config.AreaMeshCodes != null && this.config.AreaMeshCodes.Length > 0;
                // using (new EditorGUILayout.HorizontalScope())
                // {
                //     EditorGUILayout.Space();
                //     EditorGUILayout.LabelField();
                //     EditorGUILayout.Space();
                // }
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    string str = this.isAreaSelectComplete ? "範囲選択 : セット済" : "範囲選択 : 未";
                    PlateauEditorStyle.LabelSizeFit(new GUIContent(str), EditorStyles.label);
                });
            }
            

            

            if (this.isAreaSelectComplete)
            {
                PlateauEditorStyle.Heading("地物別設定", null);
                CityLoadConfigGUI.Draw(this.config);
                
                PlateauEditorStyle.Separator(0);
                
                PlateauEditorStyle.Separator(0);
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    if (PlateauEditorStyle.MainButton("インポート"))
                    {
                        var mainThreadContext = SynchronizationContext.Current;
                        var task = CityImporter.ImportAsync(this.config, ProgressDisplayWindow.Open(mainThreadContext));
                        task.ContinueWithErrorCatch();
                    }
                }
            }
            
        }

        public void ReceiveResult(string[] areaMeshCodes, Extent extent, PredefinedCityModelPackage availablePackageFlags)
        {
            this.config.InitWithPackageFlags(availablePackageFlags);
            this.config.AreaMeshCodes = areaMeshCodes;
            this.config.Extent = extent;
        }
    }
}
