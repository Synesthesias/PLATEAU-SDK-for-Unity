using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// 都市モデルをインポートするウィンドウです。
    /// udxフォルダを指定し、条件に合うgmlファイルを一括で変換するGUIを提供します。
    /// </summary>
    public class CityModelImportWindow : EditorWindow
    {
        private bool isInitialized;
        private Vector2 scrollPosition;
        
        // 機能を提供するクラス
        private GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
        private MultiGmlConverter multiGmlConverter;
        
        // GUI描画を委譲するクラス
        private GmlSelectorGUI gmlSelectorGUI;
        private InputFolderSelectorGUI inputFolderSelectorGUI;
        private ExportFolderPathSelectorGUI exportFolderPathSelectorGUI;
        
        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityModelImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }

        private void Init()
        {
            this.inputFolderSelectorGUI = new InputFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSelectorGUI = new GmlSelectorGUI();
            this.exportFolderPathSelectorGUI = new ExportFolderPathSelectorGUI();
            this.gmlFileSearcher = new GmlFileSearcher();
            this.multiGmlConverter = new MultiGmlConverter();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            // udxフォルダの選択GUIを表示します。
            string udxFolderPath = this.inputFolderSelectorGUI.Draw("udxフォルダ選択");
            if (GmlFileSearcher.IsPathUdx(udxFolderPath))
            {
                // udxフォルダが選択されているなら、設定と出力のGUIを表示します。
                var gmlFiles = this.gmlSelectorGUI.Draw(this.gmlFileSearcher);
                var exportFolderPath = this.exportFolderPathSelectorGUI.Draw();
                HeaderDrawer.Draw("出力");
                if (PlateauEditorStyle.MainButton("出力"))
                {
                    this.multiGmlConverter.Convert(gmlFiles, udxFolderPath, exportFolderPath);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// udxフォルダパス選択GUIで、新しいパスが指定されたときに呼ばれます。
        /// </summary>
        private void OnUdxPathChanged(string path)
        {
            this.gmlFileSearcher.GenerateFileDictionary(path);
            this.gmlSelectorGUI.OnUdxPathChanged(this.gmlFileSearcher);
        }
    }
}