using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;
using System;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class PlateauWindowGUI : IEditorDrawable
    {
        public Action<int> OnTabChange;
        private int TabIndex { get => this.tabIndex; 
            set { 
                if(value != this.tabIndex)
                    OnTabChange?.Invoke(value);  
                this.tabIndex = value;
            } 
        }
        private int tabIndex;
        private readonly IEditorDrawable[] tabGUIArray;
       
        private readonly string[] tabImages =
            { "dark_icon_import.png", "dark_icon_modify.png", "dark_icon_export.png", "dark_icon_information.png" };

        public PlateauWindowGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.tabGUIArray = new IEditorDrawable[]
            {
                new CityAddGUI(parentEditorWindow),
                new CityModificationFrameGUI(parentEditorWindow),
                new CityExportGUI(),
                new CityAttributeGUI(parentEditorWindow, this)
            };
        }

        public void Draw()
        {
            // ウィンドウのメインとなるタブ選択GUIを表示し、選択中のタブGUIクラスに描画処理を委譲します。
            this.TabIndex = PlateauEditorStyle.TabWithImages(this.TabIndex, this.tabImages, 80);
            PlateauEditorStyle.MainLogo();
            this.tabGUIArray[this.TabIndex].Draw();
        }

        public void Dispose()
        {
            foreach (var gui in tabGUIArray)
                gui.Dispose();
        }

        /// <summary> テストからアクセスする用 </summary>
        internal const string NameOfTabIndex = nameof(tabIndex);

        internal const string NameOfTabGUIArray = nameof(tabGUIArray);
    }
}
