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
                // 一括選択ボタンを表示します。
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
                
                searcherConfig.areaIds = gmlSearcher.AreaIds;
                int areaCount = searcherConfig.areaIds.Length;
                if (searcherConfig.isAreaIdTarget.Length != areaCount)
                {
                    Initialize(gmlSearcher, searcherConfig);
                }
                // 地域IDごとに対象とするかを設定するGUIを表示します。
                for (int i = 0; i < areaCount; i++)
                {
                    searcherConfig.isAreaIdTarget[i] = EditorGUILayout.Toggle(searcherConfig.areaIds[i].ToString(), searcherConfig.isAreaIdTarget[i]);
                }
            }

            // 選択した地域に存在しない地物タイプは、GUI上で無効にしたいのでそのための辞書を構築します。
            var typeExistingDict = gmlSearcher.ExistingTypesForAreaIds(searcherConfig.TargetAreaIds());
            
            
            // 地物タイプごとの設定です。
            HeaderDrawer.Draw("含める地物");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
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
                
                // 地物タイプごとのチェックマークです。
                foreach (var gmlType in searcherConfig.AllGmlTypes())
                {
                    bool isTypeExist = typeExistingDict[gmlType];
                    using (new EditorGUI.DisabledScope(!isTypeExist))
                    {
                        string typeText = gmlType.ToDisplay();
                        var isTypeTarget = searcherConfig.GetIsTypeTarget(gmlType);
                        isTypeTarget = EditorGUILayout.Toggle(typeText, isTypeTarget && isTypeExist);
                        searcherConfig.SetIsTypeTarget(gmlType, isTypeTarget);
                    }
                }

            }

            HeaderDrawer.Draw("対象gmlファイル");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.gmlFiles = ListTargetGmlFiles(gmlSearcher, searcherConfig.areaIds, searcherConfig.isAreaIdTarget,
                    searcherConfig);
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

        
        private static List<string> ListTargetGmlFiles(GmlSearcher gmlSearcher, int[] areaIds, bool[] areaCheckboxes, GmlSearcherConfig searcherConfig)
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
                gmlFiles.AddRange(gmlSearcher.GetGmlFilePathsForAreaIdAndType(areaIds[i], searcherConfig, false));
            }

            return gmlFiles;
        }
    }
}