using System;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs
{
    /// <summary>
    /// <see cref="ReliefImportConfig"/> に対応するGUIクラスです。
    /// <see cref="PackageImportConfig"/> を継承し、土地特有の設定GUIを追加したクラスです。
    /// </summary>
    internal class ReliefImportConfigGUI : PackageImportConfigGUI
    {
        private readonly ReliefImportConfig config;
        private readonly MapZoomLevelSelectGUI zoomLevelSelectGUI;
        private string mapTileURLOnGUI;


        public ReliefImportConfigGUI(ReliefImportConfig config, PackageImportConfigExtendable masterConf,
            MeshCode firstMeshCode) : base(config, masterConf)
        {
            this.config = config;
            this.mapTileURLOnGUI = config.MapTileURL;
            zoomLevelSelectGUI = new MapZoomLevelSelectGUI(config, mapTileURLOnGUI, firstMeshCode);
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
                        string inputtedUrl = EditorGUILayout.TextField("URL", this.mapTileURLOnGUI);
                        if (inputtedUrl != mapTileURLOnGUI)
                        {
                            zoomLevelSelectGUI.OnMapTileUrlChanged(inputtedUrl);
                        }

                        mapTileURLOnGUI = inputtedUrl;
                        try
                        {
                            conf.MapTileURL = this.mapTileURLOnGUI;
                        }
                        catch (ArgumentException)
                        {
                            EditorGUILayout.HelpBox("URLが正しくありません。", MessageType.Error);
                        }

                        if (this.mapTileURLOnGUI != ReliefImportConfig.DefaultMapTileUrl)
                        {
                            PlateauEditorStyle.CenterAlignHorizontal(() =>
                            {
                                if (PlateauEditorStyle.MiniButton("デフォルトURLに戻す", 150))
                                {
                                    string defaultURL = ReliefImportConfig.DefaultMapTileUrl;
                                    this.mapTileURLOnGUI = defaultURL;
                                    conf.MapTileURL = defaultURL;
                                    zoomLevelSelectGUI.OnMapTileUrlChanged(defaultURL);
                                    GUI.FocusControl("");
                                }
                            });
                        }

                        zoomLevelSelectGUI.Draw();
                    }
                }
            }
        }
    }
}