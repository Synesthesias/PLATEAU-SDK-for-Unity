using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Runtime.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// 都市モデルをインポートするウィンドウです。
    /// 表示は <see cref="CityImportGUI"/> に委譲します。
    /// </summary>
    internal class CityImportWindow : EditorWindow
    {
        private bool isInitialized;
        private Vector2 scrollPosition;
        
        private CityImportGUI cityImportGUI;
        private CityImporterConfig importerConfig;

        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }
        

        private void Init()
        {
            this.cityImportGUI = new CityImportGUI();
            this.importerConfig = new CityImporterConfig();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.cityImportGUI.Draw(this.importerConfig);
            EditorGUILayout.EndScrollView();
        }
    }
}