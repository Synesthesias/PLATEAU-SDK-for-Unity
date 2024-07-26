namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// 折りたたみGUIを表示します。
    /// 開いている場合は、子の<see cref="Element"/>を表示します。
    /// </summary>
    internal class FoldOutElement : Element
    {
        private readonly string label;
        private bool foldOutState;
        public ElementGroup ChildElementGroup { get; private set; }
        
        public FoldOutElement(string name, string label, bool initialFoldOutState, params Element[] childElements) : base(name)
        {
            this.label = label;
            this.foldOutState = initialFoldOutState;
            this.ChildElementGroup = new ElementGroup("", 0, childElements);
        }
        
        public override void DrawContent()
        {
            foldOutState = PlateauEditorStyle.FoldOut(foldOutState, label, () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1(1))
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