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
        private GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
        private UdxConverter udxConverter;
        private GmlSelectorGUI gmlSelectorGUI;
        private UdxFolderSelectorGUI udxFolderSelectorGUI;
        private CityModelExportPathSelectorGUI cityModelExportPathSelectorGUI;
        
        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityModelImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }

        private void Init()
        {
            this.udxFolderSelectorGUI = new UdxFolderSelectorGUI(OnUdxPathChanged);
            this.gmlSelectorGUI = new GmlSelectorGUI(this.gmlFileSearcher);
            this.cityModelExportPathSelectorGUI = new CityModelExportPathSelectorGUI();
            this.gmlFileSearcher = new GmlFileSearcher();
            this.udxConverter = new UdxConverter();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            // udxフォルダの選択GUIを表示します。
            string udxFolderPath = this.udxFolderSelectorGUI.Draw();
            if (GmlFileSearcher.IsPathUdx(udxFolderPath))
            {
                // udxフォルダが選択されているなら、設定と出力のGUIを表示します。
                var gmlFiles = this.gmlSelectorGUI.Draw();
                var exportFolderPath = this.cityModelExportPathSelectorGUI.Draw(gmlFiles, udxFolderPath);
                HeaderDrawer.Draw("出力");
                if (PlateauEditorStyle.MainButton("出力"))
                {
                    this.udxConverter.Convert(gmlFiles, udxFolderPath, exportFolderPath);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("udxフォルダが選択されていません。", MessageType.Error);
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnUdxPathChanged(string path)
        {
            this.gmlFileSearcher.GenerateFileDictionary(path);
            this.gmlSelectorGUI.OnUdxPathChanged();
        }
    }
}