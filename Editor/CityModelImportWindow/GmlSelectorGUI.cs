using System;
using System.Collections.Generic;
using System.Linq;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// フォルダ内の gml ファイルのうち、どれを対象とするかを
    /// 条件によって絞り込む GUI を提供します。
    /// </summary>
    public class GmlSelectorGUI
    {
        // private bool[] areaIdCheckboxes;
        // private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();
        private GmlSelectorConfig config;
        private List<string> gmlFiles;


        /// <summary>
        /// 変換対象とする gmlファイルを選択するGUIを表示し、
        /// その結果を gmlファイルの相対パスのリストで返します。
        /// </summary>
        public List<string> Draw(GmlFileSearcher gmlFileSearcher)
        {
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            this.config.areaIds = gmlFileSearcher.AreaIds;
            int areaCount = this.config.areaIds.Length;
            this.config.isAreaIdTarget = new bool[areaCount];
            for (int i = 0; i < areaCount; i++)
            {
                this.config.isAreaIdTarget[i] = EditorGUILayout.Toggle(this.config.areaIds[i], this.config.isAreaIdTarget[i]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    this.config.SetAllAreaId(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    this.config.SetAllAreaId(false);
                }
            }
            
            HeaderDrawer.Draw("含める地物");
            var typeDict = this.config.gmlTypeTarget.TargetDict;
            foreach (var gmlType in typeDict.Keys.ToArray())
            {
                typeDict[gmlType] = EditorGUILayout.Toggle(GmlTypeConvert.ToDisplay(gmlType), typeDict[gmlType]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    this.config.gmlTypeTarget.SetAll(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    this.config.gmlTypeTarget.SetAll(false);
                }
            }

            HeaderDrawer.Draw("対象gmlファイル");
            this.gmlFiles = ListTargetGmlFiles(gmlFileSearcher, this.config.areaIds, this.config.isAreaIdTarget, this.config.gmlTypeTarget);
            EditorGUILayout.TextArea(String.Join("\n", this.gmlFiles));
            HeaderDrawer.DecrementDepth();
            return this.gmlFiles;
        }

        public void OnUdxPathChanged(GmlFileSearcher gmlFileSearcher)
        {
            this.config.isAreaIdTarget = Enumerable.Repeat(true, gmlFileSearcher.AreaIds.Length).ToArray();
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
    }
}