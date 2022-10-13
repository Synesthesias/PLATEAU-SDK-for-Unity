using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAddGUI : IEditorDrawable
    {
        private int importTabIndex;
        private readonly IEditorDrawable[] importTabGUIArray = { new CityImportLocalGUI(), new CityImportRemoteGUI() };
        
        public void Draw()
        {
            HeaderDrawer.Draw("都市の追加");
            this.importTabIndex = PlateauEditorStyle.Tabs(this.importTabIndex, "ローカル", "サーバー");
            this.importTabGUIArray[this.importTabIndex].Draw();
        }
    }
}
