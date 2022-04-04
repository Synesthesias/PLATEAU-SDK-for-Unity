using UnityEditor;

namespace PlateauUnitySDK.Editor.FileConverter {
    /// <summary>
    /// 3Dモデルのファイル形式を変換するEditorWindowです。
    /// 変換できる形式は次の2つです。
    /// ・GMLからOBJに
    /// ・OBJからFBXに
    /// </summary>
    public class ModelFileConvertWindow : EditorWindow {
        private IEditorWindowContents[] tabContents;
        private int tabIndex;
        private bool isInitialized;

        [MenuItem("Plateau/Model File Converter Window")]
        private static void Open() {
            var window = GetWindow<ModelFileConvertWindow>("Model File Converter");
            window.Show();
            window.Init();
        }

        private void Init() {
            // タブごとのGUIの内容をここで指定します。
            this.tabContents = new IEditorWindowContents[] {
                new GmlToObjFileConvertTab(),
                new ObjToFbxFileConvertTab()
            };
            this.isInitialized = true;
        }

        /// <summary>
        /// GUI描画のメインメソッドです。
        /// </summary>
        private void OnGUI() {
            if(!this.isInitialized || this.tabContents == null) Init();
            PlateauEditorStyle.Heading1("1. Choose Convert Type");
            
            // タブ形式のボタンで変換のファイル形式を選びます。
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, "GML to OBJ", "OBJ to FBX");
            
            // 選ばれたタブの内容を描画します。描画は tabContents の要素に委譲します。
            var tabContent = this.tabContents[this.tabIndex];
            tabContent.DrawGUI();
        }
    }
}