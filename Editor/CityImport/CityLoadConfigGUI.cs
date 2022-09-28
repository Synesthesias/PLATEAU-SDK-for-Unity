using System;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.IO;
using PLATEAU.Udx;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
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
                            LODRangeGUI(conf, (uint)predefined.minLOD, (uint)predefined.maxLOD);
                            conf.meshGranularity = (MeshGranularity)EditorGUILayout.Popup("メッシュ結合単位",
                                (int)conf.meshGranularity, new[] { "最小地物単位", "主要地物単位", "都市モデル地域単位" });
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

        private static void LODRangeGUI(PackageLoadSetting conf, uint minLODLimit, uint maxLODLimit)
        {
            if (minLODLimit == maxLODLimit)
            {
                (conf.minLOD, conf.maxLOD) = (minLODLimit, maxLODLimit);
                return;
            }
            (float sliderValMin, float sliderValMax) = (conf.minLOD, conf.maxLOD);
            EditorGUILayout.MinMaxSlider("LOD範囲", ref sliderValMin, ref sliderValMax, minLODLimit, maxLODLimit);
            conf.minLOD = (uint)Mathf.Round(sliderValMin);
            conf.maxLOD = (uint)Mathf.Round(sliderValMax);
            EditorGUILayout.LabelField($"選択LOD: {conf.minLOD} 以上 {conf.maxLOD} 以下");
        }
    }
}
