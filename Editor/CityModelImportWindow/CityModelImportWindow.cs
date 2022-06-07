using PlateauUnitySDK.Editor.EditorWindowCommon;
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
        private UdxFolderSelectorGUI udxFolderSelectorGUI;
        private bool isInitialized;
        private Vector2 scrollPosition;
        private GmlSelectorGUI gmlSelectorGUI;
        private readonly GmlFileSearcher gmlFileSearcher = new GmlFileSearcher();
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
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            string udxFolderPath = this.udxFolderSelectorGUI.Draw();
            if (GmlFileSearcher.IsPathUdx(udxFolderPath))
            {
                var gmlFiles = this.gmlSelectorGUI.Draw();
                this.cityModelExportPathSelectorGUI.Draw(gmlFiles, udxFolderPath);
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