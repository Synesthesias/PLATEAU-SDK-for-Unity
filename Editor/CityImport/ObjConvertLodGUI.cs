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
                        var typeLodRange = lodConf.TypeLodDict[gmlType];
                        var sliderMinMax = lodConf.TypeLodSliderDict[gmlType];
                        (float sliderMin, float sliderMax) = (sliderMinMax.Min, sliderMinMax.Max);
                        EditorGUILayout.MinMaxSlider(ref sliderMin, ref sliderMax, 0f, 3f);
                        typeLodRange.SetMinMax((int)Math.Round(sliderMin), (int)Math.Round(sliderMax));
                        sliderMinMax.SetMinMax(sliderMin, sliderMax);
                        EditorGUILayout.LabelField($"最小LOD: {typeLodRange.Min}, 最大LOD: {typeLodRange.Max}");
                    }
                    
                }
            }
        }
    }
}