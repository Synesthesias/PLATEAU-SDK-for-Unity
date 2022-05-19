using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.GUITabs;
using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter
{
    /// <summary>
    /// ファイルの変換を行うEditorWindowです。
    /// 変換の形式ごとのタブが用意されたウィンドウを表示します。
    /// 具体的な処理を各タブのクラスに委譲します。
    /// </summary>
    public class ModelFileConvertWindow : EditorWindow
    {
        private IEditorWindowContents[] tabContents;
        private string[] tabNames;
        private int tabIndex;
        private bool isInitialized;

        [MenuItem("Plateau/Model File Converter Window")]
        private static void Open()
        {
            var window = GetWindow<ModelFileConvertWindow>("Model File Converter");
            window.Show();
            window.Init();
        }

        private void Init()
        {
            // タブごとのGUIの内容をここで指定します。
            this.tabContents = new IEditorWindowContents[]
            {
                new GmlToObjFileConvertTab(),
                new GmlToIdFileTableConvertTab(),
                new ObjToFbxFileConvertTab()
            };
            this.tabNames = new[]
            {
                "GML to OBJ", "GML to ID->File Table", "OBJ to FBX"
            };
            this.isInitialized = true;
        }

        /// <summary>
        /// GUI描画のメインメソッドです。
        /// </summary>
        private void OnGUI()
        {
            if (!this.isInitialized || this.tabContents == null) Init();
            PlateauEditorStyle.Heading1("1. Choose Convert Type");

            // タブ形式のボタンで変換のファイル形式を選びます。
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);

            // 選ばれたタブの内容を描画します。描画は tabContents の要素に委譲します。
            var tabContent = this.tabContents[this.tabIndex];
            tabContent.DrawGUI();
        }
    }
}