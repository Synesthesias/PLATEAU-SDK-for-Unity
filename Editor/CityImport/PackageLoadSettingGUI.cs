using System;
using System.Collections.Generic;
using PLATEAU.CityImport.Setting;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Texture;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


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

                // パッケージ種による場合分けです。
                // これと似たロジックが PackageLoadSetting.CreateSettingFor にあるので、変更時はそちらも合わせて変更をお願いします。
                var gui = package switch
                {
                    PredefinedCityModelPackage.Relief => new ReliefLoadSettingGUI((ReliefLoadSetting)packageConf, MeshCode.Parse(cityLoadConf.AreaMeshCodes[0])),
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

            public void Draw()
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
                            AdditionalSettingGUI();
                            var predefined = CityModelPackageInfo.GetPredefined(package);
                            TextureIncludeGUI(conf, predefined.hasAppearance);
                            conf.DoSetMeshCollider =
                                EditorGUILayout.Toggle("Mesh Collider をセットする", conf.DoSetMeshCollider);

                            int minLOD = conf.LODRange.MinLOD;
                            int maxLOD = conf.LODRange.MaxLOD;
                            int availableMaxLOD = conf.LODRange.AvailableMaxLOD;
                            PlateauEditorStyle.LODSlider("LOD描画設定", ref minLOD, ref maxLOD,
                                Math.Min(predefined.minLOD, availableMaxLOD), availableMaxLOD);
                            conf.LODRange = new LODRange(minLOD, maxLOD, availableMaxLOD);

                            conf.MeshGranularity = (MeshGranularity)EditorGUILayout.Popup("モデル結合",
                                (int)conf.MeshGranularity, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });

                            conf.DoSetAttrInfo =
                                EditorGUILayout.Toggle("属性情報を含める", conf.DoSetAttrInfo);

                            conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                                conf.FallbackMaterial, typeof(Material), false);
                        }
                    }
                });
            }

            /// <summary>
            /// パッケージによって追加のGUIがある場合、サブクラスでこのメソッドをオーバーライドして実装します。
            /// </summary>
            protected virtual void AdditionalSettingGUI()
            {
                // サブクラスで実装しない限り何もしません
            }

            private static void TextureIncludeGUI(PackageLoadSetting conf, bool mayTextureExist)
            {
                if (!mayTextureExist) return; // 仕様上、テクスチャの存在可能性がない場合
                conf.IncludeTexture = EditorGUILayout.Toggle("テクスチャを含める", conf.IncludeTexture);

                if (!conf.IncludeTexture) return;
                conf.EnableTexturePacking = EditorGUILayout.Toggle("テクスチャを結合する", conf.EnableTexturePacking);
                if (!conf.EnableTexturePacking) return;
                conf.TexturePackingResolution = (TexturePackingResolution)EditorGUILayout.Popup("テクスチャ解像度",
                    (int)conf.TexturePackingResolution, new[] { "2048x2048", "4096x4096", "8192x8192" });
            }
        }

        /// <summary>
        /// <see cref="ReliefLoadSetting"/> に対応するGUIクラスです。
        /// <see cref="PackageLoadSetting"/> に対して、土地特有の設定GUIを追加したクラスです。
        /// </summary>
        private class ReliefLoadSettingGUI : PackageLoadSettingGUI
        {
            private readonly ReliefLoadSetting config;
            private string mapTileURLOnGUI;
            private GeoCoordinate geoCoord;
            private MapZoomLevelSearchResult zoomLevelSearchResult = new MapZoomLevelSearchResult{AvailableZoomLevelMax = -1, AvailableZoomLevelMin = -1, IsValid = false};
            private bool zoomLevelSearchButtonPushed;

            public ReliefLoadSettingGUI(ReliefLoadSetting setting, MeshCode firstMeshCode) : base(setting)
            {
                this.config = setting;
                this.mapTileURLOnGUI = setting.MapTileURL;
                geoCoord = firstMeshCode.Extent.Center;

            }

            /// <summary> インポート設定GUIのうち土地専用の部分です。 </summary>
            protected override void AdditionalSettingGUI()
            {
                EditorGUILayout.LabelField("土地起伏の設定：");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    var conf = this.config;
                    conf.AttachMapTile = EditorGUILayout.Toggle("航空写真または地図を貼り付ける", conf.AttachMapTile);
                    if (conf.AttachMapTile)
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel2())
                        {
                            this.mapTileURLOnGUI = EditorGUILayout.TextField("URL", this.mapTileURLOnGUI);
                            try
                            {
                                conf.MapTileURL = this.mapTileURLOnGUI;
                            }
                            catch (ArgumentException)
                            {
                                EditorGUILayout.HelpBox("URLが正しくありません。", MessageType.Error);
                            }
                            if (this.mapTileURLOnGUI != ReliefLoadSetting.DefaultMapTileUrl)
                            {
                                PlateauEditorStyle.CenterAlignHorizontal(() =>
                                {
                                    if (PlateauEditorStyle.MiniButton("デフォルトURLに戻す", 150))
                                    {
                                        this.mapTileURLOnGUI = ReliefLoadSetting.DefaultMapTileUrl;
                                        conf.MapTileURL = ReliefLoadSetting.DefaultMapTileUrl;
                                        GUI.FocusControl("");
                                    }
                                });

                            }
                            int zoomLevel = conf.MapTileZoomLevel;
                            if (zoomLevelSearchButtonPushed)
                            {
                                if (zoomLevelSearchResult.IsValid)
                                {
                                    var zoomChoicesStr = new List<string>();
                                    var zoomChoicesInt = new List<int>();
                                    for (int i = zoomLevelSearchResult.AvailableZoomLevelMin;
                                         i <= zoomLevelSearchResult.AvailableZoomLevelMax;
                                         i++)
                                    {
                                        zoomChoicesStr.Add(i.ToString());
                                        zoomChoicesInt.Add(i);
                                    }

                                    zoomLevel = EditorGUILayout.IntPopup("ズームレベル", zoomLevel, zoomChoicesStr.ToArray(),
                                        zoomChoicesInt.ToArray());
                                }
                            }
                            else
                            {
                                zoomLevel = EditorGUILayout.IntField("ズームレベル", conf.MapTileZoomLevel);
                                zoomLevel = Math.Min(zoomLevel, ReliefLoadSetting.MaxZoomLevel);
                                zoomLevel = Math.Max(zoomLevel, ReliefLoadSetting.MinZoomLevel);
                            }
                            
                            conf.MapTileZoomLevel = zoomLevel;
                            PlateauEditorStyle.CenterAlignHorizontal(() =>
                            {
                                if (PlateauEditorStyle.MiniButton("利用可能ズームレベルを検索", 180))
                                {
#if UNITY_EDITOR
                                    EditorUtility.DisplayProgressBar("PLATEAU", "利用可能ズームレベルを検索中...", 0.1f);
#endif

                                    try
                                    {
                                        zoomLevelSearchResult = new MapZoomLevelSearcher().Search(mapTileURLOnGUI,
                                            geoCoord.Latitude, geoCoord.Longitude);
                                        zoomLevelSearchButtonPushed = true;
                                        conf.MapTileZoomLevel = zoomLevelSearchResult.AvailableZoomLevelMax;
                                    }
                                    catch (Exception)
                                    {
                                        Debug.LogError("Failed to load map zoom level.");
                                        zoomLevelSearchResult.IsValid = false;
                                    }
                                    

#if UNITY_EDITOR
                                    EditorUtility.ClearProgressBar();
#endif
                                }
                            });

                            if (zoomLevelSearchButtonPushed)
                            {
                                if (zoomLevelSearchResult.IsValid)
                                {
                                    EditorGUILayout.HelpBox(
                                        $"ズームレベルは {zoomLevelSearchResult.AvailableZoomLevelMin} から {zoomLevelSearchResult.AvailableZoomLevelMax} まで利用可能です",
                                        MessageType.Info);
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("URLに誤りがあります", MessageType.Error);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}