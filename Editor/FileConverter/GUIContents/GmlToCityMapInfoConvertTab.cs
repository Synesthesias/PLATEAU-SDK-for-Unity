using System;
using System.IO;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using PlateauUnitySDK.Runtime.SemanticsLoader;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.GUIContents
{
    /// <summary>
    /// Gmlファイルを読んで <see cref="CityMapInfo"/> を出力するGUIを提供します。
    /// </summary>
    public class GmlToCityMapInfoConvertTab : BaseConvertTab
    {
        private readonly GmlToCityMapInfoConverter converter = new GmlToCityMapInfoConverter();

        private bool doOptimize = true;

        private bool doTessellate;
        protected override string SourceFileExtension => "gml";
        public override string DestFileExtension => "asset";
        public override IFileConverter FileConverter => this.converter;
        private static string projectPath = Path.GetDirectoryName(Application.dataPath);
        private int dstTabIndex = 0;
        private CityMapInfo existingMapInfo;
        private string existingMapInfoPath;

        public override void HeaderInfoGUI()
        {
            
        }

        public override void DstFileSelectGUI()
        {
            // 対象ファイルの選択方法は、新ファイルか既存ファイルかの2パターンあります。
            // 前者は基底クラスと同じ処理で、後者をオーバーライドして実装しています。
            
            // 選択タブ
            HeaderDrawer.Draw("Select ID->File Table Path");
            HeaderDrawer.IncrementDepth();
            HeaderDrawer.Draw("Select Target File Type");
            EditorGUILayout.LabelField("Write ID->FileName Table to:");
            this.dstTabIndex =
                PlateauEditorStyle.Tabs(
                    this.dstTabIndex,
                    "New File", "Existing File"
                );
            // タブにより対象ファイル選択
            switch (this.dstTabIndex)
            {
                case 0:
                    // New File のとき、基底クラスの保存先選択GUIを表示します。
                    base.DstFileSelectGUI();
                    break;
                case 1:
                    HeaderDrawer.Draw("Select Existing File");
                    // Existing File のとき、既存のファイル選択GUIを表示します。
                    this.existingMapInfo = (CityMapInfo)EditorGUILayout.ObjectField(
                        $"{nameof(CityMapInfo)}:",
                        this.existingMapInfo,
                        typeof(IdToGmlTable),
                        false
                        );
                    this.existingMapInfoPath = 
                        Path.Combine(projectPath, AssetDatabase.GetAssetPath(this.existingMapInfo));
                    GUILayout.TextArea(this.existingMapInfoPath);
                    break;
                default:
                    throw new Exception("Unknown Tab Index.");
            }
            HeaderDrawer.DecrementDepth();
        }

        public override void ConfigureGUI()
        {
            this.doOptimize = EditorGUILayout.Toggle("Optimize", this.doOptimize);
            this.doTessellate = EditorGUILayout.Toggle("Tessellate", this.doTessellate);
        }

        public override void OnConfigureGUIChanged()
        {
            this.converter.SetConfig(this.doOptimize, this.doTessellate);
        }

        protected override bool Convert()
        {
            // 変換方法のタブによって Convert に渡すパスが違うので、
            // オーバーライドして場合分けでパスを指定します。
            // TODO 数値指定は分かりにくいのでenum形式にする
            switch (this.dstTabIndex)
            {
                case 0:
                    // New File の挙動は基底クラスと同じです。
                    return base.Convert();
                case 1:
                    // Existing File のときは対象パスが変わります。
                    return FileConverter.Convert(this.SourceFilePath, this.existingMapInfoPath);
                default:
                    throw new Exception("Unknown Tab Index.");
            }
        }
    }
}