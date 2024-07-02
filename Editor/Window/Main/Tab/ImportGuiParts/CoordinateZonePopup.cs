using PLATEAU.Editor.Window.Common;
using PLATEAU.Geometries;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
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
        /// 基準座標系を選択するGUIを表示し、結果をintで返します。
        /// </summary>
        public static int Draw(int current)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                return EditorGUILayout.Popup(
                    "基準座標系", current - 1, 
                    GeoReference.ZoneIdExplanation
                ) + 1; // 番号は 1 スタート
            }
        }
    }
}
