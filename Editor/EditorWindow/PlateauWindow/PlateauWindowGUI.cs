using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class PlateauWindowGUI
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray =
            {new CityAddGUI(), new CityVisualizeGUI(), new CityExportGUI()};
        private readonly string[] tabNames = { "追加", "可視化", "エクスポート" };

        public void Draw(UnityEditor.EditorWindow window)
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);
            PlateauEditorStyle.MainLogo(window.position.size);
            this.tabGUIArray[this.tabIndex].Draw();
        }
    }
}
