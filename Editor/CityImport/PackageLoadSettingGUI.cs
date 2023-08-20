using System;
using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// インポート設定GUIのうち、パッケージ種ごとの設定GUI <see cref="PackageLoadSettingGUI"/>を、利用可能パッケージ分だけ集めたものです。
    /// </summary>
    internal class PackageLoadSettingGUIList : IEditorDrawable
    {
        private readonly List<PackageLoadSettingGUI> packageGUIList;

        public PackageLoadSettingGUIList(PackageToLodDict availablePackageLODDict, CityLoadConfig cityLoadConf)
        {
            packageGUIList = new();
            foreach (var (package, maxLOD) in availablePackageLODDict)
            {
                if (maxLOD < 0)
                {
                    cityLoadConf.GetConfigForPackage(package).LoadPackage = false;
                    continue;
                }

                packageGUIList.Add(new PackageLoadSettingGUI(cityLoadConf.GetConfigForPackage(package)));
            }
        }

        public void Draw()
        {
            foreach (var gui in packageGUIList)
            {
                gui.Draw();
            }
        }


        /// <summary>
        /// インポート設定のうち、パッケージ種1つの設定GUIです。
        /// 具体的には、<see cref="PackageLoadSetting"/>を1つ設定するGUIです。
        /// </summary>
        private class PackageLoadSettingGUI
        {
            private readonly PackageLoadSetting conf;

            /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
            private bool guiFoldOutState = true;

            public PackageLoadSettingGUI(PackageLoadSetting conf)
            {
                this.conf = conf;
            }

            public void Draw()
            {
                var package = conf.Package;
                guiFoldOutState = PlateauEditorStyle.FoldOut(guiFoldOutState, package.ToJapaneseName(), () =>
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        conf.LoadPackage = EditorGUILayout.Toggle("インポートする", conf.LoadPackage);
                        if (conf.LoadPackage)
                        {
                            using (PlateauEditorStyle.VerticalScopeLevel1(1))
                            {
                                var predefined = CityModelPackageInfo.GetPredefined(package);
                                TextureIncludeGUI(conf, predefined.hasAppearance);
                                conf.DoSetMeshCollider =
                                    EditorGUILayout.Toggle("Mesh Collider をセットする", conf.DoSetMeshCollider);

                                conf.DoSetAttrInfo =
                                    EditorGUILayout.Toggle("属性情報を含める", conf.DoSetAttrInfo);

                                PlateauEditorStyle.LODSlider("LOD描画設定", ref conf.MinLOD, ref conf.MaxLOD,
                                    Math.Min(predefined.minLOD, conf.AvailableMaxLOD), conf.AvailableMaxLOD);

                                conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                                    (int)conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
                            }
                        }
                    }
                });
            }

            private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
            {
                if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
                conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.IncludeTexture);
            }
        }
    }
}