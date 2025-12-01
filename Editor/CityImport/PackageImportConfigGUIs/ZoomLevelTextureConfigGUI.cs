using PLATEAU.CityImport.Config;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs
{
    /// <summary>
    /// ズームレベル(ズームレベル9以外)ごとにテクスチャ解像度倍率(1倍, 1/2倍, 1/4倍)を設定するGUI
    /// </summary>
    internal class ZoomLevelTextureConfigGUI : IEditorDrawable
    {

        private static int[] zl11Denominators = new int[] { 1, 2 }; // 1倍, 1/2倍
        private static int[] zl10Denominators = new int[] { 2, 4 }; // 1/2, 1/4倍
        private static GUIContent[] zl11SelectOptions = new GUIContent[] { new GUIContent("1倍"), new GUIContent("1 \u2215 2倍") }; // ドロップダウン選択内容( 1倍, 1/2倍 )
        private static GUIContent[] zl10SelectOptions = new GUIContent[] { new GUIContent("1 \u2215 2倍"), new GUIContent("1 \u2215 4倍") }; // ドロップダウン選択内容( 1/2, 1/4倍 )
        private CityImportConfig conf;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conf">The city import configuration used to initialize the GUI settings.</param>
        public ZoomLevelTextureConfigGUI(CityImportConfig conf)
        {
            this.conf = conf;
        }

        public void Dispose(){}

        /// <summary>
        /// テクスチャ解像度設定のGUIを描画します。
        /// 設定された内容はconf<see cref="CityImportConfig"/>に反映されます。
        /// </summary>
        public void Draw()
        {
            EditorGUILayout.LabelField("テクスチャ解像度設定", GUILayout.Width(120));

            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter // 中央寄せ
            };
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("ズームレベル11（カメラが近いとき）");
                    GUILayout.FlexibleSpace();
                    var currentIndex = GetIndexFromDenominator(conf.DynamicTileImportConfig.ZoomLevel11TextureResolutionDenominator, 11);
                    var newIndex = EditorGUILayout.Popup(currentIndex, zl11SelectOptions, popupStyle, GUILayout.Width(80));
                    conf.DynamicTileImportConfig.ZoomLevel11TextureResolutionDenominator = GetDenominatorFromIndex(newIndex, 11);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("ズームレベル10（カメラがやや遠いとき）");
                    GUILayout.FlexibleSpace();
                    var currentIndex = GetIndexFromDenominator(conf.DynamicTileImportConfig.ZoomLevel10TextureResolutionDenominator, 10);
                    var newIndex = EditorGUILayout.Popup(currentIndex, zl10SelectOptions, popupStyle, GUILayout.Width(80));
                    conf.DynamicTileImportConfig.ZoomLevel10TextureResolutionDenominator = GetDenominatorFromIndex(newIndex, 10);
                }
            }
        }

        /// <summary>
        /// ズームレベルに応じた配列から、指定されたゼロベースインデックスの分母を取得します。
        /// </summary>
        /// <param name="index"><see cref="zl10Denominators"/>又は、<see cref="zl11Denominators"/>の配列のindex</param>
        /// <param name="zoomLevel">ズームレベル( 10 ,11 )</param>
        /// <returns><see cref="zl10Denominators"/>又は、<see cref="zl11Denominators"/>で定義された分母の値。取得できない場合は、デフォルト値を返す</returns>
        private int GetDenominatorFromIndex(int index, int zoomLevel)
        {
            int[] denominators = null;

            if (zoomLevel == 10)
                denominators = zl10Denominators;
            else if (zoomLevel == 11)
                denominators = zl11Denominators;

            if (denominators != null && index >= 0 && index < denominators.Length)
                return denominators[index];

            return DynamicTileTool.GetDefaultDenominatorFromZoomLevel(zoomLevel);
        }

        /// <summary>
        /// ズームレベルに応じた配列から、指定された分母のゼロベースインデックスを取得します。
        /// </summary>
        /// <param name="denominator"><see cref="zl10Denominators"/>又は、<see cref="zl11Denominators"/>で定義された分母の値</param>
        /// <param name="zoomLevel">ズームレベル( 10 ,11 )</param>
        /// <returns><see cref="zl10Denominators"/>又は、<see cref="zl11Denominators"/>で定義された分母の値の配列上のインデックス。取得できない場合は、0を返す</returns>
        private int GetIndexFromDenominator(int denominator, int zoomLevel)
        {
            int[] denominators = null;

            if (zoomLevel == 10)
                denominators = zl10Denominators;
            else if (zoomLevel == 11)
                denominators = zl11Denominators;

            if (denominators == null || denominators.Length == 0)
                return 0;

            int index = Array.IndexOf(denominators, denominator);
            return index >= 0 ? index : 0;
        }
    }
}
