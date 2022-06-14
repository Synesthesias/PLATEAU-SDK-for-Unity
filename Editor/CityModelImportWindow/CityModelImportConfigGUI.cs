using System.Diagnostics;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEditor;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// udxフォルダを指定し、条件に合うgmlファイルを一括で変換するGUIを提供します。
    /// <see cref="CityModelImportWindow"/> の一部分で、この設定は <see cref="MultiGmlConverter"/> に渡されます。
    /// <see cref="CityMapMetaDataEditor"/> でも利用します。
    /// </summary>
    public class CityModelImportConfigGUI
    {
        private InputFolderSelectorGUI inputFolderSelectorGUI;
        private GmlSelectorGUI gmlSelectorGUI;
        private GmlFileSearcher gmlFileSearcher;
        private ExportFolderPathSelectorGUI exportFolderPathSelectorGUI;
        private MultiGmlConverter multiGmlConverter;
        public CityModelImportConfig Config { get; set; } = new CityModelImportConfig();

        public CityModelImportConfigGUI()
        {
            this.inputFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSelectorGUI = new GmlSelectorGUI();
            this.gmlFileSearcher = new GmlFileSearcher();
            this.exportFolderPathSelectorGUI = new ExportFolderPathSelectorGUI();
            this.multiGmlConverter = new MultiGmlConverter();
        }

        public CityModelImportConfig Draw()
        {
            this.inputFolderSelectorGUI.FolderPath = Config.sourceUdxFolderPath;
            string sourcePath = this.inputFolderSelectorGUI.Draw("udxフォルダ選択");
            Config.sourceUdxFolderPath = sourcePath;
            if (GmlFileSearcher.IsPathUdx(sourcePath))
            {
                // udxフォルダが選択されているなら、設定と出力のGUIを表示します。
                var gmlFiles = this.gmlSelectorGUI.Draw(this.gmlFileSearcher);
                Config.exportFolderPath = this.exportFolderPathSelectorGUI.Draw(Config.exportFolderPath);
                HeaderDrawer.Draw("変換設定");
                Config.optimizeFlag = EditorGUILayout.Toggle("最適化", Config.optimizeFlag);
                Config.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", Config.meshGranularity);
                Config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", this.Config.logLevel);
                HeaderDrawer.Draw("出力");
                if (PlateauEditorStyle.MainButton("出力"))
                {
                    this.multiGmlConverter.Convert(gmlFiles, Config.sourceUdxFolderPath, Config.exportFolderPath, Config);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
            
            return Config;
        }
        
        /// <summary>
        /// udxフォルダパス選択GUIで、新しいパスが指定されたときに呼ばれます。
        /// </summary>
        private void OnUdxPathChanged(string path)
        {
            if (!GmlFileSearcher.IsPathUdx(path)) return;
            this.gmlFileSearcher.GenerateFileDictionary(path);
            this.gmlSelectorGUI.OnUdxPathChanged(this.gmlFileSearcher);
        }
    }
}