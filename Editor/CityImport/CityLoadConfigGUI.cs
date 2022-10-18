﻿using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.IO;
using PLATEAU.Udx;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityLoadConfig"/> を設定するGUIです。
    /// </summary>
    internal static class CityLoadConfigGUI
    {
        /// <summary>
        /// <see cref="CityLoadConfig"/> を設定するGUIを描画します。
        /// </summary>
        public static void Draw(CityLoadConfig cityLoadConf)
        {
            foreach (var pair in cityLoadConf.ForEachPackagePair)
            {
                var package = pair.Key;
                var conf = pair.Value;
                conf.GuiFoldOutState = PlateauEditorStyle.FoldOut(conf.GuiFoldOutState, package.ToJapaneseName(), () =>
                {
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
                });
            }
        }

        private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
        {
            if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
            conf.includeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.includeTexture);
        }

        public static void LODRangeGUI(PackageLoadSetting conf, uint minLODLimit, uint maxLODLimit)
        {
            if (minLODLimit == maxLODLimit)
            {
                (conf.minLOD, conf.maxLOD) = (minLODLimit, maxLODLimit);
                return;
            }
            (float sliderValMin, float sliderValMax) = (conf.minLOD, conf.maxLOD);
            using (new EditorGUILayout.HorizontalScope())
            {
                // PlateauEditorStyle.LabelSizeFit(new GUIContent("LOD範囲設定"));
                EditorGUILayout.LabelField("LOD範囲設定", GUILayout.Width(150));
                PlateauEditorStyle.NumberDisplay((int)conf.minLOD);
                EditorGUILayout.MinMaxSlider("", ref sliderValMin, ref sliderValMax, minLODLimit, maxLODLimit);
                PlateauEditorStyle.NumberDisplay((int)conf.maxLOD);
            }
            
            conf.minLOD = (uint)Mathf.Round(sliderValMin);
            conf.maxLOD = (uint)Mathf.Round(sliderValMax);
        }
    }
}
