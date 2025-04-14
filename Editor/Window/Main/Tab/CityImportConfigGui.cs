using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.Window.Common;
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
        /// コンストラクタです。、
        /// 範囲選択するより前のインポート設定GUIはローカルかリモートかによって異なるので、引数によって処理を分けます。
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
            config.ConfBeforeAreaSelect = this.configGUIBeforeAreaSelect.Draw();

            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaGridCodes,
                this.config.ConfBeforeAreaSelect,
                this);

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