using System.Threading;
using PLATEAU.CityImport;
using PLATEAU.CityImport.Load;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// 都市モデルの読み込み設定をするGUIをインスペクタに表示し、
    /// ボタン操作に応じて選択画面の表示や読み込みを行います。
    /// </summary>
    [CustomEditor(typeof(PLATEAUCityModelLoader))]
    internal class PLATEAUCityModelLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityModelLoader)target;
            bool isAreaSelectComplete;

            HeaderDrawer.Reset();
            
            HeaderDrawer.Draw("基準座標系の選択");

            // 基準座標系についてはこのWebサイトを参照してください。
            // https://www.gsi.go.jp/sokuchikijun/jpc.html

            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                loader.CoordinateZoneID = EditorGUILayout.Popup(
                    "基準座標系", loader.CoordinateZoneID - 1, 
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
            

            HeaderDrawer.Draw("範囲選択");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (PlateauEditorStyle.MainButton("範囲選択"))
                {
                    AreaSelectorStarter.Start(loader.SourcePathBeforeImport, loader, loader.CoordinateZoneID);
                }
                isAreaSelectComplete = loader.AreaMeshCodes != null && loader.AreaMeshCodes.Length > 0;
                EditorGUILayout.LabelField(isAreaSelectComplete ? "範囲選択 : 済" : "範囲選択 : 未");
            }
            

            

            if (isAreaSelectComplete)
            {
                CityLoadConfigGUI.Draw(loader.CityLoadConfig);
                
                HeaderDrawer.Draw("インポート");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    if (PlateauEditorStyle.MainButton("インポート"))
                    {
                        var mainThreadContext = SynchronizationContext.Current;
                        CityImporter.ImportV2(loader, ProgressDisplayWindow.Open(mainThreadContext));
                    }
                }
            }
            
            HeaderDrawer.Draw("詳細データ");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("インポート前パス:");
                PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
                EditorGUILayout.LabelField("インポート後パス:");
                PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
                EditorGUILayout.LabelField("地域メッシュコード");
                PlateauEditorStyle.MultiLineLabelWithBox(DebugUtil.EnumerableToString(loader.AreaMeshCodes));
            }
            
        }
    }
}
