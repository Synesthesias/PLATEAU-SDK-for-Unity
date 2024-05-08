using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAU SDK ウィンドウのエントリーポイントです。
    /// </summary>
    internal class PlateauWindow : PlateauWindowBase
    {
        private readonly ScrollView scrollView = new ();
        private PlateauWindowGUI gui;

        [MenuItem("PLATEAU/PLATEAU SDK")]
        public static void Open()
        {
            var window = GetWindow<PlateauWindow>("PLATEAU SDK");
            window.Show();
        }
        
        protected override IEditorDrawable InitGui()
        {
            // タブの内容を依存性注入により定義します。
            var tabs = new TabWithImage(
                80,
                // インポート
                new TabElementWithImage("dark_icon_import.png", new CityAddGUI(this)),
                // モデル調整
                new TabElementWithImage("dark_icon_modify.png",
                    // 入れ子タブ
                    new TabWithFrame(
                        new TabElement("Assetsに保存", new ConvertToAssetGUI()),
                        new TabElement("ゲームオブジェクト\nON/OFF", new CityChangeActiveGUI()),
                        new TabElement("マテリアル分け", new CityMaterialAdjustGUI(this)),
                        new TabElement("結合/分離", new CityGranularityConvertGUI(this)))
                ),
                // エクスポート
                new TabElementWithImage("dark_icon_export.png", new CityExportGUI()),
                // 属性情報
                new TabElementWithImage("dark_icon_information.png", new CityAttributeGUI(this))
            );
            return new PlateauWindowGUI(tabs);
        }
        

        /// <summary> テストからアクセスする用 </summary>
        internal const string NameOfInnerGuiField = nameof(gui);
    }
}
