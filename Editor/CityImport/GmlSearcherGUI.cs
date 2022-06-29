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
        /// その際にユーザーが選択した設定内容は引数 <paramref name="config"/> に格納されます。
        /// </summary>
        public List<string> Draw(GmlSearcher gmlSearcher, ref GmlSearcherConfig config)
        {
            if(!this.isInitialized) Initialize(gmlSearcher, config);
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("含める地域");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
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
                    if (PlateauEditorStyle.MiniButton("すべて選択"))
                    {
                        config.SetAllAreaId(true);
                    }

                    if (PlateauEditorStyle.MiniButton("すべて除外"))
                    {
                        config.SetAllAreaId(false);
                    }
                }
            }
            
            
            // 地物タイプごとの設定です。
            HeaderDrawer.Draw("含める地物");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var typeConfDict = config.gmlTypeTarget.GmlTypeConfigs;
                foreach (var gmlType in typeConfDict.Keys.ToArray())
                {
                    EditorGUILayout.LabelField(GmlTypeConvert.ToDisplay(gmlType));
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var typeConf = typeConfDict[gmlType];
                        typeConf.isTarget = EditorGUILayout.Toggle("変換対象", typeConf.isTarget);
                        // using (new EditorGUI.DisabledScope(!typeConf.isTarget))
                        // {
                        //     EditorGUILayout.MinMaxSlider("LOD", ref typeConf.SliderMinLod, ref typeConf.SliderMaxLod, 0f, 4f);
                        //     // Min <= Max となるようにスワップ
                        //     if (typeConf.SliderMinLod > typeConf.SliderMaxLod)
                        //         (typeConf.SliderMinLod, typeConf.SliderMaxLod) =
                        //             (typeConf.SliderMaxLod, typeConf.SliderMinLod);
                        //     typeConf.minLod = (int)Math.Round(typeConf.SliderMinLod);
                        //     typeConf.maxLod = (int)Math.Round(typeConf.SliderMaxLod);
                        //     EditorGUILayout.LabelField($"最小LOD: {typeConf.minLod}, 最大LOD: {typeConf.maxLod}");
                        // }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("すべて選択"))
                    {
                        config.gmlTypeTarget.SetAllTarget(true);
                    }

                    if (PlateauEditorStyle.MiniButton("すべて除外"))
                    {
                        config.gmlTypeTarget.SetAllTarget(false);
                    }
                }
            
            }

            HeaderDrawer.Draw("対象gmlファイル");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.gmlFiles = ListTargetGmlFiles(gmlSearcher, config.areaIds, config.isAreaIdTarget,
                    config.gmlTypeTarget);
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