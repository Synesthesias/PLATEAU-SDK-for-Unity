using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts;
using PLATEAU.Util;
using System;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts
{
    /// <summary>
    /// 属性情報キーを入力するGUIです。
    /// </summary>
    internal class AttributeKeyGui : Element, IAttrKeySelectResultReceiver
    {
        private string attrKey;
        private string AttrKey
        {
            get
            {
                return attrKey;
            }
            set
            {
                bool isChanged = attrKey != value;                
                attrKey = value;
                if (isChanged)
                {
                    onAttrKeyChanged?.Invoke(value);
                }
            }
        }

        private EditorWindow parentWindow;
        private Action<string> onAttrKeyChanged;
        private Func<UniqueParentTransformList> selectedObjectsGetter;

        public AttributeKeyGui(EditorWindow parentWindow,Func<UniqueParentTransformList> selectedObjectsGetter, Action<string> onAttrKeyChanged)
        {
            this.selectedObjectsGetter = selectedObjectsGetter;
            this.onAttrKeyChanged = onAttrKeyChanged;
            this.parentWindow = parentWindow;
            AttrKey = "";
            onAttrKeyChanged?.Invoke(AttrKey);
        }
            
        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 100;
                if (PlateauEditorStyle.MainButton("属性情報キーを選択"))
                {
                    var window = AttrKeySelectWindow.Open();
                    window.Init(this, selectedObjectsGetter(), parentWindow);
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