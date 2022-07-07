using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.CityMeta;
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
        // private bool isInitialized;
        private Vector2 scrollPosition;
        
        private CityImportGUI cityImportGUI;
        // private CityImportConfig importConfig;

        [MenuItem("PLATEAU/都市モデルをインポート")]
        public static void Open()
        {
            var window = GetWindow<CityImportWindow>("都市モデルをインポート");
            window.Show();
            window.Init();
        }
        

        private void Init()
        {
            var importConfig = new CityImportConfig();
            importConfig.objConvertTypesConfig.SetLodRangeToAllRange(); // 初期値
            this.cityImportGUI = new CityImportGUI(importConfig);
            // this.isInitialized = true;
        }

        private void OnGUI()
        {
            // if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.cityImportGUI.Draw();
            EditorGUILayout.EndScrollView();
        }
    }
}