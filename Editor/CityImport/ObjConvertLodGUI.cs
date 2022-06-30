using System;
using PLATEAU.CityMeta;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    internal class ObjConvertLodGUI
    {
        public void Draw(ObjConvertLodConfig lodConf)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var gmlTypes = lodConf.TypeLodDict.Keys;
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