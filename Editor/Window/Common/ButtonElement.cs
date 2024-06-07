using System;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common
{
    /// <summary>
    /// ボタンを<see cref="Element"/>でラップしたクラスです。
    /// </summary>
    internal class ButtonElement : Element
    {
        private readonly Action onClick;
        private readonly string buttonText;
        private Vector4 padding = Vector4.zero; // left, right, top, bottom

        public ButtonElement(string name, string buttonText, Action onClick) : base(name)
        {
            this.buttonText = buttonText;
            this.onClick = onClick;
        }

        public ButtonElement(string name, string buttonText, Vector4 padding, Action onClick) : this(name, buttonText, onClick)
        {
            this.padding = padding;
        }

        public override void DrawContent()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding((int)padding.x, (int)padding.y, (int)padding.z,
                       (int)padding.w))
            {
                if (PlateauEditorStyle.MainButton(buttonText))
                {
                    onClick();
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}