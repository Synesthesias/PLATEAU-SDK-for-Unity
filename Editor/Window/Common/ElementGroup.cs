using UnityEditor;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// 複数の<see cref="Element"/>をまとめます。
    /// また、まとめた要素から1つを返すための検索メソッドを提供します。
    /// </summary>
    internal class ElementGroup : Element
    {
        protected Element[] Elements { get; set; }
        private int indent;
        private BoxStyle boxStyle;

        /// <summary> スタイルなしで<see cref="ElementGroup"/>を作成します。 </summary>
        public ElementGroup(string name, int indent, params Element[] elements) :
            this(name, indent, BoxStyle.None, elements){}
        
        /// <summary> スタイル込みで<see cref="ElementGroup"/>を作成します。 </summary>
        public ElementGroup(string name, int indent, BoxStyle boxStyle, params Element[] elements) : base(name)
        {
            Elements = elements;
            this.indent = indent;
            this.boxStyle = boxStyle;
        }
        public override void DrawContent()
        {
            if(boxStyle == BoxStyle.None) EditorGUI.indentLevel += indent;
            using var scope = boxStyle switch
            {
                BoxStyle.VerticalStyleLevel1 => PlateauEditorStyle.VerticalScopeLevel1(indent),
                BoxStyle.VerticalStyleLevel2 => PlateauEditorStyle.VerticalScopeLevel2(),
                _ => null
            };
            
            // ここで描画します
            foreach (var d in Elements)
            {
                d.Draw();
            }

            if(boxStyle == BoxStyle.None) EditorGUI.indentLevel -= indent;
        }

        /// <summary>
        /// <see cref="Element"/>を型で検索して最初に合致したものを返します。
        /// <see cref="ElementGroup"/>の入れ子になっている場合は子も検索します。
        /// </summary>
        public T Get<T>() where T : Element
        {
            foreach (var d in Elements)
            {
                if (d is T ret) return ret;
            }

            foreach (var d in Elements)
            {
                if (d is ElementGroup child)
                {
                    var t = child.Get<T>();
                    if (t != null) return t;
                }
            }
            return null;
        }

        /// <summary>
        /// <see cref="Element"/>を名前で検索して最初に合致した者を返します。
        /// <see cref="ElementGroup"/>の入れ子になっている場合は子も検索します。
        /// </summary>
        public Element Get(string name)
        {
            foreach (var d in Elements)
            {
                if (d.Name == name) return d;
            }

            foreach (var d in Elements)
            {
                if (d is ElementGroup child)
                {
                    var n = child.Get(name);
                    if (n != null) return n;
                }
            }

            return null;
        }

        /// <summary>
        /// 型と名前の両方一致検索です。
        /// </summary>
        public T Get<T>(string name) where T : Element
        {
            foreach (var d in Elements)
            {
                if (d.Name == name && d is T ret) return ret;
            }

            foreach (var d in Elements)
            {
                if (d is ElementGroup child)
                {
                    var hit = child.Get<T>(name);
                    if (hit != null) return hit;
                }
            }

            return null;
        }

        public override void Reset()
        {
            foreach (var d in Elements)
            {
                d.Reset();
            }
        }

        public override void Dispose()
        {
            foreach (var d in Elements)
            {
                d.Dispose();
            }
        }
    }

    internal enum BoxStyle
    {
        None, VerticalStyleLevel1, VerticalStyleLevel2
    }
}