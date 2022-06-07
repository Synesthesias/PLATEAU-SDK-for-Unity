using System;
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
        private GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
        private bool[] areaIdCheckboxes;
        private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();
        private string exportFolderPath;
        private List<string> gmlFiles;

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
                DrawGmlTargetSelectorGUI(this.gmlFileSearcher);
                DrawExportPathSelectorGUI(this.gmlFiles, this.exportFolderPath, this.udxFolderPath);
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
        }

        private void DrawGmlTargetSelectorGUI(GmlFileSearcher gmlSearcher)
        {
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            var areaIds = gmlSearcher.AreaIds;
            int areaCount = areaIds.Length;
            for (int i = 0; i < areaCount; i++)
            {
                this.areaIdCheckboxes[i] = EditorGUILayout.Toggle(areaIds[i], this.areaIdCheckboxes[i]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    for (int i = 0; i < this.areaIdCheckboxes.Length; i++) this.areaIdCheckboxes[i] = true;
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    for (int i = 0; i < this.areaIdCheckboxes.Length; i++) this.areaIdCheckboxes[i] = false;
                }
            }
            
            HeaderDrawer.Draw("含める地物");
            var typeDict = this.gmlTypeTarget.TargetDict;
            foreach (var gmlType in typeDict.Keys.ToArray())
            {
                typeDict[gmlType] = EditorGUILayout.Toggle(GmlTypeConvert.ToDisplay(gmlType), typeDict[gmlType]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    this.gmlTypeTarget.SetAll(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    this.gmlTypeTarget.SetAll(false);
                }
            }


            HeaderDrawer.Draw("対象gmlファイル");
            this.gmlFiles = ListTargetGmlFiles(this.gmlFileSearcher, areaIds, this.areaIdCheckboxes, this.gmlTypeTarget);
            EditorGUILayout.TextArea(String.Join("\n", this.gmlFiles));
            HeaderDrawer.DecrementDepth();
        }

        private static void DrawExportPathSelectorGUI(List<string> gmlFiles, string exportFolderPath, string udxFolderPath)
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

        private static List<string> ListTargetGmlFiles(GmlFileSearcher gmlSearcher, string[] areaIds, bool[] areaCheckboxes, GmlTypeTarget gmlTypeTarget)
        {
            if (areaIds.Length != areaCheckboxes.Length)
            {
                throw new ArgumentException("areaId.Length does not match areaCheckboxes.Length.");
            }

            int areaCount = areaIds.Length;
            var gmlFiles = new List<string>();
            for (int i = 0; i < areaCount; i++)
            {
                if (!areaCheckboxes[i]) continue;
                gmlFiles.AddRange(gmlSearcher.GetGmlFilePathsForAreaIdAndType(areaIds[i], gmlTypeTarget, false));
            }

            return gmlFiles;
        }

        private void OnUdxPathChanged(string selectedPath)
        {
            this.udxFolderPath = selectedPath;
            this.gmlFileSearcher.GenerateFileDictionary(selectedPath);
            this.areaIdCheckboxes = Enumerable.Repeat(true, this.gmlFileSearcher.AreaIds.Length).ToArray();
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