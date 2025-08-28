using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.Window.Main.Tab.ImportGuiParts;
using PLATEAU.Editor.Window.ProgressDisplay;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// インポート画面の設定GUIです。
    /// </summary>
    public class CityImportConfigGui : IAreaSelectResultReceiver, IEditorDrawable
    {
        private CityImportConfig config = CityImportConfig.CreateDefault();
        private readonly IConfigGUIBeforeAreaSelect configGUIBeforeAreaSelect;

        /// <summary> GUIのうち、範囲選択後に表示する部分です。 </summary>
        private ImportGUIAfterAreaSelect guiAfterAreaSelect;

        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static ProgressDisplayGUI progressGUI;

        /// <summary>
        /// コンストラクタです。
        /// 範囲選択するより前のインポート設定GUIはローカルかリモートかによって異なるので、依存性注入で渡します。
        /// </summary>
        private CityImportConfigGui(IConfigGUIBeforeAreaSelect configGUIBeforeAreaSelect, UnityEditor.EditorWindow parentEditorWindow)
        {
            this.configGUIBeforeAreaSelect = configGUIBeforeAreaSelect;
            progressGUI = new ProgressDisplayGUI(parentEditorWindow);
        }

        /// <summary>
        /// ローカルインポートの設定GUIを作ります。
        /// </summary>
        public static CityImportConfigGui CreateLocal(UnityEditor.EditorWindow parentWindow)
        {
            return new CityImportConfigGui(new ConfigGUIBeforeAreaSelectLocal(), parentWindow);
        }

        /// <summary>
        /// サーバーインポートの設定GUIを作ります。
        /// </summary>
        public static CityImportConfigGui CreateRemote(UnityEditor.EditorWindow parentWindow)
        {
            return new CityImportConfigGui(new ConfigGUIBeforeAreaSelectRemote(parentWindow), parentWindow);
        }

        public void Draw()
        {
            // 範囲選択前のGUI
            config.ConfBeforeAreaSelect = this.configGUIBeforeAreaSelect.Draw();

            // 動的タイル設定GUI（範囲選択ボタンの前に表示）
            new DynamicTileConfigGUI(this.config).Draw();

            // 範囲選択ボタン
            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaGridCodes,
                this.config.ConfBeforeAreaSelect,
                this);

            // 範囲選択後のGUI
            if (isAreaSelectComplete)
            {
                this.guiAfterAreaSelect.Draw();
            }

            progressGUI.Draw();
        }

        public void Dispose()
        {
        }

        public void ReceiveResult(AreaSelectResult result)
        {
            this.config = CityImportConfig.CreateWithAreaSelectResult(result);
            this.guiAfterAreaSelect = new ImportGUIAfterAreaSelect(this.config, result.PackageToLodDict, progressGUI);
        }

        // テスト用
        internal static string NameOfConfigGUIBeforeAreaSelect => nameof(configGUIBeforeAreaSelect);
    }
}