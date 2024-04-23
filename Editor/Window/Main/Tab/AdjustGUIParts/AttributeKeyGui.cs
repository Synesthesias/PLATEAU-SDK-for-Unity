using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.AdjustGUIParts
{
    /// <summary>
    /// 属性情報キーを入力するGUIです。
    /// </summary>
    internal class AttributeKeyGui : Element
    {
        public string AttrKey { get; private set; }= "";

        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 100;
                AttrKey = EditorGUILayout.TextField("属性情報キー", AttrKey);
            }
        }

        public override void Dispose()
        {
            
        }
        
    }
}