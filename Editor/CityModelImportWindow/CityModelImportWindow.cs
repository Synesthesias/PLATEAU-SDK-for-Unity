using PlateauUnitySDK.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{

    /// <summary>
    /// 都市モデルをインポートするウィンドウです。
    /// 表示は <see cref="CityModelImportConfigGUI"/> に委譲します。
    /// </summary>
    public class CityModelImportWindow : EditorWindow
    {
        private bool isInitialized;
        private Vector2 scrollPosition;
        
        private CityModelImportConfigGUI cityModelImportConfigGUI;

        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityModelImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }
        

        private void Init()
        {
            this.cityModelImportConfigGUI = new CityModelImportConfigGUI();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.cityModelImportConfigGUI.Draw();
            EditorGUILayout.EndScrollView();
        }
    }
}