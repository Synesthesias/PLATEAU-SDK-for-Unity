using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.GUIContents;
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

        [MenuItem("Plateau/単品ファイル変換")]
        public static void Open()
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
                new GmlToObjAndIdTableConvertTab(),
                new GmlToObjFileConvertTab(),
                new GmlToCityMapInfoConvertTab(),
                new ObjToFbxFileConvertTab()
            };
            this.tabNames = new[]
            {
                "GML → OBJ & CityMapInfo ", "GML → OBJ", "GML → CityMapInfo", "OBJ → FBX"
            };
            this.isInitialized = true;
        }

        /// <summary>
        /// GUI描画のメインメソッドです。
        /// </summary>
        private void OnGUI()
        {
            if (!this.isInitialized || this.tabContents == null) Init();
            HeaderDrawer.Reset();
            HeaderDrawer.Draw("Choose Convert Type");

            // タブ形式のボタンで変換のファイル形式を選びます。
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);

            // 選ばれたタブの内容を描画します。描画は tabContents の要素に委譲します。
            var tabContent = this.tabContents[this.tabIndex];
            tabContent.DrawGUI();
        }
    }
}