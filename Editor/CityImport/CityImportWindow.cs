using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityImport
{

    /// <summary>
    /// 都市モデルをインポートするウィンドウです。
    /// 表示は <see cref="CityImportConfigGUI"/> に委譲します。
    /// </summary>
    public class CityImportWindow : EditorWindow
    {
        private bool isInitialized;
        private Vector2 scrollPosition;
        
        private CityImportConfigGUI cityImportConfigGUI;
        private CityModelImportConfig importConfig;

        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }
        

        private void Init()
        {
            this.cityImportConfigGUI = new CityImportConfigGUI();
            this.importConfig = new CityModelImportConfig();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.cityImportConfigGUI.Draw(this.importConfig);
            EditorGUILayout.EndScrollView();
        }
    }
}