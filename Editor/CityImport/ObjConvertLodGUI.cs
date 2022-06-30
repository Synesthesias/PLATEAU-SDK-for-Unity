using System;
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
    internal class ObjConvertLodGUI
    {
        /// <summary>
        /// obj変換のLOD設定のGUI描画し、ユーザーのGUI操作に応じて引数である <see cref="ObjConvertLodConfig"/> の中身を書き換えます。
        /// </summary>
        public void Draw(ObjConvertLodConfig lodConf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (PlateauEditorStyle.MiniButton("　全選択　"))
                    {
                        lodConf.SetToAllRange();
                    }

                    if (PlateauEditorStyle.MiniButton("最小値のみ"))
                    {
                        lodConf.SetToOnlyMin();
                    }

                    if (PlateauEditorStyle.MiniButton("最大値のみ"))
                    {
                        lodConf.SetToOnlyMax();
                    }
                }

                var gmlTypes = lodConf.TypeLodDict.Keys;

                // 地物タイプごとのループ
                foreach (var gmlType in gmlTypes)
                {
                    string typeText = gmlType.ToDisplay();
                    EditorGUILayout.LabelField(typeText);
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        lodConf.ClampLodRangeToPossibleVal();
                        var typeLodRange = lodConf.TypeLodDict[gmlType];
                        var sliderMinMax = lodConf.TypeLodSliderDict[gmlType];
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