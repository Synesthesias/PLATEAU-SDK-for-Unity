using UnityEngine.Serialization;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// 折りたたみGUIを表示します。
    /// 開いている場合は、子の<see cref="Element"/>を表示します。
    /// </summary>
    internal class FoldOutElement : Element
    {
        private readonly string label;
        private bool foldOutState = true;
        public ElementGroup ChildElementGroup { get; private set; }
        
        public FoldOutElement(string name, string label, params Element[] childElements) : base(name)
        {
            this.label = label;
            this.ChildElementGroup = new ElementGroup("", childElements);
        }
        
        public override void DrawContent()
        {
            foldOutState = PlateauEditorStyle.FoldOut(foldOutState, label, () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    ChildElementGroup.Draw();
                }
            });
        }

        public override void Dispose()
        {
        }
    }
}