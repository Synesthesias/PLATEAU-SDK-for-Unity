using System;
using System.Collections.Generic;
using System.Linq;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMetaData;
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
        private List<string> gmlFiles;
        private bool isInitialized;

        public GmlSelectorConfig Config { get; set; } = new GmlSelectorConfig();

        /// <summary>
        /// 変換対象とする gmlファイルを選択するGUIを表示し、
        /// その結果を gmlファイルの相対パスのリストで返します。
        /// </summary>
        public List<string> Draw(GmlFileSearcher gmlFileSearcher, out GmlSelectorConfig outGmlSelectorConfig)
        {
            if(!this.isInitialized) Initialize(gmlFileSearcher);
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            Config.areaIds = gmlFileSearcher.AreaIds;
            int areaCount = Config.areaIds.Length;
            if (Config.isAreaIdTarget.Length != areaCount)
            {
                Initialize(gmlFileSearcher);
            }
            for (int i = 0; i < areaCount; i++)
            {
                Config.isAreaIdTarget[i] = EditorGUILayout.Toggle(Config.areaIds[i], Config.isAreaIdTarget[i]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    Config.SetAllAreaId(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    Config.SetAllAreaId(false);
                }
            }
            
            HeaderDrawer.Draw("含める地物");
            var typeDict = Config.gmlTypeTarget.TargetDict;
            foreach (var gmlType in typeDict.Keys.ToArray())
            {
                typeDict[gmlType] = EditorGUILayout.Toggle(GmlTypeConvert.ToDisplay(gmlType), typeDict[gmlType]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    Config.gmlTypeTarget.SetAll(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    Config.gmlTypeTarget.SetAll(false);
                }
            }

            HeaderDrawer.Draw("対象gmlファイル");
            this.gmlFiles = ListTargetGmlFiles(gmlFileSearcher, this.Config.areaIds, this.Config.isAreaIdTarget, this.Config.gmlTypeTarget);
            EditorGUILayout.TextArea(String.Join("\n", this.gmlFiles));
            
            HeaderDrawer.DecrementDepth();
            outGmlSelectorConfig = Config;
            
            return this.gmlFiles;
        }

        public void OnUdxPathChanged(GmlFileSearcher gmlFileSearcher)
        {
            Initialize(gmlFileSearcher);
        }

        public void Initialize(GmlFileSearcher gmlFileSearcher)
        {
            int areaCount = gmlFileSearcher.AreaIds.Length;
            Config.isAreaIdTarget = Enumerable.Repeat(true, areaCount ).ToArray();
            this.isInitialized = true;
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