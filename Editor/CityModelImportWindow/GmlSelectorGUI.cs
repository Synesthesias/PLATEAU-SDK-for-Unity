using System;
using System.Collections.Generic;
using System.Linq;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    public class GmlSelectorGUI
    {
        private bool[] areaIdCheckboxes;
        private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();
        private List<string> gmlFiles;
        private GmlFileSearcher gmlFileSearcher;

        public IEnumerable<string> GmlFiles => this.gmlFiles;

        public GmlSelectorGUI(GmlFileSearcher gmlFileSearcher)
        {
            this.gmlFileSearcher = gmlFileSearcher;
        }
        
        /// <summary>
        /// 変換対象とする gmlファイルを選択するGUIを表示し、
        /// その結果を gmlファイルの相対パスのリストで返します。
        /// </summary>
        public List<string> Draw()
        {
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            var areaIds = this.gmlFileSearcher.AreaIds;
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
            return this.gmlFiles;
        }

        public void OnUdxPathChanged()
        {
            this.areaIdCheckboxes = Enumerable.Repeat(true, this.gmlFileSearcher.AreaIds.Length).ToArray();
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