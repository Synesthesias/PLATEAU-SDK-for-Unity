using System;
using System.Collections.Generic;
using System.Linq;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMeta;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityImport
{

    /// <summary>
    /// udxフォルダ内の gml ファイルのうち、どれを対象とするかを
    /// 条件によって絞り込む GUI を提供します。
    /// </summary>
    internal class GmlSearcherGUI
    {
        private List<string> gmlFiles;
        private bool isInitialized;


        /// <summary>
        /// 変換対象とする gmlファイルを条件設定で絞り込むGUIを表示し、
        /// その結果を gmlファイルの相対パスのリストで返します。
        /// その際にユーザーが選択した設定内容は引数 <paramref name="config"/> に格納されます。
        /// </summary>
        public List<string> Draw(GmlSearcher gmlSearcher, ref GmlSearcherConfig config)
        {
            if(!this.isInitialized) Initialize(gmlSearcher, config);
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            config.areaIds = gmlSearcher.AreaIds;
            int areaCount = config.areaIds.Length;
            if (config.isAreaIdTarget.Length != areaCount)
            {
                Initialize(gmlSearcher, config);
            }
            for (int i = 0; i < areaCount; i++)
            {
                config.isAreaIdTarget[i] = EditorGUILayout.Toggle(config.areaIds[i].ToString(), config.isAreaIdTarget[i]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    config.SetAllAreaId(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    config.SetAllAreaId(false);
                }
            }
            
            HeaderDrawer.Draw("含める地物");
            var typeDict = config.gmlTypeTarget.TargetDict;
            foreach (var gmlType in typeDict.Keys.ToArray())
            {
                typeDict[gmlType] = EditorGUILayout.Toggle(GmlTypeConvert.ToDisplay(gmlType), typeDict[gmlType]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (PlateauEditorStyle.MainButton("すべて選択"))
                {
                    config.gmlTypeTarget.SetAll(true);
                }

                if (PlateauEditorStyle.MainButton("すべて除外"))
                {
                    config.gmlTypeTarget.SetAll(false);
                }
            }

            HeaderDrawer.Draw("対象gmlファイル");
            this.gmlFiles = ListTargetGmlFiles(gmlSearcher, config.areaIds, config.isAreaIdTarget, config.gmlTypeTarget);
            EditorGUILayout.TextArea(String.Join("\n", this.gmlFiles));
            
            HeaderDrawer.DecrementDepth();

            return this.gmlFiles;
        }

        public void OnUdxPathChanged()
        {
            this.isInitialized = false;
        }

        private void Initialize(GmlSearcher gmlSearcher, GmlSearcherConfig config)
        {
            int areaCount = gmlSearcher.AreaIds.Length;
            if (config.isAreaIdTarget.Length != areaCount)
            {
                config.isAreaIdTarget = Enumerable.Repeat(true, areaCount ).ToArray();
            }
            this.isInitialized = true;
        }

        
        private static List<string> ListTargetGmlFiles(GmlSearcher gmlSearcher, int[] areaIds, bool[] areaCheckboxes, GmlTypeTarget gmlTypeTarget)
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