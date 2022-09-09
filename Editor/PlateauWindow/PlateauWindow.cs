using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.PlateauWindow
{
    /// <summary>
    /// PLATEAU SDK ウィンドウのエントリーポイントです。
    /// </summary>
    internal class PlateauWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private PlateauWindowGUI gui;

        [MenuItem("PLATEAU/PLATEAU SDK ウィンドウ")]
        public static void Open()
        {
            var window = GetWindow<PlateauWindow>("PLATEAU SDK");
            window.Show();
        }

        private void OnGUI()
        {
            if (this.gui == null) this.gui = new PlateauWindowGUI();
            HeaderDrawer.Reset();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            this.gui.Draw();
            EditorGUILayout.EndScrollView();
        }
    }
}
