using UnityEditor;
using UnityEngine;
using static PlateauUnitySDK.Editor.FileConverter.ObjToFbxFileConverter;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// gmlファイルを読んでobjファイルに変換して出力する機能を持ったウィンドウのタブです。
    /// </summary>
    public class ObjToFbxFileConvertTab : IEditorWindowContents {
        private readonly ConvertFileSelectorGUI fileSelectorGUI = new ConvertFileSelectorGUI();
        private readonly ObjToFbxFileConverter fileConverter;
        private Vector2 scrollPosition;
        private FbxFormat fbxFormat;
        
        /// <summary>初期化処理です。</summary>
        public ObjToFbxFileConvertTab() {
            this.fileConverter = new ObjToFbxFileConverter();
            this.fileConverter.SetConfig(this.fbxFormat);
            this.fileSelectorGUI.OnEnable();
        }


        /// <summary> GUI表示のメインメソッドです。 </summary>
        public void DrawGUI() {
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
           
            // ファイルの入出力指定のGUIを fileSelectorGUI に委譲して描画します。
            using (PlateauEditorStyle.VerticalScope()) {
                EditorGUILayout.LabelField("入力objファイルはAssetsフォルダ内のファイルのみ指定できますが、");
                EditorGUILayout.LabelField("出力fbxファイルはAssetsフォルダの外でも指定できます。");
            }
            this.fileSelectorGUI.SourceFileSelectMenu("obj");
            this.fileSelectorGUI.DestinationFileSelectMenu("fbx");

            // fbxファイル特有の設定をするGUIです。
            PlateauEditorStyle.Heading1("4. Configure");
            using (PlateauEditorStyle.VerticalScope()) {
                EditorGUI.BeginChangeCheck();
                this.fbxFormat = (FbxFormat)EditorGUILayout.EnumPopup("FBX Format",this.fbxFormat);
                if (EditorGUI.EndChangeCheck()) {
                    this.fileConverter.SetConfig(this.fbxFormat);
                }
            }
            ConvertFileSelectorGUI.Space();

            // 変換ボタンです。
            this.fileSelectorGUI.PrintConvertButton(this.fileConverter);
            
            EditorGUILayout.EndScrollView();
            
        }
        
        
    }
}
