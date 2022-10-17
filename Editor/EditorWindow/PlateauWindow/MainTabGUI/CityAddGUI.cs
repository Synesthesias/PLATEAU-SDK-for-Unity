using PLATEAU.Editor.EditorWindow.Common;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAddGUI : IEditorDrawable
    {
        private int importTabIndex;
        private readonly IEditorDrawable[] importTabGUIArray = { new CityImportLocalGUI(), new CityImportRemoteGUI() };
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのインポートを行います。");
            PlateauEditorStyle.Heading("都市の追加", PlateauEditorStyle.IconPathBuilding);
            this.importTabIndex = PlateauEditorStyle.Tabs(this.importTabIndex, "ローカル", "サーバー");
            this.importTabGUIArray[this.importTabIndex].Draw();
        }
    }
}
