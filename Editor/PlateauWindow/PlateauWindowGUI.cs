using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Runtime.CityLoader.AreaSelector.MainTabGUI;

namespace PLATEAU.Runtime.CityLoader.AreaSelector
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray =
            {new CityAddGUI(), new CityVisualizeGUI(), new CityExportGUI()};

        public void Draw()
        {
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, "追加", "可視化", "エクスポート");
            this.tabGUIArray[this.tabIndex].Draw();
        }
    }
}
