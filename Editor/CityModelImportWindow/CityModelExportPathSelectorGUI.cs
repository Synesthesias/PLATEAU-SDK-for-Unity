using System.Collections.Generic;
using System.IO;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    public class CityModelExportPathSelectorGUI
    {
        private string exportFolderPath;
        
        public void DrawGUI(List<string> gmlFiles, string udxFolderPath)
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
                OnExportButtonPushed(gmlFiles, udxFolderPath, exportFolderPath);
            }
        }
        
        private static void OnExportButtonPushed(IEnumerable<string> gmlFiles, string udxFolderPath, string exportFolderPath)
        {
            foreach (var gmlRelativePath in gmlFiles)
            {
                // TODO Configを設定できるようにする
                string gmlFullPath = Path.GetFullPath(Path.Combine(udxFolderPath, gmlRelativePath));
                string gmlFileName = Path.GetFileNameWithoutExtension(gmlRelativePath);
                string objPath = Path.Combine(exportFolderPath, gmlFileName + ".obj");
                string idTablePath = Path.Combine(exportFolderPath, "idToFileTable.asset");
                var objConverter = new GmlToObjFileConverter();
                var idTableConverter = new GmlToIdFileTableConverter();
                objConverter.Convert(gmlFullPath, objPath);
                idTableConverter.Convert(gmlFullPath, idTablePath);
            }
            AssetDatabase.Refresh();
        }
    }
}