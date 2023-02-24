using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Geometries;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts
{
    /// <summary>
    /// 基準座標系をポップアップで選択するGUIです。
    ///
    /// 基準座標系についてはこのWebサイトを参照してください。
    /// https://www.gsi.go.jp/sokuchikijun/jpc.html
    /// </summary>
    internal static class CoordinateZonePopup
    {
        /// <summary>
        /// 基準座標系を選択するGUIを表示し、結果を <see cref="CityLoadConfig"/> にセットします。
        /// </summary>
        public static void DrawAndSet(CityLoadConfig conf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                conf.CoordinateZoneID = EditorGUILayout.Popup(
                    "基準座標系", conf.CoordinateZoneID - 1, 
                    GeoReference.ZoneIdExplanation
                ) + 1; // 番号は 1 スタート
            }
        }
    }
}
