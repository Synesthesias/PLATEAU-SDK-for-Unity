using System;
using System.Diagnostics;
using System.Linq;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMeta;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// udxフォルダを指定し、条件に合うgmlファイルを一括で変換するGUIを提供します。
    /// <see cref="CityModelImportWindow"/> および <see cref="CityMapMetaDataEditor"/> によって利用されます。
    /// この設定は <see cref="MultiGmlConverter"/> に渡されます。
    /// </summary>
    public class CityModelImportConfigGUI
    {
        private readonly InputFolderSelectorGUI udxFolderSelectorGUI;
        private readonly GmlSelectorGUI gmlSelectorGUI;
        private readonly GmlFileSearcher gmlFileSearcher;
        private readonly ExportFolderPathSelectorGUI exportFolderPathSelectorGUI;
        private readonly MultiGmlConverter multiGmlConverter;
        
        /// <summary> <see cref="MultiGmlConverter"/> に渡す設定です。</summary>
        // public CityModelImportConfig Config { get; set; } = new CityModelImportConfig();

        public CityModelImportConfigGUI()
        {
            this.udxFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSelectorGUI = new GmlSelectorGUI();
            this.gmlFileSearcher = new GmlFileSearcher();
            this.exportFolderPathSelectorGUI = new ExportFolderPathSelectorGUI();
            this.multiGmlConverter = new MultiGmlConverter();
        }

        public void Draw(CityModelImportConfig config)
        {
            // udxフォルダ選択
            this.udxFolderSelectorGUI.FolderPath = config.sourceUdxFolderPath;
            string sourcePath = this.udxFolderSelectorGUI.Draw("udxフォルダ選択");
            config.sourceUdxFolderPath = sourcePath;
            
            // udxフォルダが選択されているなら、設定と出力のGUIを表示
            if (GmlFileSearcher.IsPathUdx(sourcePath))
            {
                // 変換対象の絞り込み
                var gmlFiles = this.gmlSelectorGUI.Draw(this.gmlFileSearcher, ref config.gmlSelectorConfig);
                
                // 変換先パス設定
                config.exportFolderPath = this.exportFolderPathSelectorGUI.Draw(config.exportFolderPath);
                
                // 変換設定
                HeaderDrawer.Draw("変換設定");
                config.optimizeFlag = EditorGUILayout.Toggle("最適化", config.optimizeFlag);
                config.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", config.meshGranularity);
                config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", config.logLevel);

                // 出力
                HeaderDrawer.Draw("出力");
                if (PlateauEditorStyle.MainButton("出力"))
                {
                    this.multiGmlConverter.Convert(gmlFiles, config);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
        }
        
        /// <summary>
        /// udxフォルダパス選択GUIで、新しいパスが指定されたときに呼ばれます。
        /// </summary>
        private void OnUdxPathChanged(string path)
        {
            if (!GmlFileSearcher.IsPathUdx(path)) return;
            this.gmlFileSearcher.GenerateFileDictionary(path);
            this.gmlSelectorGUI.OnUdxPathChanged();
        }
    }
}