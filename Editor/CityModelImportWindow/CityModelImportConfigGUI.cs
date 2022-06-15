using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMetaData;
using UnityEditor;

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
        public CityModelImportConfig Config { get; set; } = new CityModelImportConfig();

        public CityModelImportConfigGUI()
        {
            this.udxFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSelectorGUI = new GmlSelectorGUI();
            this.gmlFileSearcher = new GmlFileSearcher();
            this.exportFolderPathSelectorGUI = new ExportFolderPathSelectorGUI();
            this.multiGmlConverter = new MultiGmlConverter();
        }

        public CityModelImportConfig Draw()
        {
            this.udxFolderSelectorGUI.FolderPath = Config.sourceUdxFolderPath;
            string sourcePath = this.udxFolderSelectorGUI.Draw("udxフォルダ選択");
            Config.sourceUdxFolderPath = sourcePath;
            if (GmlFileSearcher.IsPathUdx(sourcePath))
            {
                // udxフォルダが選択されているなら、設定と出力のGUIを表示します。
                var gmlFiles = this.gmlSelectorGUI.Draw(this.gmlFileSearcher, ref Config.gmlSelectorConfig);
                Config.exportFolderPath = this.exportFolderPathSelectorGUI.Draw(Config.exportFolderPath);
                HeaderDrawer.Draw("変換設定");
                Config.optimizeFlag = EditorGUILayout.Toggle("最適化", Config.optimizeFlag);
                Config.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", Config.meshGranularity);
                Config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", Config.logLevel);
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
            this.gmlSelectorGUI.OnUdxPathChanged();
        }
    }
}