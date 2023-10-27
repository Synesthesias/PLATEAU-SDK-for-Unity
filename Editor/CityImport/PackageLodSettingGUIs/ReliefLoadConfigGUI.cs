using System;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Editor.CityImport.GUIParts;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace PLATEAU.Editor.CityImport.PackageLodSettingGUIs
{
    /// <summary>
    /// <see cref="ReliefLoadConfig"/> に対応するGUIクラスです。
    /// <see cref="PackageLoadConfig"/> を継承し、土地特有の設定GUIを追加したクラスです。
    /// </summary>
    internal class ReliefLoadConfigGUI : PackageLoadConfigGUI
    {
        private readonly ReliefLoadConfig config;
        private readonly MapZoomLevelSelectGUI zoomLevelSelectGUI;
        private string mapTileURLOnGUI;


        public ReliefLoadConfigGUI(ReliefLoadConfig config, MeshCode firstMeshCode) : base(config)
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

                        if (this.mapTileURLOnGUI != ReliefLoadConfig.DefaultMapTileUrl)
                        {
                            PlateauEditorStyle.CenterAlignHorizontal(() =>
                            {
                                if (PlateauEditorStyle.MiniButton("デフォルトURLに戻す", 150))
                                {
                                    string defaultURL = ReliefLoadConfig.DefaultMapTileUrl;
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