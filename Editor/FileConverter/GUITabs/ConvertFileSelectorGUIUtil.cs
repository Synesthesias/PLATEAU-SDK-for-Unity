using System.IO;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.GUITabs
{
    /// <summary>
    /// ファイルを受け取り、変換してファイルを出力する各EditorWindowのうち、共通機能をまとめたクラスです。
    /// 次の機能とGUIの描画を提供します:
    /// ・変換元ファイルの選択　・変換後ファイルのパスの選択　・変換ボタン
    /// </summary>
    public static class ConvertFileSelectorGUIUtil
    {
        private const float spaceWidth = 20f;

        /// <summary> 初期化処理のうち、Unityの仕様で Init に書けない部分をここに書きます。 </summary>
        public static void SetDefaultPath(ref string sourceFilePath, ref string destinationFilePath)
        {
            DefaultPathIfEmpty(ref sourceFilePath);
            DefaultPathIfEmpty(ref destinationFilePath);
        }

        /// <summary>
        /// 変換元ファイルを選択するGUIを表示します。
        /// </summary>
        public static void SourceFileSelectMenu(string srcFileExtension, ref string sourceFilePath)
        {
            PlateauEditorStyle.Heading1($"2. Select {srcFileExtension} File");
            using (PlateauEditorStyle.VerticalScope())
            {
                if (PlateauEditorStyle.MainButton($"Select {srcFileExtension} File"))
                    ButtonSelectGMLFilePushed(srcFileExtension, ref sourceFilePath);
                GUILayout.Label($"{srcFileExtension} file path:");
                GUILayout.TextArea($"{sourceFilePath}");
                Space();
            }
        }

        /// <summary>
        /// 変換後ファイルのパスを選択するGUIを表示します。
        /// </summary>
        public static void DestinationFileSelectMenu(ref string destinationFilePath, string dstFileExtension)
        {
            PlateauEditorStyle.Heading1($"3. Select {dstFileExtension} File Destination");
            using (PlateauEditorStyle.VerticalScope())
            {
                if (PlateauEditorStyle.MainButton($"Select {dstFileExtension} Destination"))
                    ButtonSelectDestinationPushed(ref destinationFilePath, dstFileExtension);
                GUILayout.Label($"Destination {dstFileExtension} file path:");
                GUILayout.TextArea($"{destinationFilePath}");
                Space();
            }
        }

        /// <summary>
        /// 変換ボタンを表示します。
        /// ボタンが押された時、引数の converter.Convert メソッドが実行されます。
        /// </summary>
        public static void PrintConvertButton(IFileConverter converter, string srcFilePath, string dstFilePath)
        {
            PlateauEditorStyle.Heading1("5. Convert");
            using (PlateauEditorStyle.VerticalScope())
            {
                if (PlateauEditorStyle.MainButton("Convert")) ButtonConvertPushed(converter, srcFilePath, dstFilePath);
            }
        }

        /// <summary>
        /// パスのデフォルト値は、ファイル選択画面で最初に表示されるディレクトリに影響します。
        /// Assetsフォルダを起点にしたほうが操作性が良さそうなので、そのようにデフォルト値を設定します。
        /// </summary>
        private static void DefaultPathIfEmpty(ref string path)
        {
            if (string.IsNullOrEmpty(path)) path = Application.dataPath; // Assetsフォルダ
        }


        /// <summary> ボタン押下時に呼ばれます。入力ファイルを選択するウィンドウを出します。 </summary>
        private static void ButtonSelectGMLFilePushed(string fileExtension, ref string sourceFilePath)
        {
            var path = EditorUtility.OpenFilePanel($"Select {fileExtension} File", sourceFilePath, fileExtension);
            if (string.IsNullOrEmpty(path)) return;
            sourceFilePath = path;
        }


        /// <summary> ボタン押下時に呼ばれます。ファイルの出力先を選択するウィンドウを出します。 </summary>
        private static void ButtonSelectDestinationPushed(ref string destinationFilePath, string fileExtension)
        {
            var path = EditorUtility.SaveFilePanel(
                "Select Destination",
                Path.GetDirectoryName(destinationFilePath),
                "exported", fileExtension
            );
            if (string.IsNullOrEmpty(path)) return; 
            destinationFilePath = path;
        }


        /// <summary> ボタン押下時に呼ばれます。引数の converter に変換を委譲し、結果を表示します。 </summary>
        private static void ButtonConvertPushed(IFileConverter converter, string sourceFilePath, string destinationFilePath)
        {
            var result = converter.Convert(sourceFilePath, destinationFilePath);
            EditorUtility.DisplayDialog(
                "Convert Result",
                result ? "Convert Complete!" : "Convert Failed...\nSee console log for detail.",
                "OK");
            if (result) AssetDatabase.Refresh();
        }

        /// <summary> 空白を表示します。 </summary>
        public static void Space()
        {
            EditorGUILayout.Space(spaceWidth);
        }
    }
}