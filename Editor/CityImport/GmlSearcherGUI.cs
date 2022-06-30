using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// udxフォルダ内の gml ファイルのうち、どれを対象とするかを
    /// 条件によって絞り込む GUI を提供します。
    /// </summary>
    internal class GmlSearcherGUI
    {
        private List<string> gmlFiles;
        private bool isInitialized;
        private Vector2 scrollPosForGmlList;


        /// <summary>
        /// 変換対象とする gmlファイルを条件設定で絞り込むGUIを表示し、
        /// その結果を gmlファイルの相対パスのリストで返します。
        /// その際にユーザーが選択した設定内容は引数 <paramref name="searcherConfig"/> に格納されます。
        /// </summary>
        public List<string> Draw(GmlSearcher gmlSearcher, ref GmlSearcherConfig searcherConfig)
        {
            if(!this.isInitialized) Initialize(gmlSearcher, searcherConfig);
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                searcherConfig.areaIds = gmlSearcher.AreaIds;
                int areaCount = searcherConfig.areaIds.Length;
                if (searcherConfig.isAreaIdTarget.Length != areaCount)
                {
                    Initialize(gmlSearcher, searcherConfig);
                }
                for (int i = 0; i < areaCount; i++)
                {
                    searcherConfig.isAreaIdTarget[i] = EditorGUILayout.Toggle(searcherConfig.areaIds[i].ToString(), searcherConfig.isAreaIdTarget[i]);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("すべて選択"))
                    {
                        searcherConfig.SetAllAreaId(true);
                    }

                    if (PlateauEditorStyle.MiniButton("すべて除外"))
                    {
                        searcherConfig.SetAllAreaId(false);
                    }
                }
            }
            
            
            // 地物タイプごとの設定です。
            HeaderDrawer.Draw("含める地物");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var typeConfDict = searcherConfig.gmlTypeTarget.IsTypeTargetDict;
                foreach (var gmlType in typeConfDict.Keys.ToArray())
                {
                    string typeText = gmlType.ToDisplay();
                    var isTypeTarget = typeConfDict[gmlType];
                    isTypeTarget = EditorGUILayout.Toggle(typeText, isTypeTarget);
                    typeConfDict[gmlType] = isTypeTarget;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("すべて選択"))
                    {
                        searcherConfig.SetAllTypeTarget(true);
                    }

                    if (PlateauEditorStyle.MiniButton("すべて除外"))
                    {
                        searcherConfig.SetAllTypeTarget(false);
                    }
                }
            
            }

            HeaderDrawer.Draw("対象gmlファイル");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.gmlFiles = ListTargetGmlFiles(gmlSearcher, searcherConfig.areaIds, searcherConfig.isAreaIdTarget,
                    searcherConfig.gmlTypeTarget);
                this.scrollPosForGmlList = PlateauEditorStyle.ScrollableMultiLineLabel(String.Join("\n", this.gmlFiles), 150, this.scrollPosForGmlList);
            }
            
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