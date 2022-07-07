using System;
using System.Collections.Generic;
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

                // 地域IDごとに対象とするかを設定するGUIを表示します。
                foreach (var tuple in searcherConfig.IterateAreaTree())
                {
                    var node = tuple.node;
                    var area = node.Value;
                    int depth = tuple.depth;
                    int indent = depth - 1;
                    var parentArea = node.Parent.Value;
                    // ルートノードであるか、親の地域にチェックが入っている場合のみ表示します。
                    if (AreaTree.IsTopLevelArea(node) || parentArea.IsTarget)
                    {
                        EditorGUI.indentLevel += indent;
                        area.IsTarget = EditorGUILayout.Toggle(area.ToString(), area.IsTarget);
                        EditorGUI.indentLevel -= indent;
                        // チェックが外れているなら、子の地域もチェックを外します。
                        if (!area.IsTarget)
                        {
                            AreaTree.SetIsTargetRecursive(node, false);
                        }
                    }
                }
            }

            // 選択した地域に存在しない地物タイプは、GUI上で無効にしたいのでそのための辞書を構築します。
            var typeExistingDict = gmlSearcher.ExistingTypesForAreaIds(searcherConfig.GetTargetAreaIds());
            
            
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
                this.gmlFiles = ListTargetGmlFiles(gmlSearcher, searcherConfig.AreaTree,
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
            config.GenerateAreaTreeIfNull(gmlSearcher.AreaIds);
            
            this.isInitialized = true;
        }

        
        private static List<string> ListTargetGmlFiles(GmlSearcher gmlSearcher, AreaTree areaTree, GmlSearcherConfig searcherConfig)
        {
            var gmlFiles = new List<string>();

            var nodes = areaTree.IterateDfs();
            foreach (var tuple in nodes)
            {
                var area = tuple.node.Value;
                if (!area.IsTarget) continue;
                gmlFiles.AddRange(gmlSearcher.GetGmlFilePathsForAreaIdAndType(area.Id, searcherConfig, false));
            }

            return gmlFiles;
        }
    }
}