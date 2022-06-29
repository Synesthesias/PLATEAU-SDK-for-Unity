using System.IO;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.CityMeta;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// Plateau元データをインポートするためのGUIです。
    /// ユーザーが選択したインポート設定を <see cref="CityImporter"/> に渡して実行することでインポートが行われます。
    /// ユーザーが行う設定項目には、gmlファイル群を地物タイプや地域IDで絞り込む機能を含みます。
    ///
    /// このクラスを利用するクラスは、
    /// <see cref="CityImportWindow"/> および <see cref="CityMetaDataEditor"/> です。
    /// </summary>
    internal class CityImportGUI
    {
        private readonly InputFolderSelectorGUI udxFolderSelectorGUI;
        private readonly GmlSearcherGUI gmlSearcherGUI;
        private readonly GmlSearcher gmlSearcher;
        private readonly ObjConvertLodConfig objConvertLodConfig;
        private readonly ObjConvertLodGUI objConvertLodGUI;
        private readonly ScenePlacementGUI scenePlacementGUI;
        private readonly ExportFolderSelectorGUI exportFolderSelectorGUI;
        private readonly CityImporter cityImporter;
        
        public CityImportGUI(CityImporterConfig config)
        {
            this.udxFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSearcherGUI = new GmlSearcherGUI();
            this.gmlSearcher = new GmlSearcher();
            this.objConvertLodConfig = new ObjConvertLodConfig();
            this.objConvertLodGUI = new ObjConvertLodGUI();
            this.scenePlacementGUI = new ScenePlacementGUI();
            this.exportFolderSelectorGUI = new ExportFolderSelectorGUI();
            this.cityImporter = new CityImporter();
            
            // 記録されたインポート元パスを復元し、GUI画面の初期値に代入します。
            string loadedUdxPath = config.sourcePath.udxAssetPath;
            string initialUdxPath = loadedUdxPath;
            if (initialUdxPath.Replace('\\', '/').StartsWith("Assets/"))
            {
                initialUdxPath = PathUtil.AssetsPathToFullPath(initialUdxPath);
            }
            config.UdxPathBeforeImport = initialUdxPath;
        }

        public void Draw(CityImporterConfig config)
        {
            // udxフォルダ選択
            this.udxFolderSelectorGUI.FolderPath = config.UdxPathBeforeImport;
            string sourcePath = this.udxFolderSelectorGUI.Draw("udxフォルダ選択");
            config.UdxPathBeforeImport = sourcePath;

            // udxフォルダが選択されているなら、設定と出力のGUIを表示
            if (GmlSearcher.IsPathUdx(sourcePath))
            {
                // 案内
                if (!CityImporter.IsInStreamingAssets(sourcePath))
                {
                    EditorGUILayout.HelpBox($"入力フォルダは {PathUtil.FullPathToAssetsPath(PlateauUnityPath.StreamingGmlFolder)} にコピーされます。", MessageType.Info);
                }
                
                // 変換対象の絞り込み
                var gmlFiles = this.gmlSearcherGUI.Draw(this.gmlSearcher, ref config.gmlSearcherConfig);
                
                // 変換先パス設定
                config.importDestPath.dirAssetPath = this.exportFolderSelectorGUI.Draw(config.importDestPath.dirAssetPath);
                
                // 変換設定
                HeaderDrawer.Draw("変換設定");
                HeaderDrawer.IncrementDepth();
                HeaderDrawer.Draw("基本変換設定");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    config.optimizeFlag = EditorGUILayout.Toggle("最適化", config.optimizeFlag);
                    config.meshGranularity =
                        (MeshGranularity)EditorGUILayout.EnumPopup("オブジェクト分けの粒度", config.meshGranularity);
                    config.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", config.logLevel);
                }
                HeaderDrawer.Draw("LOD設定（3Dモデル変換）");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    this.objConvertLodGUI.Draw(this.objConvertLodConfig);
                }
                HeaderDrawer.DecrementDepth();
                
                // 配置設定
                HeaderDrawer.Draw("シーン配置設定");
                this.scenePlacementGUI.Draw(config.scenePlacementConfig);

                // 出力ボタン
                HeaderDrawer.Draw("出力");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    // 出力できない状況なら、エラーメッセージを表示してボタンを無効化します。
                    bool importReady = IsImportReady(config, out string message);
                    if (!string.IsNullOrEmpty(message))
                    {
                        EditorGUILayout.HelpBox(message, MessageType.Error);
                    }

                    using (new EditorGUI.DisabledScope(!importReady))
                    {
                        if (PlateauEditorStyle.MainButton("出力"))
                        {
                            this.cityImporter.Import(gmlFiles.ToArray(), config, out _);
                        }
                    }
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

        private bool IsImportReady(CityImporterConfig config, out string message)
        {
            message = "";
            var dirPath = config.importDestPath;
            if (string.IsNullOrEmpty(dirPath?.dirAssetPath))
            {
                message = "出力先を指定してください。";
                return false;
            }
            if (!Directory.Exists(dirPath.DirFullPath))
            {
                message = "出力先として指定されたフォルダが存在しません。";
                return false;
            }

            return true;
        }
    }
}