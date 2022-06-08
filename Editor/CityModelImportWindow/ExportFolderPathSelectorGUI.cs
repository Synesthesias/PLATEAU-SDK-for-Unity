using System.Collections.Generic;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// 出力先フォルダ選択GUIを表示します。
    /// </summary>
    public class ExportFolderPathSelectorGUI
    {
        private string exportFolderPath;
        
        /// <summary>
        /// GUIを表示し、選択されたパスを返します。
        /// </summary>
        public string Draw()
        {
            HeaderDrawer.Draw("出力先選択");
            using (new EditorGUILayout.HorizontalScope())
            { 
                this.exportFolderPath = EditorGUILayout.TextField("出力先フォルダ", this.exportFolderPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    this.exportFolderPath = EditorUtility.SaveFolderPanel("保存先選択", Application.dataPath, "PlateauData");
                }
            }

            return this.exportFolderPath;
        }
    }
}