using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Editor.PlateauWindow.MainTabGUI;

namespace PLATEAU.Editor.PlateauWindow
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray =
            {new CityAddGUI(), new CityVisualizeGUI(), new CityExportGUI()};
        private readonly string[] tabNames = { "追加", "可視化", "エクスポート" };

        public void Draw()
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);
            this.tabGUIArray[this.tabIndex].Draw();
        }
    }
}
