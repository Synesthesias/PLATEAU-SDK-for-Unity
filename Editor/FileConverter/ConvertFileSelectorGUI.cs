using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// ファイルを受け取り、変換してファイルを出力する各EditorWindowのうち、共通機能をまとめたクラスです。
    /// 次の機能とGUIの描画を提供します:
    /// ・変換元ファイルの選択　・変換後ファイルのパスの選択　・変換ボタン
    /// </summary>
    public class ConvertFileSelectorGUI {
        private string sourceFilePath;
        private string destinationFilePath;
        private const float spaceWidth = 20f;

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        public void OnEnable() {
            DefaultPathIfEmpty(ref this.sourceFilePath);
            DefaultPathIfEmpty(ref this.destinationFilePath);
        }

        /// <summary>
        /// 変換元ファイルを選択するGUIを表示します。
        /// </summary>
        public void SourceFileSelectMenu(string srcFileExtension) {
            PlateauEditorStyle.Heading1($"1. Select {srcFileExtension} File");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton($"Select {srcFileExtension} File")) {
                    ButtonSelectGMLFilePushed(srcFileExtension);
                }
                GUILayout.Label($"{srcFileExtension} file path:");
                GUILayout.TextArea($"{this.sourceFilePath}");
                Space();
            }
        }

        /// <summary>
        /// 変換後ファイルのパスを選択するGUIを表示します。
        /// </summary>
        public void DestinationFileSelectMenu(string dstFileExtension) {
            PlateauEditorStyle.Heading1($"2. Select {dstFileExtension} File Destination");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton($"Select {dstFileExtension} Destination")) {
                    ButtonSelectDestinationPushed(dstFileExtension);
                }
                GUILayout.Label($"Destination {dstFileExtension} file path:");
                GUILayout.TextArea($"{this.destinationFilePath}");
                Space();
            }
        }

        /// <summary>
        /// 変換ボタンを表示します。
        /// ボタンが押された時、引数の converter.Convert メソッドが実行されます。
        /// </summary>
        public void PrintConvertButton(IFileConverter converter) {
            PlateauEditorStyle.Heading1("4. Convert");
            using (new EditorGUILayout.VerticalScope(PlateauEditorStyle.BoxStyle)) {
                if (PlateauEditorStyle.MainButton("Convert")) {
                    ButtonConvertPushed(converter);
                }
            }
        }
        
        /// <summary>
        /// パスのデフォルト値は、ファイル選択画面で最初に表示されるディレクトリに影響します。
        /// Assetsフォルダを起点にしたほうが操作性が良さそうなので、そのようにデフォルト値を設定します。
        /// </summary>
        private static void DefaultPathIfEmpty(ref string path) {
            if (string.IsNullOrEmpty(path)) {
                path = Application.dataPath; // Assetsフォルダ
            }
        }
        
        
        /// <summary> ボタン押下時に呼ばれます。入力ファイルを選択するウィンドウを出します。 </summary>
        private void ButtonSelectGMLFilePushed(string fileExtension) {
            string path = EditorUtility.OpenFilePanel($"Select {fileExtension} File", this.sourceFilePath, fileExtension);
            if (string.IsNullOrEmpty(path)) return;
            this.sourceFilePath = path;
        }
        
        
        /// <summary> ボタン押下時に呼ばれます。ファイルの出力先を選択するウィンドウを出します。 </summary>
        private void ButtonSelectDestinationPushed(string fileExtension) {
            string path = EditorUtility.SaveFilePanel(
                "Select Destination",
                Path.GetDirectoryName(this.sourceFilePath),
                "exported", fileExtension
            );
            if (string.IsNullOrEmpty(path)) return;
            this.destinationFilePath = path;
        }
        
        
        /// <summary> ボタン押下時に呼ばれます。引数の converter に変換を委譲し、結果を表示します。 </summary>
        private void ButtonConvertPushed(IFileConverter converter) {
            bool result = converter.Convert(this.sourceFilePath, this.destinationFilePath);
            EditorUtility.DisplayDialog(
                "Convert Result",
                result ? "Convert Complete!" : "Convert Failed...\nSee console log for detail.",
                "OK");
            if (result) {
                AssetDatabase.Refresh();
            }
            
        }
        
        /// <summary> 空白を表示します。 </summary>
        public static void Space() {
            EditorGUILayout.Space(spaceWidth);
        }
    }
}
