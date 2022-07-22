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
    /// このGUIでユーザーが選択したインポート設定を <see cref="CityImporterModel"/> に渡して実行することでインポートが行われます。
    ///
    /// このクラスを利用するクラスは、
    /// <see cref="CityImportWindow"/> および <see cref="CityMetadataEditor"/> です。
    /// </summary>
    internal class CityImporterView : IInputPathChangedEventListener
    {
        private readonly InputFolderSelectorGUI importFolderSelectorGUI;
        private readonly GmlSearcherPresenter gmlSearcherPresenter;
        private readonly ObjConvertTypesGUI objConvertTypesGUI;
        private readonly ExportFolderSelectorGUI exportFolderSelectorGUI;
        
        public CityImporterView(CityImportConfig importConfig)
        {
            this.importFolderSelectorGUI = new InputFolderSelectorGUI(this);
            this.gmlSearcherPresenter = new GmlSearcherPresenter(importConfig.gmlSearcherConfig);
            this.objConvertTypesGUI = new ObjConvertTypesGUI();
            this.exportFolderSelectorGUI = new ExportFolderSelectorGUI();
        }

        public void Draw(CityImporterPresenter presenter, CityImportConfig importConfig)
        {
            // インポート元フォルダ選択
            string sourcePath = this.importFolderSelectorGUI.Draw("インポート元フォルダ選択", importConfig.SrcRootPathBeforeImport);
            importConfig.SrcRootPathBeforeImport = sourcePath;

            // udxフォルダが選択されているなら、設定と出力のGUIを表示
            if (this.importFolderSelectorGUI.IsPlateauFolderSelected())
            {
                // 案内
                this.importFolderSelectorGUI.DisplayGuidanceAboutCopy();
                
                // 変換対象の絞り込み
                var gmlFiles = this.gmlSearcherPresenter.Draw(importConfig.gmlSearcherConfig);

                // 変換設定
                HeaderDrawer.Draw("メッシュ設定");
                HeaderDrawer.IncrementDepth();
                HeaderDrawer.Draw("基本メッシュ設定");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    importConfig.exportAppearance = EditorGUILayout.Toggle("テクスチャを含める", importConfig.exportAppearance);
                    importConfig.meshGranularity = (MeshGranularity)EditorGUILayout.Popup("メッシュ結合単位", (int)importConfig.meshGranularity,
                        new [] { "最小地物単位", "主要地物単位", "都市モデル地域単位" });
                    importConfig.logLevel = (DllLogLevel)EditorGUILayout.EnumPopup("(開発者向け)ログの詳細度", importConfig.logLevel);
                }
                HeaderDrawer.Draw("地物タイプ別 メッシュ設定");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    this.objConvertTypesGUI.Draw(importConfig.objConvertTypesConfig, importConfig.gmlSearcherConfig);
                }
                
                // 変換先パス設定
                importConfig.importDestPath.DirAssetsPath = this.exportFolderSelectorGUI.Draw(importConfig.importDestPath.DirAssetsPath);
                
                HeaderDrawer.DecrementDepth();
                
                // 出力ボタン
                HeaderDrawer.Draw("出力");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    // 出力できない状況なら、エラーメッセージを表示してボタンを無効化します。
                    bool importReady = IsImportReady(importConfig, out string message);
                    if (!string.IsNullOrEmpty(message))
                    {
                        EditorGUILayout.HelpBox(message, MessageType.Error);
                    }

                    using (new EditorGUI.DisabledScope(!importReady))
                    {
                        if (PlateauEditorStyle.MainButton("出力"))
                        {
                            // インポート開始します。
                            presenter.Import(gmlFiles.ToArray(), out _);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Plateauフォルダが選択されていません。（直下に udx という名前のフォルダを含むフォルダを選択してください。）", MessageType.Error);
            }
        }
        
        /// <summary>
        /// udxフォルダパス選択GUIで、新しいパスが指定されたときに呼ばれます。
        /// </summary>
        public void OnInputPathChanged(string path, InputFolderSelectorGUI.PathChangeMethod changeMethod)
        {
            if (this.gmlSearcherPresenter == null)
            {
                Debug.LogError($"{nameof(this.gmlSearcherPresenter)} is null.");
                return;
            }
            this.gmlSearcherPresenter.OnImportSrcPathChanged(path, changeMethod);
        }

        private static bool IsImportReady(CityImportConfig config, out string message)
        {
            message = "";
            var dirPath = config.importDestPath;
            if (string.IsNullOrEmpty(dirPath?.DirAssetsPath))
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