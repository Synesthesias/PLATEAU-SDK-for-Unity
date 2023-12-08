using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAddGUI : IEditorDrawable
    {
        private int importTabIndex;
        private readonly IEditorDrawable[] importTabGUIArray;

        public CityAddGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.importTabGUIArray = new IEditorDrawable[]
            {
                CityImportConfigGUI.CreateLocal(parentEditorWindow),
                CityImportConfigGUI.CreateRemote(parentEditorWindow) 
            };
        }
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのインポートを行います。");
            PlateauEditorStyle.Heading("都市の追加", PlateauEditorStyle.IconPathBuilding);
            PlateauEditorStyle.CenterAlignHorizontal(() =>
            {
                PlateauEditorStyle.LabelSizeFit(new GUIContent("インポート元"), new GUIStyle(EditorStyles.label));
            });
            this.importTabIndex = PlateauEditorStyle.Tabs(this.importTabIndex, "ローカル", "サーバー");
            this.importTabGUIArray[this.importTabIndex].Draw();
        }

        public void Dispose() {}

        /// <summary> テストで使う用です。 </summary>
        internal const string NameOfImportTabIndex = nameof(importTabIndex);
        internal const string NameOfImportTabGUIArray = nameof(importTabGUIArray);
    }
}
