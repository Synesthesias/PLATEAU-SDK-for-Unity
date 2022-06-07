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
        
        [MenuItem("Plateau/都市モデルインポート")]
        public static void Open()
        {
            var window = GetWindow<CityModelImportWindow>("都市モデルインポート");
            window.Show();
            window.Init();
        }

        private void Init()
        {
            this.udxFolderSelectorGUI = new UdxFolderSelectorGUI();
            this.isInitialized = true;
        }

        private void OnGUI()
        {
            if (!this.isInitialized) Init();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.udxFolderSelectorGUI.DrawGUI();
            EditorGUILayout.EndScrollView();
        }
    }
}