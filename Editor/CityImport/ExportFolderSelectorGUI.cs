using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// 出力先フォルダ選択GUIを表示します。
    /// </summary>
    internal class ExportFolderSelectorGUI
    {

        /// <summary>
        /// GUIを表示し、選択されたパスを返します。
        /// パスは Assets パスとなります。
        /// </summary>
        public string Draw(string exportAssetPath)
        {
            HeaderDrawer.Draw("出力先選択");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("出力先フォルダ");
                
                if (PlateauEditorStyle.MiniButton("参照..."))
                {
                    // ボタン押下時
                    string selectedFullPath =
                        EditorUtility.SaveFolderPanel("保存先選択", Application.dataPath, "PlateauData");
                    
                    // キャンセル時は変更なし（引数をそのまま返す）
                    if (string.IsNullOrEmpty(selectedFullPath))
                    {
                        return exportAssetPath;
                    }
                    
                    if (!PathUtil.IsSubDirectoryOfAssets(selectedFullPath))
                    {
                        EditorUtility.DisplayDialog("エラー", "出力先は Assets フォルダ内である必要があります。", "OK");
                        return exportAssetPath;
                    }
                    
                    exportAssetPath = PathUtil.FullPathToAssetsPath(selectedFullPath);
                }
                
                string displayExportPath = string.IsNullOrEmpty(exportAssetPath) ? "未選択" : exportAssetPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayExportPath);
            }

            return exportAssetPath;
        }
    }
}