using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    /// <summary>
    /// PLATEAU SDK ウィンドウのエントリーポイントです。
    /// </summary>
    internal class PlateauWindow : UnityEditor.EditorWindow
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
            this.gui ??= new PlateauWindowGUI();
            HeaderDrawer.Reset();
            using var scrollView = new EditorGUILayout.ScrollViewScope(this.scrollPosition);
            this.scrollPosition = scrollView.scrollPosition;
            this.gui.Draw();
        }
    }
}
