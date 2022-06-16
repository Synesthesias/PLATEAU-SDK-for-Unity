using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// 出力先フォルダ選択GUIを表示します。
    /// </summary>
    public class ExportFolderPathSelectorGUI
    {
        // private string exportFolderPath;

        // public string ExportFolderPath
        // {
        //     set => this.exportFolderPath = value;
        // }

        /// <summary>
        /// GUIを表示し、選択されたパスを返します。
        /// </summary>
        public string Draw(string currentExportPath)
        {
            HeaderDrawer.Draw("出力先選択");
            using (new EditorGUILayout.HorizontalScope())
            { 
                currentExportPath = EditorGUILayout.TextField("出力先フォルダ", currentExportPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    currentExportPath = EditorUtility.SaveFolderPanel("保存先選択", Application.dataPath, "PlateauData");
                }
            }

            return currentExportPath;
        }
    }
}