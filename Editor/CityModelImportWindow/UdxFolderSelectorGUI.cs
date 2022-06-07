using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// udxフォルダを選択するGUIを提供します。
    /// </summary>
    public class UdxFolderSelectorGUI : IEditorWindowContents
    {
        private string udxFolderPath;
        private readonly GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
        
        private string exportFolderPath;
        private GmlSelectorGUI gmlSelectorGUI;

        public UdxFolderSelectorGUI()
        {
            this.gmlSelectorGUI = new GmlSelectorGUI(this.gmlFileSearcher);
        }

        public void DrawGUI()
        {
            HeaderDrawer.Draw("取得データ選択");
            using (new EditorGUILayout.HorizontalScope())
            {
                this.udxFolderPath = EditorGUILayout.TextField("インポートフォルダ", this.udxFolderPath);
                if (PlateauEditorStyle.MainButton("参照..."))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("udxフォルダ選択", Application.dataPath, "udx");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        OnUdxPathChanged(selectedPath);
                    }
                }
            }

            if (GmlFileSearcher.IsPathUdx(this.udxFolderPath))
            {
                this.gmlSelectorGUI.DrawGUI();
                DrawExportPathSelectorGUI(this.gmlSelectorGUI.GmlFiles, ref this.exportFolderPath, this.udxFolderPath);
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
        }

        

        private static void DrawExportPathSelectorGUI(IEnumerable<string> gmlFiles, ref string exportFolderPath, string udxFolderPath)
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

        

        private void OnUdxPathChanged(string selectedPath)
        {
            this.udxFolderPath = selectedPath;
            this.gmlFileSearcher.GenerateFileDictionary(selectedPath);
            this.gmlSelectorGUI.OnUdxPathChanged();
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
        }
    }
}