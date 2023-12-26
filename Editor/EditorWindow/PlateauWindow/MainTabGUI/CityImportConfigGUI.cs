using System.Runtime.CompilerServices;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// インポート画面の設定GUIです。
    /// </summary>
    public class CityImportConfigGUI : IAreaSelectResultReceiver, IEditorDrawable
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
        private CityImportConfigGUI(IConfigGUIBeforeAreaSelect configGUIBeforeAreaSelect, UnityEditor.EditorWindow parentEditorWindow)
        {
            this.configGUIBeforeAreaSelect = configGUIBeforeAreaSelect;
            progressGUI = new ProgressDisplayGUI(parentEditorWindow);
        }

        /// <summary>
        /// ローカルインポートの設定GUIを作ります。
        /// </summary>
        public static CityImportConfigGUI CreateLocal(UnityEditor.EditorWindow parentWindow)
        {
            return new CityImportConfigGUI(new ConfigGUIBeforeAreaSelectLocal(), parentWindow);
        }

        /// <summary>
        /// サーバーインポートの設定GUIを作ります。
        /// </summary>
        public static CityImportConfigGUI CreateRemote(UnityEditor.EditorWindow parentWindow)
        {
            return new CityImportConfigGUI(new ConfigGUIBeforeAreaSelectRemote(parentWindow), parentWindow);
        }

        public void Draw()
        {
            config.ConfBeforeAreaSelect = this.configGUIBeforeAreaSelect.Draw();

            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");

            bool isAreaSelectComplete = AreaSelectButton.Draw(this.config.AreaMeshCodes,
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