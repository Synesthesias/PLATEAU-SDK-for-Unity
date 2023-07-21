using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        public int tabIndex { get; private set; }
        private readonly IEditorDrawable[] tabGUIArray;

        private readonly string[] tabImages =
            { "dark_icon_import.png", "dark_icon_adjust.png", "dark_icon_export.png", "dark_icon_information.png" };

        public PlateauWindowGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.tabGUIArray = new IEditorDrawable[]
            {
                new CityAddGUI(parentEditorWindow),
                new CityAdjustGUI(),
                new CityExportGUI(),
                new CityAttributeGUI(parentEditorWindow, this)
            };
        }

        public void Draw()
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            this.tabIndex = PlateauEditorStyle.TabWithImages(this.tabIndex, this.tabImages, 80);
            PlateauEditorStyle.MainLogo();
            this.tabGUIArray[this.tabIndex].Draw();
        }

        /// <summary> テストからアクセスする用 </summary>
        internal const string NameOfTabIndex = nameof(tabIndex);

        internal const string NameOfTabGUIArray = nameof(tabGUIArray);
    }
}
