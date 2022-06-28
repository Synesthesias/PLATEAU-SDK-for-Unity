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
                string displayExportPath = string.IsNullOrEmpty(exportAssetPath) ? "未選択" : exportAssetPath;
                PlateauEditorStyle.MultiLineLabelWithBox(displayExportPath);
                if (PlateauEditorStyle.MiniButton("参照..."))
                {
                    // ボタン押下時
                    string selectedFullPath =
                        EditorUtility.SaveFolderPanel("保存先選択", Application.dataPath, "PlateauData");
                    if (!PathUtil.IsSubDirectoryOfAssets(selectedFullPath))
                    {
                        EditorUtility.DisplayDialog("エラー", "出力先は Assets フォルダ内である必要があります。", "OK");
                        return "";
                    }
                    if (!string.IsNullOrEmpty(selectedFullPath))
                    {
                        exportAssetPath = PathUtil.FullPathToAssetsPath(selectedFullPath);
                    }
                }
            }

            return exportAssetPath;
        }
    }
}