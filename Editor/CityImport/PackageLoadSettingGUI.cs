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
            this.packageGUIList = new();
            foreach (var (package, maxLOD) in availablePackageLODDict)
            {
                if (maxLOD < 0)
                {
                    cityLoadConf.GetConfigForPackage(package).LoadPackage = false;
                    continue;
                }

                var packageConf = cityLoadConf.GetConfigForPackage(package);
                var gui = package switch
                {
                    PredefinedCityModelPackage.Relief => new ReliefLoadSettingGUI((ReliefLoadSetting)packageConf),
                    _ => new PackageLoadSettingGUI(packageConf)
                };
                this.packageGUIList.Add(gui);
            }
        }

        public void Draw()
        {
            foreach (var gui in this.packageGUIList)
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
            private readonly PackageLoadSetting config;

            /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
            private bool guiFoldOutState = true;

            public PackageLoadSettingGUI(PackageLoadSetting conf)
            {
                this.config = conf;
            }

            public virtual void Draw()
            {
                var conf = this.config;
                var package = conf.Package;
                this.guiFoldOutState = PlateauEditorStyle.FoldOut(this.guiFoldOutState, package.ToJapaneseName(), () =>
                {
                    using (PlateauEditorStyle.VerticalScopeLevel1())
                    {
                        conf.LoadPackage = EditorGUILayout.Toggle("インポートする", conf.LoadPackage);
                        if (!conf.LoadPackage) return;
                        using (PlateauEditorStyle.VerticalScopeLevel1(1))
                        {
                            var predefined = CityModelPackageInfo.GetPredefined(package);
                            TextureIncludeGUI(conf, predefined.hasAppearance);
                            conf.DoSetMeshCollider =
                                EditorGUILayout.Toggle("Mesh Collider をセットする", conf.DoSetMeshCollider);

                            conf.DoSetAttrInfo =
                                EditorGUILayout.Toggle("属性情報を含める", conf.DoSetAttrInfo);

                            int minLOD = conf.LODRange.MinLOD;
                            int maxLOD = conf.LODRange.MaxLOD;
                            int availableMaxLOD = conf.LODRange.AvailableMaxLOD;
                            PlateauEditorStyle.LODSlider("LOD描画設定", ref minLOD, ref maxLOD,
                                Math.Min(predefined.minLOD, availableMaxLOD), availableMaxLOD);
                            conf.LODRange = new LODRange(minLOD, maxLOD, availableMaxLOD);

                            conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                                (int)conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
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

        /// <summary>
        /// <see cref="ReliefLoadSetting"/> に対応するGUIクラスです。
        /// <see cref="PackageLoadSetting"/> に対して、土地特有の設定GUIを追加したクラスです。
        /// </summary>
        private class ReliefLoadSettingGUI : PackageLoadSettingGUI
        {
            private readonly ReliefLoadSetting conf;
            public ReliefLoadSettingGUI(ReliefLoadSetting setting) : base(setting)
            {
                this.conf = setting;
            }

            public override void Draw()
            {
                this.conf.AttachMapTile = EditorGUILayout.Toggle("航空写真または地図を貼り付ける", this.conf.AttachMapTile);
                base.Draw();

            }
        }
    }
    
}