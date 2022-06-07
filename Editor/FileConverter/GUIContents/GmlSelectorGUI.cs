using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.GUIContents
{

    /// <summary>
    /// udxフォルダ内から変換対象を選択するGUIを提供します。
    /// </summary>
    public class GmlSelectorGUI : IEditorWindowContents
    {
        private string udxFolderPath;
        private GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
        private bool[] areaIdCheckboxes;
        private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

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
                DrawAreaIdSelectorGUI(this.gmlFileSearcher);
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
        }

        private void DrawAreaIdSelectorGUI(GmlFileSearcher gmlSearcher)
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
            var gmlFiles = ListTargetGmlFiles(this.gmlFileSearcher, areaIds, this.areaIdCheckboxes);
            EditorGUILayout.TextArea(String.Join("\n", gmlFiles));
            HeaderDrawer.DecrementDepth();
        }

        private List<string> ListTargetGmlFiles(GmlFileSearcher gmlSearcher, string[] areaIds, bool[] areaCheckboxes)
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
                gmlFiles.AddRange(gmlSearcher.GetGmlFilePathsForAreaIdAndType(areaIds[i], this.gmlTypeTarget, false));
            }

            return gmlFiles;
        }

        private void OnUdxPathChanged(string selectedPath)
        {
            this.udxFolderPath = selectedPath;
            this.gmlFileSearcher.GenerateFileDictionary(selectedPath);
            this.areaIdCheckboxes = Enumerable.Repeat(true, this.gmlFileSearcher.AreaIds.Length).ToArray();
        }
    }
}