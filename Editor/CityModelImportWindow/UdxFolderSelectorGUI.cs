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

        private readonly GmlSelectorGUI gmlSelectorGUI;
        private readonly CityModelExportPathSelectorGUI cityModelExportPathSelectorGUI;

        public UdxFolderSelectorGUI()
        {
            this.gmlSelectorGUI = new GmlSelectorGUI(this.gmlFileSearcher);
            this.cityModelExportPathSelectorGUI = new CityModelExportPathSelectorGUI();
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
                var gmlFiles = this.gmlSelectorGUI.Select();
                this.cityModelExportPathSelectorGUI.DrawGUI(gmlFiles, this.udxFolderPath);
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
        }



        private void OnUdxPathChanged(string selectedPath)
        {
            this.udxFolderPath = selectedPath;
            this.gmlFileSearcher.GenerateFileDictionary(selectedPath);
            this.gmlSelectorGUI.OnUdxPathChanged();
        }
        
    }
}