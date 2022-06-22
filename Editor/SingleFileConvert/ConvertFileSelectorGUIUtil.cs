using System;
using System.IO;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.SingleFileConvert
{
    /// <summary>
    /// ファイルを受け取り、変換してファイルを出力する各EditorWindowのうち、共通機能をまとめたクラスです。
    /// 次の機能とGUIの描画を提供します:
    /// ・変換元ファイルの選択　・変換後ファイルのパスの選択　・変換ボタン
    /// </summary>
    internal static class ConvertFileSelectorGUIUtil
    {
        private const float spaceWidth = 20f;
        
        public static string DefaultPath()
        {
            return Application.dataPath;
        }
        
        public enum FilePanelType{Open, Save}

        /// <summary>
        /// <see cref="EditorWindow"/> 向けにファイル選択のGUIを表示します。
        /// </summary>
        public static void FileSelectGUI(ref string filePath, string extension, FilePanelType panelType, string description)
        {
            HeaderDrawer.Draw(description);
            using (PlateauEditorStyle.VerticalScope())
            {
                if (PlateauEditorStyle.MainButton(description))
                {
                    string path;
                    switch (panelType)
                    {
                        case FilePanelType.Open:
                            path = EditorUtility.OpenFilePanel(description, filePath, extension);
                            break;
                        case FilePanelType.Save:
                            path = EditorUtility.SaveFilePanel(
                                description,
                                Path.GetDirectoryName(filePath),
                                "exported", extension
                            );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(panelType)}", "unknown panel type.");
                    }
                    if (string.IsNullOrEmpty(path)) return;
                    filePath = path;
                }
                GUILayout.Label($"{extension} file path:");
                GUILayout.TextArea($"{filePath}");
                Space();
            }
        }

        /// <summary>
        /// 変換ボタンを表示します。
        /// ボタンが押された時、引数の converter.Convert メソッドが実行されます。
        /// </summary>
        public static void PrintConvertButton(Func<bool> convertFunc)
        {
            HeaderDrawer.Draw("Convert");
            using (PlateauEditorStyle.VerticalScope())
            {
                if (PlateauEditorStyle.MainButton("Convert")) ButtonConvertPushed(convertFunc);
            }
        }

        /// <summary> ボタン押下時に呼ばれます。引数の converter に変換を委譲し、結果を表示します。 </summary>
        private static void ButtonConvertPushed(Func<bool> convertFunc)
        {
            var result = convertFunc();
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