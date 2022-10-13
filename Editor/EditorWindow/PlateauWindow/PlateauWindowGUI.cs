﻿using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray =
            {new CityAddGUI(), new CityVisualizeGUI(), new CityExportGUI()};
        private readonly string[] tabNames = { "追加", "モデル調整", "エクスポート" };

        private readonly string[] tabImages =
            { "dark_icon_import.png", "dark_icon_adjust.png", "dark_icon_export.png" };

        public void Draw()
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            // this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);
            // this.tabIndex = PlateauEditorStyle.TabWithImages(this.tabIndex, this.tabNames, this.tabImages);
            this.tabIndex = PlateauEditorStyle.Tabs(this.tabIndex, this.tabNames);
            PlateauEditorStyle.MainLogo();
            this.tabGUIArray[this.tabIndex].Draw();
        }
    }
}
