using PLATEAU.Editor.EditorWindowCommon;

namespace PLATEAU.Editor.PlateauWindow.MainTabGUI
{
    internal class CityAddGUI : IEditorDrawable
    {
        private int importTabIndex;
        private readonly IEditorDrawable[] importTabGUIArray = { new CityImportLocalGUI(), new CityImportRemoteGUI() };
        
        public void Draw()
        {
            HeaderDrawer.Draw("都市の追加");
            this.importTabIndex = PlateauEditorStyle.Tabs(this.importTabIndex, "ローカル", "リモート");
            this.importTabGUIArray[this.importTabIndex].Draw();
        }
    }
}
