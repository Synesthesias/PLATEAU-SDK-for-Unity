using System;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.IO;
using PLATEAU.Udx;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityLoader
{
    internal static class CityLoadConfigGUI
    {
        public static void Draw(CityLoadConfig cityLoadConf)
        {
            HeaderDrawer.Draw("パッケージ別設定");
            foreach (var pair in cityLoadConf.ForEachPackage)
            {
                var package = pair.Key;
                var conf = pair.Value;
                EditorGUILayout.LabelField(Enum.GetName(typeof(PredefinedCityModelPackage), package));
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    var predefined = CityModelPackageInfo.GetPredefined(package);
                    conf.includeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.includeTexture);
                    (float sliderValMin, float sliderValMax) = (conf.minLOD, conf.maxLOD);
                    EditorGUILayout.MinMaxSlider("LOD範囲", ref sliderValMin, ref sliderValMax, predefined.minLOD, predefined.maxLOD);
                    conf.minLOD = (int)Mathf.Round(sliderValMin);
                    conf.maxLOD = (int)Mathf.Round(sliderValMax);
                    conf.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュ結合単位", conf.meshGranularity);
                }
            }
        }
    }
}
