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
            HeaderDrawer.IncrementDepth();
            foreach (var pair in cityLoadConf.ForEachPackagePair)
            {
                var package = pair.Key;
                var conf = pair.Value;
                HeaderDrawer.Draw(Enum.GetName(typeof(PredefinedCityModelPackage), package));
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    conf.loadPackage = EditorGUILayout.Toggle("インポートする", conf.loadPackage);
                    if (conf.loadPackage)
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel2())
                        {
                            var predefined = CityModelPackageInfo.GetPredefined(package);
                            TextureIncludeGUI(conf, predefined.hasAppearance);
                            LODRangeGUI(conf, predefined.minLOD, predefined.maxLOD);
                            conf.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュ結合単位", conf.meshGranularity);
                        }
                    }
                }
                
            }
            HeaderDrawer.DecrementDepth();
        }

        private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
        {
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            conf.includeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.includeTexture);
        }

        private static void LODRangeGUI(PackageLoadSetting conf, int minLODLimit, int maxLODLimit)
        {
            if (minLODLimit == maxLODLimit)
            {
                (conf.minLOD, conf.maxLOD) = (minLODLimit, maxLODLimit);
                return;
            }
            (float sliderValMin, float sliderValMax) = (conf.minLOD, conf.maxLOD);
            EditorGUILayout.MinMaxSlider("LOD範囲", ref sliderValMin, ref sliderValMax, minLODLimit, maxLODLimit);
            conf.minLOD = (int)Mathf.Round(sliderValMin);
            conf.maxLOD = (int)Mathf.Round(sliderValMax);
            EditorGUILayout.LabelField($"選択LOD: {conf.minLOD} 以上 {conf.maxLOD} 以下");
        }
    }
}
