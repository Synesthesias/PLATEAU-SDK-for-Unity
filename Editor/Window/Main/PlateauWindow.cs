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
        private PlateauWindowGui gui;

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
                new TabElementWithImage("dark_icon_import.png", new CityAddGui(this)),
                // モデル調整
                new TabElementWithImage("dark_icon_modify.png",
                    // モデル調整内の入れ子タブ
                    new TabWithFrame(
                        new TabElement("Assetsに保存", new ConvertToAssetGui()),
                        new TabElement("ゲームオブジェクト\nON/OFF", new CityChangeActiveGui()),
                        new TabElement("分割/結合/マテリアル分け", new CityMaterialAdjustPresenter(this)),
                        new TabElement("地形変換/高さ合わせ", new CityTerrainConvertGui(this)),
                        new TabElement("道路調整",
                            // 道路調整内の入れ子タブ
                            new TabWithFrame(
                                new TabElement("路面標示生成", new RoadMarkingGeneratorPresenter()),
                                new TabElement("レーン見た目補正", new RoadLaneEnhancePresenter()),
                                new TabElement("道路変更", new RoadChangePresenter()))
                        )
                    )
                ),
                // エクスポート
                new TabElementWithImage("dark_icon_export.png", new CityExportGui()),
                // 属性情報
                new TabElementWithImage("dark_icon_information.png", new CityAttributeGui(this))
            );
            return new PlateauWindowGui(tabs);
        }

        protected override IEditorDrawable InitFooterGui()
        {
            return new PlateauWindowFooterGui();
        }

        /// <summary> テストからアクセスする用 </summary>
        internal const string NameOfInnerGuiField = nameof(gui);
    }
}
