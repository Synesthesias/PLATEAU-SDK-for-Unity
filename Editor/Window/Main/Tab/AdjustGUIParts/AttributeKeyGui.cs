using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts
{
    /// <summary>
    /// 属性情報キーを入力するGUIです。
    /// </summary>
    internal class AttributeKeyGui : Element, IAttrKeySelectResultReceiver
    {
        public string AttrKey { get; private set; }= "";
        public CityMaterialAdjustGUI adjustGui;
        private EditorWindow parentWindow;

        public AttributeKeyGui(CityMaterialAdjustGUI adjustGui, EditorWindow parentWindow)
        {
            this.adjustGui = adjustGui;
            this.parentWindow = parentWindow;
        }
            
        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 100;
                if (PlateauEditorStyle.MainButton("属性情報キーを選択"))
                {
                    AttrKeySelectWindow.Open(this, adjustGui.SelectedObjects, parentWindow);
                }
                AttrKey = EditorGUILayout.TextField("属性情報キー", AttrKey);
            }
        }
        
        public void ReceiveAttrKeySelectResult(string selectedAttrKey)
        {
            AttrKey = selectedAttrKey;
        }

        public override void Dispose()
        {
            
        }
        
    }
}