using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAU SDK ウィンドウのエントリーポイントです。
    /// </summary>
    internal class PlateauWindow : UnityEditor.EditorWindow
    {
        private ScrollView scrollView = new ScrollView();
        private PlateauWindowGUI gui;

        [MenuItem("PLATEAU/PLATEAU SDK")]
        public static void Open()
        {
            var window = GetWindow<PlateauWindow>("PLATEAU SDK");
            window.Show();
        }

        private void OnGUI()
        {
            gui ??= new PlateauWindowGUI(this);
            PlateauEditorStyle.SetCurrentWindow(this);
            scrollView.Draw(
                gui.Draw
            );
        }

        /// <summary> テストからアクセスする用 </summary>
        internal const string NameOfInnerGuiField = nameof(gui);

        private void OnDestroy()
        {
            gui.Dispose();
        }
    }
}
