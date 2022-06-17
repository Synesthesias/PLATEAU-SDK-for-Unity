using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMeta;
using PlateauUnitySDK.Runtime.Util;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityImport
{

    /// <summary>
    /// udxフォルダを指定し、条件に合うgmlファイルを一括で変換するGUIを提供します。
    /// <see cref="CityImportWindow"/> および <see cref="CityMetaDataEditor"/> によって利用されます。
    /// この設定は <see cref="CityImporter"/> に渡されます。
    /// </summary>
    internal class CityImportGUI
    {
        private readonly InputFolderSelectorGUI udxFolderSelectorGUI;
        private readonly GmlSearcherGUI gmlSearcherGUI;
        private readonly GmlSearcher gmlSearcher;
        private readonly ExportFolderSelectorGUI exportFolderSelectorGUI;
        private readonly CityImporter cityImporter;
        
        public CityImportGUI()
        {
            this.udxFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSearcherGUI = new GmlSearcherGUI();
            this.gmlSearcher = new GmlSearcher();
            this.exportFolderSelectorGUI = new ExportFolderSelectorGUI();
            this.cityImporter = new CityImporter();
        }

        public void Draw(CityImporterConfig config)
        {
            // udxフォルダ選択
            this.udxFolderSelectorGUI.FolderPath = config.sourceUdxFolderPath;
            string sourcePath = this.udxFolderSelectorGUI.Draw("udxフォルダ選択");
            config.sourceUdxFolderPath = sourcePath;

            // udxフォルダが選択されているなら、設定と出力のGUIを表示
            if (GmlSearcher.IsPathUdx(sourcePath))
            {
                // 案内
                if (!CityImporter.IsInStreamingAssets(sourcePath))
                {
                    EditorGUILayout.HelpBox($"入力フォルダは {PathUtil.FullPathToAssetsPath(PlateauPath.StreamingGmlFolder)} にコピーされます。", MessageType.Info);
                }
                
                // 変換対象の絞り込み
                var gmlFiles = this.gmlSearcherGUI.Draw(this.gmlSearcher, ref config.gmlSearcherConfig);
                
                // 変換先パス設定
                config.exportFolderPath = this.exportFolderSelectorGUI.Draw(config.exportFolderPath);
                
                // 変換設定
                HeaderDrawer.Draw("変換設定");
                config.optimizeFlag = EditorGUILayout.Toggle("最適化", config.optimizeFlag);
                config.meshGranularity = (MeshGranularity)EditorGUILayout.EnumPopup("メッシュのオブジェクト分けの粒度", config.meshGranularity);
                config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", config.logLevel);

                // 出力
                HeaderDrawer.Draw("出力");
                if (PlateauEditorStyle.MainButton("出力"))
                {
                    this.cityImporter.Import(gmlFiles, config);
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
            if (!GmlSearcher.IsPathUdx(path)) return;
            this.gmlSearcher.GenerateFileDictionary(path);
            this.gmlSearcherGUI.OnUdxPathChanged();
        }
    }
}