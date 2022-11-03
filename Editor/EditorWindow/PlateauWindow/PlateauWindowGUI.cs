﻿using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray;
        private readonly string[] tabNames = { "追加", "モデル調整", "エクスポート" };

        private readonly string[] tabImages =
            { "dark_icon_import.png", "dark_icon_adjust.png", "dark_icon_export.png" };


        public PlateauWindowGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.tabGUIArray = new IEditorDrawable[]
            {
                new CityAddGUI(parentEditorWindow),
                new CityVisualizeGUI(),
                new CityExportGUI()
            };
        }

        public void Draw()
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            this.tabIndex = PlateauEditorStyle.TabWithImages(this.tabIndex, this.tabImages, 80);
            PlateauEditorStyle.MainLogo();
            this.tabGUIArray[this.tabIndex].Draw();
        }
    }
}
