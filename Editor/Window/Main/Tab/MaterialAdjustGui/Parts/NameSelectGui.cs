using PLATEAU.Editor.Window.Common;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 名前を入力するGUIです。
    /// </summary>
    internal class NameSelectGui : Element
    {
        public string EnteredName { get; private set; }
        private readonly string labelText;

        public NameSelectGui(string labelText, string defaultValue)
        {
            EnteredName = defaultValue;
            this.labelText = labelText;
        }
        
        public override void DrawContent()
        {
            EnteredName = EditorGUILayout.TextField(labelText, EnteredName);
        }
        public override void Reset() { }
        public override void Dispose(){}
    }
}