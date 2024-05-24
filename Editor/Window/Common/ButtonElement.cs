using System;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// ボタンを<see cref="Element"/>でラップしたクラスです。
    /// </summary>
    internal class ButtonElement : Element
    {
        private readonly Action onClick;
        private readonly string buttonText;

        public ButtonElement(string name, string buttonText, Action onClick) : base(name)
        {
            this.buttonText = buttonText;
            this.onClick = onClick;
        }
        
        public override void DrawContent()
        {
            if (PlateauEditorStyle.MainButton(buttonText))
            {
                onClick();
            }
        }

        public override void Dispose()
        {
        }
    }
}