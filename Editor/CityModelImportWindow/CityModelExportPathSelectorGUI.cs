using System.Collections.Generic;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    public class CityModelExportPathSelectorGUI
    {
        private string exportFolderPath;
        private UdxConverter udxConverter = new UdxConverter();
        
        public void Draw(List<string> gmlFiles, string udxFolderPath)
        {
            HeaderDrawer.Draw("出力先選択");
            using (new EditorGUILayout.HorizontalScope())
            {
                exportFolderPath = EditorGUILayout.TextField("出力先フォルダ", exportFolderPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    exportFolderPath = EditorUtility.SaveFolderPanel("保存先選択", Application.dataPath, "PlateauData");
                }
            }
            HeaderDrawer.Draw("出力");
            if (PlateauEditorStyle.MainButton("出力"))
            {
                this.udxConverter.Convert(gmlFiles, udxFolderPath, exportFolderPath);
            }
        }
    }
}