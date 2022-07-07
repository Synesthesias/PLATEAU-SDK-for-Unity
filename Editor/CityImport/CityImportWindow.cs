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
        private Vector2 scrollPosition;
        private CityImportGUI cityImportGUI;

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
        }

        private void OnGUI()
        {
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.cityImportGUI.Draw();
            EditorGUILayout.EndScrollView();
        }
    }
}