using System;
using System.Xml;
using PLATEAU.CityMeta;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// gmlファイルから objファイルへの変換に関して、地物タイプごとに対象のLOD範囲を設定するGUIです。
    /// <see cref="CityImportGUI"/> によって利用されます。
    /// </summary>
    internal class ObjConvertTypesGUI
    {
        /// <summary>
        /// obj変換のLOD設定のGUI描画し、ユーザーのGUI操作に応じて引数である <see cref="ObjConvertTypesConfig"/> の中身を書き換えます。
        /// </summary>
        public void Draw(ObjConvertTypesConfig typesConf, GmlSearcherConfig gmlSearcherConfig)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("LOD一括選択");
                using (new GUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("　全選択　"))
                    {
                        typesConf.SetLodRangeToAllRange();
                    }

                    if (PlateauEditorStyle.MiniButton("最小値のみ"))
                    {
                        typesConf.SetLodRangeToOnlyMin();
                    }

                    if (PlateauEditorStyle.MiniButton("最大値のみ"))
                    {
                        typesConf.SetLodRangeToOnlyMax();
                    }
                }

                var gmlTypes = typesConf.TypeLodDict.Keys;

                // 地物タイプごとのループ
                foreach (var gmlType in gmlTypes)
                {
                    bool isTypeTarget = gmlSearcherConfig.gmlTypeTarget.IsTypeTarget(gmlType);
                    if (!isTypeTarget) continue;
                    
                    string typeText = gmlType.ToDisplay();
                    EditorGUILayout.LabelField(typeText);
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        // LOD対象選択 複数 / 最大のみ
                        var typeExportLower = typesConf.TypeExportLowerLodDict[gmlType];
                        typeExportLower = Convert.ToBoolean(EditorGUILayout.Popup("出力モード",
                            Convert.ToInt32(typeExportLower), new string[] { "選択中最大LODのみ", "全LOD" }));
                        typesConf.TypeExportLowerLodDict[gmlType] = typeExportLower;
                        
                        // LOD範囲選択
                        typesConf.ClampLodRangeToPossibleVal();
                        var typeLodRange = typesConf.TypeLodDict[gmlType];
                        var sliderMinMax = typesConf.TypeLodSliderDict[gmlType];
                        (float valueMin, float valueMax) = (sliderMinMax.Min, sliderMinMax.Max);
                        var availableRange = gmlType.PossibleLodRange();
                        EditorGUILayout.MinMaxSlider(ref valueMin, ref valueMax, availableRange.Min, availableRange.Max);
                        typeLodRange.SetMinMax((int)Math.Round(valueMin), (int)Math.Round(valueMax));
                        sliderMinMax.SetMinMax(valueMin, valueMax);
                        EditorGUILayout.LabelField($"最小LOD: {typeLodRange.Min}, 最大LOD: {typeLodRange.Max}");
                    }
                    
                }
            }
        }
    }
}